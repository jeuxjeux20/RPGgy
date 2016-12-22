using System;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json;
using RPGgy.Game.Core;

namespace RPGgy.Game.Player
{
    public interface IWarriorUser : IGameEntity
    {
        [JsonIgnore]
        IUser AttachedUser { get; }

        [JsonProperty("id")]
        ulong AttachedUserId { get; set; }

        [JsonProperty("statPoints")]
        uint StatPoints { get; set; }

        //[JsonProperty("nameOfUser")]
        //string AttachedUserName { get; }

        bool IsOk(IUser testUser);
        event EventHandler<WarriorUser.LevelUpEventArgs> LevelUpEvent;

        Task Buy(uint cost,Action<IWarriorUser> action,IMessageChannel channel = null);
    }
}