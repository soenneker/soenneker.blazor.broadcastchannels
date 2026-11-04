using Soenneker.Blazor.BroadcastChannels.Abstract;
using Soenneker.Tests.HostedUnit;

namespace Soenneker.Blazor.BroadcastChannels.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public sealed class BroadcastChannelsTests : HostedUnitTest
{
    private readonly IBroadcastChannels _blazorlibrary;

    public BroadcastChannelsTests(Host host) : base(host)
    {
        _blazorlibrary = Resolve<IBroadcastChannels>(true);
    }

    [Test]
    public void Default()
    {

    }
}
