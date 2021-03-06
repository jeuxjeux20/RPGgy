﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using JetBrains.Annotations;
using Newtonsoft.Json;
using RPGgy.Game.Core;
using RPGgy.Game.Fights;
using RPGgy.Game.Items;
using RPGgy.Game.Items.Core;
using RPGgy.Misc.Tools;
using RPGgy.Public.Modules.Tools;

namespace RPGgy.Game.Player
{
    [JsonObject(MemberSerialization.OptIn, Title = "WarriorUser")]
    public sealed class WarriorUser : IWarriorUser, INotifyPropertyChanged
    {
        public string Mention => AttachedUser.Mention;

        public static WarriorUser GetUser(IUser findToken)
        {
            return GameContext.WarriorsList.FirstOrDefault(war => war.IsOk(findToken));
        }

        private static readonly JsonSerializer Json = new JsonSerializer();
        private static readonly Random Randomiser = new Random(DateTime.Now.Millisecond + (int)DateTime.Today.Ticks); // true random :ok_hand:
        private ushort _critical = 10;
        private BigInteger _experience;
        private int _lifePoints;
        private uint _gold = DefaultGold;
        // DEFAULTS 
        private const int DefaultAttack = 50;
        private const int DefaultDefense = 30;
        private const int DefaultLifePoints = 200;
        private const int DefaultMaxLife = 200;
        private const int DefaultGold = 250;
        // DEFAULTS
        public WarriorUser(IUser user,
            int attack = DefaultAttack,
            int lifePoints = DefaultLifePoints,
            AttackItem attItem = null,
            DefenseItem defItem = null)
        {
            LevelUpEvent += WarriorUser_LevelUpEvent;
            Attack = attack;
            LifePoints = lifePoints;
            AttachedUser = user;
        }

        [JsonConstructor]
        [UsedImplicitly]
        public WarriorUser(ulong user,
            int attack = DefaultAttack,
            int lifePoints = DefaultLifePoints,
            uint maxLife = DefaultMaxLife,
            AttackItem attitem = null,
            DefenseItem defitem = null,
            uint gold = DefaultGold) // For JSON
        {
            LevelUpEvent += WarriorUser_LevelUpEvent;
            Attack = attack;
            LifePoints = lifePoints;
            AttachedUserId = user;
            AttItem = attitem ?? AttackItem.DeaultAttackItem;
            DefItem = defitem ?? DefenseItem.DefaultDefenseItem;
            _gold = gold;
            if (attitem == null || defitem == null)
                Program.Log(new LogMessage(LogSeverity.Debug, "GameCtor", "god damn"));
            Died += WarriorUser_Died;
            Inventory.CollectionChanged += Inventory_CollectionChanged;
        }

        private  void Inventory_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // GameContext.SerializeMapped();
            foreach (var thing in Inventory)
            {
                if (thing.IsDummy)
                    Inventory.Remove(thing);
            }
        }

        public ObservableCollection<ItemBase> Inventory { get; set; } = new ObservableCollection<ItemBase>();
        public event PropertyChangedEventHandler PropertyChanged;
        public uint StatPoints { get; set; }

        public int AttackTotal => Attack + AttItem.Value;
        public int DefenseTotal => Defense + DefItem.Value;

        public bool IsOk(IUser testUser)
        {
            return testUser.Equals(AttachedUser) || testUser.Id == AttachedUserId;
        }

        public IUser AttachedUser { get; private set; }

        public int Attack { get; set; }

        public int LifePoints
        {
            get { return _lifePoints; }
            set
            {
                if (value <= 0)
                {
                    Died?.Invoke(this, null);
                    Task.Run(async () =>
                    {
                        var someoneDied = AttachedFightContext?.SomeoneDied(this);
                        if (someoneDied != null)
                            await someoneDied;
                    }); 
                    _lifePoints = 0;
                }
                else if (value > MaxLife)
                {
                    _lifePoints = MaxLife;
                }
                else
                {
                    _lifePoints = value;
                }
                // GameContext.SerializeMapped();
            }
        }

        public AttackItem AttItem { get; set; } = AttackItem.DeaultAttackItem;
        public DefenseItem DefItem { get; set; } = DefenseItem.DefaultDefenseItem;

        public ulong AttachedUserId
        {
            get { return AttachedUser.Id; }
            set { AttachedUser = Program.Instance.Client.GetUser(value); }
        }

