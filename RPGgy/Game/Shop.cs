using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using JetBrains.Annotations;
using Newtonsoft.Json;
using RPGgy.Game.Items;
using RPGgy.Game.Items.Core;
using RPGgy.Game.Player;

namespace RPGgy.Game
{
    /// <summary>
    /// banana banana.. i'm not on fxcop wait ; anyways it's a static shop class.
    /// </summary>
    public static class Shop
    {
        [JsonObject(MemberSerialization.OptOut)]
        public class ShopInfo
        {
            private int _quantity;

            public int Quantity
            {
                get { return _quantity; }
                internal set
                {
                    if (value <= 0)
                    {
                        ListOfItemBases.Remove(ListOfItemBases.FirstOrDefault(tup => tup.Item2 == this));
                    }
                    _quantity = value;
                }
            }
            public uint Value { get; internal set; }
            public ShopInfo() { }
            [JsonConstructor,UsedImplicitly]            
            public ShopInfo(int quantity, uint value)
            {
                Quantity = quantity;
                Value = value;
            }
        }
        private static readonly ObservableCollection<Tuple<ItemBase, ShopInfo>> ListOfItemBases =
            new ObservableCollection<Tuple<ItemBase, ShopInfo>>
            {
                new Tuple<ItemBase, ShopInfo>(new AttackItem("Noice topkek",15), new ShopInfo
                                                                                 {
                                                                                     Quantity = 5,
                                                                                     Value = 100
                                                                                 })
            };

        static Shop()
        {
            // TODO: Serialization
        }

        public static string BuyString(int index,uint goldAvailable)
        {
            var item = GetItem(index);
            return
                $@"Are you sure you wanna buy {item.Item1.Name} for {item.Item2.Value} gold ? You have {goldAvailable} gold.
Type `RPG.yes` or `RPG.no`";
        }
        public static void AddEventOnItemAdded(NotifyCollectionChangedEventHandler act)
        {
            ListOfItemBases.CollectionChanged += act;
        }

        public static void DelEventOnItemAdded(NotifyCollectionChangedEventHandler act)
        {
            ListOfItemBases.CollectionChanged -= act;
        }

        public static Tuple<ItemBase,ShopInfo> GetItem(int index,bool overrided = true)
        {
            if (overrided) index -= 1;
            return ListOfItemBases[index];
        }

        public static async Task BuyItem(int index, IWarriorUser warrior,IMessageChannel channel)
        {
            var item = GetItem(index);
            await warrior.Buy(item.Item2.Value, new WarriorUser.ShopChanges
                                          {
                                              ItemsToAdd = new List<IItem> {item.Item1}
                                          },channel);
            item.Item2.Quantity--;
        }
        public new static string ToString()
        {
            var keak = new List<string>
                                {
                                    "--- SHOP ---"
                                };
            keak.AddRange(ListOfItemBases.Select(item => item.Item1 + $" | Quantity : {item.Item2}"));
            var strBuild = new StringBuilder();
            keak.ForEach(str => strBuild.AppendLine(str));
            return strBuild.ToString();
        }

        public static IReadOnlyCollection<string> PaginatedShopCollection()
        {
            var keak = new List<string>
                                {
                                    "--- SHOP ---"
                                };
            keak.AddRange(ListOfItemBases.Select(item => item.Item1 + $" | Quantity : {item.Item2.Quantity} | Price : {item.Item2.Value:C}"));
            var temp = new List<string> { "" };
            var s = 0;
            var builder = new StringBuilder();
            for (var i = 0; i < keak.Count; i++)
            {

                if (i < 8 * (s + 1))
                    // temp[s] += keak[i] + "\n";
                    builder.AppendLine($"{i}. " + keak[i]);
                else
                {
                    temp[s] = builder.ToString();
                    builder.Clear();
                    temp.Add("");
                    s++;
                }
            }
            if (s == 0)
                temp[0] = builder.ToString();
            return temp.AsReadOnly();
        }

    }
}