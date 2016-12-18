   
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using JetBrains.Annotations;
using Newtonsoft.Json;
using RPGgy.Game.Core;
using RPGgy.Game.Fights;
using RPGgy.Game.Items;
using RPGgy.Game.Items.Core;


namespace RPGgy.Game.Player
{
    [JsonObject(MemberSerialization.OptIn, Title = "WarriorUser")]
    public class WarriorUser : IWarriorUser,INotifyPropertyChanged
    {
        private static SemaphoreSlim _levelUpHandleLimitSemaphoreSlim = new SemaphoreSlim(0,3);
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

        public static readonly Dictionary<StatPoint, string> StatDictionary = new Dictionary<StatPoint, string>
                                                                     {
                                                                        {StatPoint.Attack, "Attack"},
                                                                        {StatPoint.Defense, "Defense"}
                                                                     };
         
        public class LevelUpEventArgs : EventArgs
        {
            public LevelUpEventArgs(WarriorUser warrior)
            {
                Warrior = warrior;
            }
            public WarriorUser Warrior { get; private set; }
            
        }
        private static readonly JsonSerializer Json = new JsonSerializer();
        private BigInteger _experience;
        private int _lifePoints;
        private ushort _critical = 10;

        public void UseStatPoint(StatPoint typeStatPoint, ushort count = 1)
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
        }
        public WarriorUser(IUser user, int attack = 25, int lifePoints = 250,AttackItem attItem = null,DefenseItem defItem = null)
        {
            LevelUpEvent += WarriorUser_LevelUpEvent;
            Attack = attack;
            LifePoints = lifePoints;
            AttachedUser = user;
        }
        
        private void WarriorUser_LevelUpEvent(object sender, LevelUpEventArgs e)
        {
            Level += 1;
            StatPoints += 1;
            /* Retry:
            try
            {
                await (await e.Warrior.AttachedUser.CreateDMChannelAsync()).SendMessageAsync(
                    $"Congratulations, you just level-uped to {e.Warrior.Level}");
            }
            catch (RateLimitedException)
            {
                await Task.Delay(2500);
                goto Retry;
            }*/ // Apparently this causes problems
        }
        public uint StatPoints { get; set; }
        [JsonConstructor]
        [UsedImplicitly]
        public WarriorUser(ulong user, int attack = 50, int lifePoints = 50, AttackItem attitem = null, DefenseItem defitem = null) // For JSON
        {
            LevelUpEvent += WarriorUser_LevelUpEvent;
            Attack = attack;
            LifePoints = lifePoints;
            AttachedUserId = user;
            AttItem = attitem ?? AttackItem.DeaultAttackItem;
            DefItem = defitem ?? DefenseItem.DefaultDefenseItem;
            if (attitem == null || defitem == null)
                Program.Log(new LogMessage(LogSeverity.Debug, "GameCtor", "god damn"));
            Died += WarriorUser_Died;
        }

        private void WarriorUser_Died(object sender, EventArgs e)
        {
            this.AttachedFightContext?.SomeoneDied(this);
        }

        public int AttackTotal => Attack + AttItem.Value;

        public List<IItem> Inventory { get; } = new List<IItem>();
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
                _lifePoints = value;
                if (_lifePoints <= 0)
                {
                    Died?.Invoke(this, null);
                    _lifePoints = 0;
                }
                if (_lifePoints > MaxLife)
                    _lifePoints = (int)MaxLife;
                GameContext.Serialize();
            }
        }

        public AttackItem AttItem { get; set; } = AttackItem.DeaultAttackItem;
        public DefenseItem DefItem { get; set; } = DefenseItem.DefaultDefenseItem;

        public ulong AttachedUserId
        {
            get { return AttachedUser.Id; }
            set { AttachedUser = Program.Instance.Client.GetUser(value); }
        }

        public int Defense { get; set; } = 10;

        public void ToJson(TextWriter tw)
        {
            Json.Serialize(tw, this);
        }

        public short Level { get; set; } = 1;
        
        public BigInteger Experience
        {
            get { return _experience; }
            set { _experience = value;
                OnPropertyChanged();
                if (_experience < ExperienceObjective) return;
                while (_experience >= ExperienceObjective)
                    LevelUpEvent?.Invoke(this,new LevelUpEventArgs(this));
                OnPropertyChanged();
            }
        }

        
        public BigInteger ExperienceNeededForNextLevel => ExperienceObjective - Experience;
        public BigInteger ExperienceObjective => (ulong) (Level * (20 + Math.Pow(Level,Level * 0.015 + 1)));
        public event EventHandler<LevelUpEventArgs> LevelUpEvent;
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            // Program.Log(new LogMessage(LogSeverity.Info, "Warrior", "WOAH! i got called ;)"));
            GameContext.Serialize();
        }

        public string AttachedUserName => AttachedUser.Username +"#"+AttachedUser.Discriminator;
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
        private static readonly Random Randomiser = new Random();
        public Tuple<int,bool> AttackEntity(FightContext f, IGameEntity entity)
        {
            if (f == null) return new Tuple<int, bool>(0,false);
            bool isCrit;
            // ReSharper disable once AssignmentInConditionalExpression
            // ReSharper disable once Stupidity
            var moarAttack = (isCrit = Randomiser.Next(0, 100) < Critical)
                ? this.AttackTotal + (this.AttackTotal / 4)
                : this.AttackTotal;
            int kek;
            entity.LifePoints -= kek = Math.Max(moarAttack * ((Randomiser.Next(1, 10) / 100) + 1) - entity.DefenseTotal,Randomiser.Next(1,3));
            return new Tuple<int, bool>(kek,isCrit);
        }

        public uint MaxLife { get; set; } = 500;
        public bool isDied => LifePoints <= 0;
    }

    public enum StatPoint
    {
        Attack = 1 << 2,
        Defense = 1 << 3            
    }
}