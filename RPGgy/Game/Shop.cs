using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
                    SerializeTasked();
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
            IsBusy.WaitAsync();
            try
            {
                using (var sr = new StreamReader("shop.json")) // initiate the streamreader to read
                {
                    if (sr.ReadToEnd().Length <= 2) throw new FileNotFoundException(); // if the file contains nothing or almost nothing throw an exception
                    sr.BaseStream.Position = 0; // return back to the start of the file
                    var myLovelyReader = new JsonTextReader(sr); // get a lovely jsontextreader for you <3
                    ListOfItemBases =                                                                              // My variable will be set as...
                        JsonSerializer.Create(JsonSerializerSettings).Deserialize<ObservableCollection<Tuple<ItemBase,ShopInfo>>>(myLovelyReader); // a deserialization using my lovely reader ! 
                    Program.Log(new LogMessage(LogSeverity.Info, "JSONShop", "Parsed with success"));
                }
            }
            catch (FileNotFoundException) // if the file is not found
            {
                Program.Log(new LogMessage(LogSeverity.Warning, "JSONShop", "File not found !"));
            }
            catch (Exception ex) // else that's weirdo
            {
                Program.Log(new LogMessage(LogSeverity.Error, "JSONShop", "Something very weird happened", ex));
            }
            IsBusy.Release();
            ListOfItemBases.CollectionChanged += ListOfItemBases_CollectionChanged;
        }
        private static readonly SemaphoreSlim IsBusy = new SemaphoreSlim(1);
        private static async void ListOfItemBases_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            await Serialize();
        }

        public static void SerializeTasked()
        {
            Task.Run(async () => { await Serialize(); });
        }
        public static async Task Serialize()
        {
            await IsBusy.WaitAsync();
            using (var sw = new StreamWriter("shop.json", false)) // we init again
            {
                JsonSerializer.Create(JsonSerializerSettings).Serialize(sw, ListOfItemBases); // we deserialize, jsonSerializerSettings is OPTIONAL
            }
            IsBusy.Release();
        }
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        };
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