using System;
using System.Collections.Generic;
using System.Windows.Forms.VisualStyles;
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
        public ItemBase(string name, int val, ushort? defaultDurability = 500,ItemType type = ItemType.Unknown)
        {
            Name = name;
            Durability = defaultDurability;
            Value = val;
            Type = type;
        }
        public int Value { get; set; }
        public string Name { get; set; }
        public ushort? Durability { get; set; }
        public virtual ItemType? Type { get; } = ItemType.Unknown;
        public override string ToString()
        {
            return
                $"**{Name}** : {Value}{ShortenedDictionary[Type ?? ItemType.Unknown]} {(Durability == null ? "" : $"Durability : {Durability}")}";
        }

        public AttackItem ToAttackItem()
        {
            if (this.Type != ItemType.Attack)
                throw new InvalidCastException();
            return new AttackItem(Name,Value,Durability);
        }
        public DefenseItem ToDefenseItem()
        {
            if (this.Type != ItemType.Defense)
                throw new InvalidCastException();
            return new DefenseItem(Name, Value, Durability);
        }
    }
}
