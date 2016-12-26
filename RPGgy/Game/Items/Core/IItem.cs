using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RPGgy.Game.Items.Core
{
    [JsonObject(MemberSerialization.OptIn)]
    public interface IItem
    {
        [JsonProperty("name")]
        string Name { get; set; }

        [JsonProperty("type")]
        ItemType? Type { get; }

        [JsonProperty("value")]
        int Value { get; set; }

        [JsonProperty("isDummy")]
        bool IsDummy { get; }
    }

    public enum ItemType
    {
        Attack,
        Defense,
        Unknown
    }
}