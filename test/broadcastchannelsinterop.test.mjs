import test from "node:test";
import assert from "node:assert/strict";
import * as channels from "../src/Soenneker.Blazor.BroadcastChannels/wwwroot/js/broadcastchannelsinterop.js";

const delay = ms => new Promise(resolve => setTimeout(resolve, ms));
function open(name, handler) {
    const id = crypto.randomUUID();
    channels.open(id, name, { invokeMethodAsync: handler });
    return { id, close: () => channels.close(id), post: value => channels.post(id, JSON.stringify(value)) };
}

test("delivers JSON in order, never echoes to sender, and isolates channel names", async () => {
    const name = crypto.randomUUID();
    const seen = [];
    let echoed = false;
    let crossed = false;
    const done = Promise.withResolvers();
    const sender = open(name, async () => { echoed = true; });
    const receiver = open(name, async (method, json) => {
        assert.equal(method, "OnMessage");
        const value = JSON.parse(json);
        if (value === 1) await delay(15);
        seen.push(value);
        if (value === 2) done.resolve();
    });
    const other = open(crypto.randomUUID(), async () => { crossed = true; });
    try {
        sender.post(1);
        sender.post(2);
        await done.promise;
        assert.deepEqual(seen, [1, 2]);
        assert.equal(echoed, false);
        assert.equal(crossed, false);
    } finally { sender.close(); receiver.close(); other.close(); }
});

test("a callback can close its own channel and discard queued messages", async () => {
    const name = crypto.randomUUID();
    const done = Promise.withResolvers();
    let count = 0;
    const sender = open(name, async () => {});
    const receiver = open(name, async () => {
        count++;
        receiver.close();
        done.resolve();
    });
    try {
        sender.post("first");
        sender.post("second");
        await done.promise;
        await delay(20);
        assert.equal(count, 1);
        assert.throws(() => receiver.post("closed"), /closed/);
        receiver.close();
    } finally { sender.close(); receiver.close(); }
});

test("callback errors do not poison later messages", async () => {
    const name = crypto.randomUUID();
    const done = Promise.withResolvers();
    let count = 0;
    const sender = open(name, async () => {});
    const receiver = open(name, async () => {
        if (++count === 1) throw new Error("expected test failure");
        done.resolve();
    });
    try {
        sender.post(1);
        sender.post(2);
        await done.promise;
        assert.equal(count, 2);
    } finally { sender.close(); receiver.close(); }
});

test("incompatible payloads use the error callback", async () => {
    const name = crypto.randomUUID();
    const done = Promise.withResolvers();
    const receiver = open(name, async (method, text) => {
        assert.equal(method, "OnError");
        assert.match(text, /JSON string/);
        done.resolve();
    });
    const native = new BroadcastChannel(name);
    try { native.postMessage({ unexpected: true }); await done.promise; }
    finally { receiver.close(); native.close(); }
});
