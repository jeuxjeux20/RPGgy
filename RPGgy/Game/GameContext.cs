using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json;
using RPGgy.Game.Player;

namespace RPGgy.Game
{
    public static class GameContext
    {
        public static SemaphoreSlim IsBusy = new SemaphoreSlim(1);
        public static bool Serializing = true;

        static GameContext()
        {
            IsBusy.WaitAsync();
            try
            {
                using (var sr = new StreamReader("warriors.json"))
                {
                    if (sr.ReadToEnd().Length < 2) throw new FileNotFoundException();
                    sr.BaseStream.Position = 0;
                    var myLovelyReader = new JsonTextReader(sr);

                    WarriorsList =
                        JsonSerializer.Create().Deserialize<ObservableCollection<WarriorUser>>(myLovelyReader);
                    Program.Log(new LogMessage(LogSeverity.Info, "JSONParse", "Parsed with success"));
                }
            }
            catch (FileNotFoundException)
            {
                Program.Log(new LogMessage(LogSeverity.Warning, "JSONParse", "File not found !"));
            }
            catch (Exception ex)
            {
                Program.Log(new LogMessage(LogSeverity.Error, "JSONParse", "Something very weird happened", ex));
            }           
            IsBusy.Release();
            WarriorsList.CollectionChanged += WarriorsList_CollectionChanged;
            Serializing = false;
        }

        [JsonProperty("players")]
        public static ObservableCollection<WarriorUser> WarriorsList { get; set; } =
            new ObservableCollection<WarriorUser>();

        private static async void WarriorsList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            while (Serializing) await Task.Delay(100);
                await Program.Log(new LogMessage(LogSeverity.Info, "Game", "Woah i got called :o"));                           
                await Serialize();
                
        }

        public static void SerializeMapped()
        {
            Task.Run(async () => await Serialize());
        }
        public static async Task Serialize()
        {
            await IsBusy.WaitAsync();
            Serializing = true;
            using (var sw = new StreamWriter("warriors.json", false))
            {
                JsonSerializer.Create(new JsonSerializerSettings
                                      {
                                          MissingMemberHandling = MissingMemberHandling.Ignore,
                                          NullValueHandling = NullValueHandling.Ignore,
                                          Formatting = Formatting.Indented
                                      }).Serialize(sw, WarriorsList);
            }
            IsBusy.Release();
            Serializing = false;
        }

        public static async Task Deserialize()
        {
                await IsBusy.WaitAsync();
                DeserializeCore();
                IsBusy.Release();
        }

        private static void DeserializeCore()
        {
            Serializing = true;
            using (var sr = new StreamReader("warriors.json"))
            {
                if (sr.ReadToEnd().Length < 2) throw new FileNotFoundException();
                sr.BaseStream.Position = 0;
                var myLovelyReader = new JsonTextReader(sr);

                WarriorsList =
                    JsonSerializer.Create().Deserialize<ObservableCollection<WarriorUser>>(myLovelyReader);
                Program.Log(new LogMessage(LogSeverity.Info, "JSONParse", "Parsed with success"));
            }
            Serializing = false;
        }
    }
}