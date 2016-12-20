using Newtonsoft.Json;

namespace RPGgy.Game.Items.Core
{
    [JsonObject(MemberSerialization.OptIn)]
    public interface IItem
    {
        [JsonProperty("name")]
        string Name { get; set; }

        [JsonProperty("durability")]
        ushort? Durability { get; set; }

        [JsonProperty("type")]
        ItemType? Type { get; }

        [JsonProperty("value")]
        int Value { get; set; }
    }

    public enum ItemType
    {
        Attack,
        Defense,
        Unknown
    }
}