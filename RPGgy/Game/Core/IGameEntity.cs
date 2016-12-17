using System.ComponentModel;
using System.Numerics;
using Newtonsoft.Json;
using RPGgy.Game.Items;

namespace RPGgy.Game.Core
{
    [JsonObject(MemberSerialization.OptIn,Title = "GameEntity")]
    public interface IGameEntity
    {
        [JsonProperty("attack")]
        int Attack { get; set; }

        [JsonProperty("life")]
        int LifePoints { get; set; }

        [JsonProperty("attitem")]
        AttackItem AttItem { get; set; }

        [JsonProperty("defitem")]
        DefenseItem DefItem { get; set; }

        [JsonProperty("defenseStat")]
        int Defense { get; set; }
        [JsonProperty("level")]
        short Level { get; set; }
        [JsonProperty("experience")]
        BigInteger Experience { get; set; }
        [JsonIgnore]
        BigInteger ExperienceNeededForNextLevel { get; }
        [JsonIgnore]
        BigInteger ExperienceObjective { get; }
    }
}