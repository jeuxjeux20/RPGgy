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
        private static SemaphoreSlim IsBusy = new SemaphoreSlim(1);
        public static bool Serializing = true;
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        };
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        static GameContext()
        {
            IsBusy.WaitAsync();
            try
            {
                using (var sr = new StreamReader("warriors.json")) // initiate the streamreader to read
                {
                    if (sr.ReadToEnd().Length <= 2) throw new FileNotFoundException(); // if the file contains nothing or almost nothing throw an exception
                    sr.BaseStream.Position = 0; // return back to the start of the file
                    var myLovelyReader = new JsonTextReader(sr); // get a lovely jsontextreader for you <3
                    WarriorsList =                                                                              // My variable will be set as...
                        JsonSerializer.Create(JsonSerializerSettings).Deserialize<ObservableCollection<WarriorUser>>(myLovelyReader); // a deserialization using my lovely reader ! 
                    Program.Log(new LogMessage(LogSeverity.Info, "JSONParse", "Parsed with success"));
                }
            }
            catch (FileNotFoundException) // if the file is not found
            {
                Program.Log(new LogMessage(LogSeverity.Warning, "JSONParse", "File not found !"));
            }
            catch (Exception ex) // else that's weirdo
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
                await IsBusy.WaitAsync();
                await Program.Log(new LogMessage(LogSeverity.Info, "Game", "Woah i got called :o"));
                IsBusy.Release();                           
                await Serialize();               
        }

        public static void SerializeMapped()
        {
            Task.Run(async () => await Serialize());
        }
        public static async Task Serialize()
        {
            await IsBusy.WaitAsync();
            Serializing = true; // ignore this :p
            using (var sw = new StreamWriter("warriors.json", false)) // we init again
            {                
                JsonSerializer.Create(JsonSerializerSettings).Serialize(sw, WarriorsList); // we deserialize, jsonSerializerSettings is OPTIONAL
            }
            IsBusy.Release();
            Serializing = false; // also this
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