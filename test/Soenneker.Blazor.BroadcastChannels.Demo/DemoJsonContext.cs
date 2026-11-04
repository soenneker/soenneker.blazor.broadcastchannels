using System.Text.Json.Serialization;
namespace Soenneker.Blazor.BroadcastChannels.Demo;
[JsonSerializable(typeof(string))]
internal partial class DemoJsonContext : JsonSerializerContext;
