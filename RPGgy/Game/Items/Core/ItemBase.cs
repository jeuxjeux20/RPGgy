using System.Collections.Generic;

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
        public ItemBase(string name, int val, ushort? defaultDurability = 500)
        {
            Name = name;
            Durability = defaultDurability;
            Value = val;
            
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
    }
}
