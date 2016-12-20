using RPGgy.Game.Items.Core;

namespace RPGgy.Game.Items
{
    public class DefenseItem : ItemBase
    {
        public static readonly DefenseItem DefaultDefenseItem = new DefenseItem("Large piece of wood", 2);
        
        public DefenseItem(string name, int val, ushort? durUshort = 510) : base(name,val,durUshort)
        {
            
        }
        
        public override ItemType? Type { get; } = ItemType.Defense;
    }
}