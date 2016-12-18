using System;
using System.ComponentModel;
using System.Numerics;
using JetBrains.Annotations;
using Newtonsoft.Json;
using RPGgy.Game.Fights;
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
        [CanBeNull]
        FightContext AttachedFightContext { get; set; }
        [JsonIgnore]
        int AttackTotal { get; }
        [JsonIgnore]
        int DefenseTotal { get; }
        [JsonProperty("crit")]
        ushort Critical { get; set; }
        event EventHandler Died;
        Tuple<int,bool> AttackEntity(FightContext f, IGameEntity entity);
        [JsonProperty("maxlife")]
        uint MaxLife { get; set; }
    }
}