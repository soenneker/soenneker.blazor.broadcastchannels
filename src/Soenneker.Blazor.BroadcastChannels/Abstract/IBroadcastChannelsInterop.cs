using System;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Blazor.BroadcastChannels.Abstract;
/// <summary>Opens independently disposable, typed browser channels. Requires interactive rendering.</summary>
public interface IBroadcastChannelsInterop : IAsyncDisposable
{
    /// <summary>Loads the JavaScript module after interactive rendering.</summary>
    ValueTask Initialize(CancellationToken cancellationToken = default);
    /// <summary>Reports whether BroadcastChannel is available in the current context.</summary>
    ValueTask<bool> IsSupported(CancellationToken cancellationToken = default);
    /// <summary>Opens a channel using source-generated JSON metadata. Messages are processed in arrival order.
    /// Deserialization and handler failures are sent to onError; when omitted they are reported by the browser console.
    /// Canceling this operation does not close an already returned channel; dispose the handle to close it.
    /// Dispose prevents new callbacks; an already executing callback may finish.</summary>
    ValueTask<IBroadcastChannel<T>> Open<T>(string name, Func<T, Task> onMessage, JsonTypeInfo<T> typeInfo,
        Func<Exception, Task>? onError = null, CancellationToken cancellationToken = default);
}