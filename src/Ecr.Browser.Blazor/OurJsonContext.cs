using System.Text.Json;
using System.Text.Json.Serialization;
using ZiggyCreatures.Caching.Fusion.Internals.Distributed;

namespace Ecr.Browser.Blazor;

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(List<ImageDetailsDto>))]
[JsonSerializable(typeof(FusionCacheDistributedEntry<List<ImageDetailsDto>>))]
internal partial class OurJsonContext : JsonSerializerContext
{
    internal static JsonSerializerOptions OurOptions => Default.Options;
}