        public int Defense { get; set; } = DefaultDefense;

        public short Level { get; set; } = 1;

        public BigInteger Experience
        {
            get { return _experience; }
            set
            {
                _experience = value;
                OnPropertyChanged();
                if (_experience < ExperienceObjective) return;
                while (_experience >= ExperienceObjective)
                    LevelUpEvent?.Invoke(this, new LevelUpEventArgs(this, (uint)Level));
                OnPropertyChanged();
            }
        }


        public BigInteger ExperienceNeededForNextLevel => ExperienceObjective - Experience;
        public BigInteger ExperienceObjective => (ulong)(Level * (25 + Math.Pow(Level, Level * 0.06 + 1)));
        public event EventHandler<LevelUpEventArgs> LevelUpEvent;

        public string Name => AttachedUser.Username + "#" + AttachedUser.Discriminator;
        public FightContext AttachedFightContext { get; set; } = null;
        public event EventHandler Died;

        public ushort Critical
        {
            get { return _critical; }
            set
            {
                if (value > 100)
                {
                    _critical = 100;
                    return;
                }
                _critical = value;
            }
        }

        public Tuple<int, bool> AttackEntity(FightContext f, IGameEntity entity, TimeSpan? timeTook = null)
        {
            if (f == null) return new Tuple<int, bool>(0, false);
            bool isCrit;
            float mult;
            if (timeTook != null)
            {
                mult = (float)Math.Max(timeTook.Value.TotalSeconds / 20, 0.95);
            }
            else
            {
                mult = 1;
            }
            // ReSharper disable once AssignmentInConditionalExpression
            // ReSharper disable once Stupidity
            var moarAttack = (isCrit = Randomiser.Next(0, 100) < Critical)
                ? AttackTotal + AttackTotal / 4
                : AttackTotal;
            int kek;
            entity.LifePoints -= kek =
                                                (int)(Math.Max(
                                                           moarAttack * (Randomiser.Next(1, 25) / 100 + 1) +
                                                           Randomiser.Next(1, 3) - entity.DefenseTotal,
                                                           Randomiser.Next(1, entity.Level)) / mult);

            return new Tuple<int, bool>(kek, isCrit);
        }

        public int MaxLife { get; set; } = DefaultMaxLife;
        public bool IsDead => LifePoints <= 0;

        public Task UseStatPoint(StatPoint typeStatPoint, ushort count = 1)
        {
            if (StatPoints <= 0) throw new NoStatpointsException();
            if (count > StatPoints) throw new NotEnoughStatpointsException();
            if (typeStatPoint == StatPoint.Attack)
            {
                Attack += count;
                StatPoints -= count;
            }
            else if (typeStatPoint == StatPoint.Defense)
            {
                Defense += count;
                StatPoints -= count;
            }
            else
            {
                throw new ArgumentException("Wait m8, i don't see eitherr attack or defense -,-");
            }
            // await GameContext.Serialize();
            return Task.CompletedTask;
        }

        private async void WarriorUser_LevelUpEvent(object sender, LevelUpEventArgs e)
        {
            Level += 1;
            StatPoints += 1;
            var thing = e.Warrior.AttachedUser.CreateDMChannelAsync().Result; // ensure locking              
            string waitWhat = $"You level-uped to {e.Level}";
            await RateLimitTools.RetryRatelimits(async () => await thing.SendMessageAsync(waitWhat));
        }

