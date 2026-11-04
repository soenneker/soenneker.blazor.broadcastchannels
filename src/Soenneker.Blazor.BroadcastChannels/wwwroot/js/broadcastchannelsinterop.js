const channels = new Map();
export function isSupported() {
    return typeof globalThis.BroadcastChannel === "function";
}
export function open(id, name, receiver) {
    if (!isSupported()) throw new Error("BroadcastChannel is unavailable in this browser context.");
    if (channels.has(id)) throw new Error("Duplicate channel ID.");
    const channel = new BroadcastChannel(name);
    const entry = { channel, closed: false, pending: Promise.resolve() };
    const enqueue = callback => {
        entry.pending = entry.pending.then(() => {
            if (!entry.closed) return callback();
        }).catch(error => console.error("BroadcastChannel callback failed", error));
    };
    channel.onmessage = event => enqueue(() =>
        typeof event.data === "string"
            ? receiver.invokeMethodAsync("OnMessage", event.data)
            : receiver.invokeMethodAsync("OnError", "Expected a JSON string from a compatible channel sender."));
    channel.onmessageerror = () => enqueue(() =>
        receiver.invokeMethodAsync("OnError", "The browser could not deserialize a channel message."));
    channels.set(id, entry);
}
export function post(id, json) {
    const entry = channels.get(id);
    if (!entry || entry.closed) throw new Error("The channel is closed.");
    entry.channel.postMessage(json);
}
export function close(id) {
    const entry = channels.get(id);
    if (!entry) return;
    entry.closed = true;
    entry.channel.onmessage = null;
    entry.channel.onmessageerror = null;
    entry.channel.close();
    channels.delete(id);
    // Never await pending callbacks: a callback is allowed to close its own channel.
}