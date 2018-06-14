using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Impact.Website.Models.Options
{
    [JsonConverter(typeof(StringEnumConverter), true)]
    public enum AnimationEasing
    {
        Linear,
        In,
        Out,
        InAndOut
    }
    
    
}