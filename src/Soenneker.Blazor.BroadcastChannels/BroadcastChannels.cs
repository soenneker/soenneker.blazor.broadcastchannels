using System;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Blazor.BroadcastChannels.Abstract;

namespace Soenneker.Blazor.BroadcastChannels;

public sealed class BroadcastChannels(IBroadcastChannelsInterop interop) : IBroadcastChannels
{
    public ValueTask Initialize(CancellationToken cancellationToken = default) => interop.Initialize(cancellationToken);
    public ValueTask<bool> IsSupported(CancellationToken cancellationToken = default) => interop.IsSupported(cancellationToken);
    public ValueTask<IBroadcastChannel<T>> Open<T>(string name, Func<T, Task> onMessage, JsonTypeInfo<T> typeInfo,
        Func<Exception, Task>? onError = null, CancellationToken cancellationToken = default) =>
        interop.Open(name, onMessage, typeInfo, onError, cancellationToken);
}