        public ItemBase FromItemType(ItemType type)
        {
            switch (type)
            {
                case ItemType.Attack:
                    return AttItem;
                case ItemType.Defense:
                    return DefItem;
                case ItemType.Unknown:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private async void WarriorUser_Died(object sender, EventArgs e)
        {
            Task someoneDied = AttachedFightContext?.SomeoneDied(this);
            if (someoneDied != null) await someoneDied;
        }

        public void ToJson(TextWriter tw)
        {
            Json.Serialize(tw, this);
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            // Program.Log(new LogMessage(LogSeverity.Info, "Warrior", "WOAH! i got called ;)"));
            // await GameContext.Serialize();
        }

        /// <summary>
        /// The gold $_$
        /// </summary>
        public uint Gold
        {
            get { return _gold; }
            set { _gold = value; OnPropertyChanged(); }
        }

        private void ApplyChanges(ShopChanges changes)
        {
            this.LifePoints = changes.LifePointsChange ?? this.LifePoints;
            this.AttItem = changes.AttackItemChange ?? AttItem;
            DefItem = changes.DefenseItemChange ?? DefItem;
            if (changes.ItemsToAdd != null)
            foreach (var item1 in changes.ItemsToAdd)
            {
                var item = (ItemBase) item1;
                Inventory.Add(item);
            }
            SetAuto(changes.AutoChange);
        }
        //public static WarriorUser operator +(WarriorUser warrior, ShopChanges changes)
        //{
        //    WarriorUser warriorUser = warrior;
        //    warriorUser.LifePoints = changes.LifePointsChange ?? warriorUser.LifePoints;
        //    warriorUser.AttItem = changes.AttackItemChange ?? warriorUser.AttItem;
        //    warriorUser.DefItem = changes.DefenseItemChange ?? warriorUser.DefItem;
        //    return warriorUser;
        //}
        public async Task Buy(uint cost, ShopChanges changes, IMessageChannel channel = null)
        {
            if (cost > Gold)
                throw new NotEnoughGoldException("You don't have enough gold to buy this.");
            Gold -= cost;
            if (channel != null)
            {
                var message = await channel.SendMessageAsync($"Buying... :moneybag: <- :money_with_wings: {cost} gold");
                using (channel.EnterTypingState())
                {
                    await Task.Delay(2500);
                    ApplyChanges(changes);
                    await message.ModifyAsync(modifier => modifier.Content = ":heavy_check_mark: The transaction has been succesfully executed !");
                }
            }
        }

        public void SetAuto(ItemBase wut)
        {
            if (wut == null) return;
            switch (wut.Type)
            {
                case ItemType.Attack:
                    AttItem = wut.ToAttackItem();
                    break;
                case ItemType.Defense:
                    DefItem = wut.ToDefenseItem();
                    break;
                case ItemType.Unknown:
                    Inventory.Add(wut);
                    break;
                case null:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public class ShopChanges
        {
            public AttackItem AttackItemChange { get; set; } = null;
            public DefenseItem DefenseItemChange { get; set; } = null;
            public int? LifePointsChange { get; set; } = null;
            public IEnumerable<IItem> ItemsToAdd { get; set; } = null;
            public ItemBase AutoChange { get; set; } = null;
        }

        [Serializable]
        public class NoStatpointsException : Exception
        {
            //
            // For guidelines regarding the creation of new exception types, see
            //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
            // and
            //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
            //

            public NoStatpointsException()
            {
            }

            public NoStatpointsException(string message) : base(message)
            {
            }

            public NoStatpointsException(string message, Exception inner) : base(message, inner)
            {
            }

            protected NoStatpointsException(
                SerializationInfo info,
                StreamingContext context) : base(info, context)
            {
            }
        }

        [Serializable]
        public class NotEnoughStatpointsException : Exception
        {
            //
            // For guidelines regarding the creation of new exception types, see
            //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
            // and
            //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
            //

            public NotEnoughStatpointsException()
            {
            }

            public NotEnoughStatpointsException(string message) : base(message)
            {
            }

            public NotEnoughStatpointsException(string message, Exception inner) : base(message, inner)
            {
            }

            protected NotEnoughStatpointsException(
                SerializationInfo info,
                StreamingContext context) : base(info, context)
            {
            }
        }

        [Serializable]
        public class NotEnoughGoldException : Exception
        {
            //
            // For guidelines regarding the creation of new exception types, see
            //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
            // and
            //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
            //

            public NotEnoughGoldException()
            {
            }

            public NotEnoughGoldException(string message) : base(message)
            {
            }

            public NotEnoughGoldException(string message, Exception inner) : base(message, inner)
            {
            }

            protected NotEnoughGoldException(
                SerializationInfo info,
                StreamingContext context) : base(info, context)
            {
            }
        }
        public class LevelUpEventArgs : EventArgs
        {
            public LevelUpEventArgs(WarriorUser warrior, uint level)
            {
                Warrior = warrior;
                Level = level + 1;
            }
            public WarriorUser Warrior { get; }
            public uint Level { get; }
        }
    }

    public enum StatPoint
    {
        Attack = 1 << 2,
        Defense = 1 << 3
    }
}