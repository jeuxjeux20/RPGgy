using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows.Forms.VisualStyles;
using Discord;
using Newtonsoft.Json;

namespace RPGgy.Game.Items.Core
{
    public class ItemBase : IItem
    {
        public static readonly Dictionary<ItemType, string> ShortenedDictionary = new Dictionary<ItemType, string>
                                                                        {
                                                                            {ItemType.Attack, "ATK" },
                                                                            {ItemType.Defense, "DEF" },
                                                                            {ItemType.Unknown, "???" }
                                                                        };
        [JsonConstructor]
        protected ItemBase(string name, int val, bool isDummy, ItemType type = ItemType.Unknown,bool isdummy = false)
        {
            Name = name;
            Value = val;
            this.IsDummy = isDummy;
            Type = type;
        }
        [JsonProperty("wtf")]
        public bool IsDummy { get; }
        public int Value { get; set; }
        public string Name { get; set; }
        public virtual ItemType? Type { get; } = ItemType.Unknown;
        public override string ToString()
        {
            return
                $"**{Name}** : {Value}{ShortenedDictionary[Type ?? ItemType.Unknown]}";
        }

        public ItemBase Modify(Action<ItemModifier> modify)
        {
            var modifier = new ItemModifier
                           {
                               NameModified = Name
                           };
            modify(modifier);
            Name = modifier.NameModified;
            Value += modifier.ValueModifier;
            return this;
        }

        public class ItemModifier
        {
            /// <summary>
            /// Attention, it means that it will increase/decrease that value
            /// </summary>
            public int ValueModifier { get; set; } = 0;
            public string NameModified { get; set; }
        }

        public AttackItem ToAttackItem()
        {
            if (this.Type != ItemType.Attack)
                throw new InvalidCastException();
            return new AttackItem(Name,Value);
        }
        public DefenseItem ToDefenseItem()
        {
            if (this.Type != ItemType.Defense)
                throw new InvalidCastException();
            return new DefenseItem(Name, Value);
        }
    }
}
