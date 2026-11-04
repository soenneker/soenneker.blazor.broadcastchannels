using Soenneker.Atomics.ValueBools;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Soenneker.Blazor.Utils.ModuleImport.Abstract;
using Soenneker.Blazor.BroadcastChannels.Abstract;

namespace Soenneker.Blazor.BroadcastChannels;

public sealed class BroadcastChannelsInterop(IModuleImportUtil modules) : IBroadcastChannelsInterop
{
    private const string ModulePath = "./_content/Soenneker.Blazor.BroadcastChannels/js/broadcastchannelsinterop.js";
    private readonly ConcurrentDictionary<string, IAsyncDisposable> _channels = new();
    private readonly CancellationTokenSource _lifetime = new();
    private ValueAtomicBool _disposed;
    private readonly object _disposeSync = new();
    private Task? _disposeTask;
    private async ValueTask<IJSObjectReference> Module(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed.Value, this);
        return await modules.GetContentModuleReference(ModulePath, cancellationToken);
    }
    public async ValueTask Initialize(CancellationToken cancellationToken = default) => _ = await Module(cancellationToken);
    public async ValueTask<bool> IsSupported(CancellationToken cancellationToken = default) =>
        await (await Module(cancellationToken)).InvokeAsync<bool>("isSupported", cancellationToken);
    public async ValueTask<IBroadcastChannel<T>> Open<T>(string name, Func<T, Task> onMessage, JsonTypeInfo<T> typeInfo,
        Func<Exception, Task>? onError = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(onMessage);
        ArgumentNullException.ThrowIfNull(typeInfo);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _lifetime.Token);
        IJSObjectReference module = await Module(linked.Token);
        var channel = new BroadcastChannel<T>(name, module, onMessage, typeInfo, onError, id => _channels.TryRemove(id, out _));
        _channels.TryAdd(channel.Id, channel);
        try
        {
            try
            {
                linked.Token.ThrowIfCancellationRequested();
                // Registration must finish before cancellation cleanup can close the JS object.
                await module.InvokeVoidAsync("open", channel.Id, name, channel.Reference);
            }
            finally { channel.Registered.TrySetResult(); }
            linked.Token.ThrowIfCancellationRequested();
            return channel;
        }
        catch
        {
            await channel.DisposeAsync();
            throw;
        }
    }
    public ValueTask DisposeAsync()
    {
        lock (_disposeSync)
            return new ValueTask(_disposeTask ??= DisposeCore());
    }

    private async Task DisposeCore()
    {
        if (!_disposed.TrySetTrue())
            return;
        await _lifetime.CancelAsync();
        try { await Task.WhenAll(_channels.Values.Select(item => item.DisposeAsync().AsTask())); }
        finally { _lifetime.Dispose(); }
    }
}