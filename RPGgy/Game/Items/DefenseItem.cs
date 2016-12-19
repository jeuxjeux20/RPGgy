using RPGgy.Game.Items.Core;

namespace RPGgy.Game.Items
{
    public class DefenseItem : IItem
    {
        public static readonly DefenseItem DefaultDefenseItem = new DefenseItem("Large piece of wood", 2);

        public DefenseItem(string name, int val, ushort durUshort = 510)
        {
            Name = name;
            Durability = durUshort;
            Value = val;
        }

        public int Value { get; set; }
        public string Name { get; set; }
        public ushort? Durability { get; set; }
        public ItemType Type { get; } = ItemType.Defense;
    }
}