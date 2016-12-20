using System;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.InteractiveCommands;
using Discord.Commands;
using Discord.WebSocket;
using RPGgy.Properties;

namespace RPGgy
{
    public class Program
    {
        public static CommandHandler Handler;
        public DiscordSocketClient Client;

        private Program()
        {
            Instance = this;
        }

        public static DependencyMap Map { get; } = new DependencyMap();
        public static Program Instance { get; private set; }

        public static void Main(string[] args) =>
            new Program().Start().GetAwaiter().GetResult();

       
        private async Task Start()
        {
            
            // Define the DiscordSocketClient
            Client = new DiscordSocketClient();
           
            var token = Settings.Default.Token;

            // Login and connect to Discord.
            Again:
            try
            {
                await Client.LoginAsync(TokenType.Bot, token);
            }
            catch (Exception)
            {
                Console.WriteLine(@"Please, insert the token !");
                Console.Out.Flush();
                token = Console.ReadLine();
                goto Again;
            }

            await Client.ConnectAsync();
            Settings.Default.Token = token;
            Settings.Default.Save();
            await Log(new LogMessage(LogSeverity.Info, "Main", "The bot has been succesfully connected !"));

            Map.Add(Client);
            Map.Add(new InteractiveService(Client));
            Handler = new CommandHandler();
            await Handler.Install(Map);

            // Block this program until it is closed.
            await Task.Delay(-1);
        }

        public static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}