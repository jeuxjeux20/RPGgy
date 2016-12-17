using System.ComponentModel;
using RPGgy.Game.Items.Core;

namespace RPGgy.Game.Items
{
    public class AttackItem : IItem
    {
        public AttackItem(string name, int val, ushort defaultDurability = 500)
        {
            Name = name;
            Durability = defaultDurability;
            Value = val;
        }

        public int Value { get; set; }
        public static readonly AttackItem DeaultAttackItem = new AttackItem("Starter's sword", 10);
        public string Name { get; set; }
        public ushort? Durability { get; set; }
        public ItemType Type { get; } = ItemType.Attack;
    }
}