using RPGgy.Game.Items.Core;

namespace RPGgy.Game.Items
{
    public class AttackItem : ItemBase
    {
        public static readonly AttackItem DeaultAttackItem = new AttackItem("Starter's sword", 10);
        public AttackItem(string name, int val,bool isDummy = false) : base(name,val,isDummy)
        {
            
        }

        public override ItemType? Type { get; } = ItemType.Attack;

        
    }
}