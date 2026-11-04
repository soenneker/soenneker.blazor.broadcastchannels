using System;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Blazor.BroadcastChannels.Abstract;
/// <summary>A typed JSON channel shared with listeners of the same name in the same browser origin and storage partition.
/// Messages are transient, are not replayed, and are not delivered back to the sending channel object.
/// Channel names are coordination identifiers, not an authorization boundary.</summary>
public interface IBroadcastChannel<T> : IAsyncDisposable
{
    /// <summary>The browser channel name.</summary>
    string Name { get; }
    /// <summary>Serializes using the metadata supplied at open and broadcasts the value.
    /// Completion confirms posting, not receipt by another tab. Cancellation does not retract a posted message.</summary>
    ValueTask Post(T message, CancellationToken cancellationToken = default);
}