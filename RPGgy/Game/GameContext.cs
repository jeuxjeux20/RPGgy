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
            Serializing = false;
            WarriorsList.CollectionChanged += WarriorsList_CollectionChanged;
        }

        [JsonProperty("players")]
        public static ObservableCollection<WarriorUser> WarriorsList { get; set; } =
            new ObservableCollection<WarriorUser>();

        private static void WarriorsList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Program.Log(new LogMessage(LogSeverity.Info, "Game", "Woah i got called :o"));
            Task.Run(() =>
            {
                IsBusy.Wait(); Serialize();
                IsBusy.Release();
            });
        }

        public static void Serialize()
        {
            if (Serializing) return;
            using (StreamWriter sw = new StreamWriter("warriors.json", false))
            {
                JsonSerializer.Create(new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                }).Serialize(sw, WarriorsList);
            }
        }

        public static async Task Deserialize()
        {
            await Task.Run(() =>
            {
                IsBusy.Wait(); DeserializeCore();
                IsBusy.Release();
            });
        }
        internal static void DeserializeCore()
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