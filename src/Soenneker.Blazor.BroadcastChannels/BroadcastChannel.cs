using Soenneker.Atomics.ValueBools;
using System;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Soenneker.Blazor.BroadcastChannels.Abstract;

namespace Soenneker.Blazor.BroadcastChannels;

internal sealed class BroadcastChannel<T> : IBroadcastChannel<T>
{
    private readonly IJSObjectReference _module;
    private readonly Func<T, Task> _onMessage;
    private readonly Func<Exception, Task>? _onError;
    private readonly JsonTypeInfo<T> _typeInfo;
    private readonly Action<string> _remove;
    private ValueAtomicBool _disposed;
    private readonly object _disposeSync = new();
    private Task? _disposeTask;
    internal string Id { get; } = Guid.NewGuid().ToString();
    internal DotNetObjectReference<BroadcastChannel<T>> Reference { get; }
    internal TaskCompletionSource Registered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public string Name { get; }
    internal BroadcastChannel(string name, IJSObjectReference module, Func<T, Task> onMessage, JsonTypeInfo<T> typeInfo,
        Func<Exception, Task>? onError, Action<string> remove)
    {
        Name = name;
        _module = module;
        _onMessage = onMessage;
        _typeInfo = typeInfo;
        _onError = onError;
        _remove = remove;
        Reference = DotNetObjectReference.Create(this);
    }
    public async ValueTask Post(T message, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed.Value, this);
        cancellationToken.ThrowIfCancellationRequested();
        string json = JsonSerializer.Serialize(message, _typeInfo);
        await _module.InvokeVoidAsync("post", cancellationToken, Id, json);
    }
    [JSInvokable]
    public async Task OnMessage(string json)
    {
        if (_disposed.Value)
            return;
        try
        {
            await _onMessage(JsonSerializer.Deserialize(json, _typeInfo)!);
        }
        catch (Exception exception)
        {
            if (_onError is null) throw;
            await _onError(exception);
        }
    }
    [JSInvokable]
    public Task OnError(string message) => _disposed.Value ? Task.CompletedTask :
        _onError?.Invoke(new InvalidOperationException(message)) ?? Task.FromException(new InvalidOperationException(message));
    public ValueTask DisposeAsync()
    {
        lock (_disposeSync)
            return new ValueTask(_disposeTask ??= DisposeCore());
    }

    private async Task DisposeCore()
    {
        if (!_disposed.TrySetTrue())
            return;
        try
        {
            await Registered.Task;
            await _module.InvokeVoidAsync("close", Id);
        }
        catch (JSDisconnectedException) { }
        finally
        {
            Reference.Dispose();
            _remove(Id);
        }
    }
}