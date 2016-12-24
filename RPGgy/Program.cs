using System;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.InteractiveCommands;
using Discord.Commands;
using Discord.WebSocket;
using RPGgy.Game;
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
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            tokin = Settings.Default.Token;
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Console.WriteLine(@"Goodbye :)");
            GameContext.SerializeMapped();
        }

        public static DependencyMap Map { get; } = new DependencyMap();
        public static Program Instance { get; private set; }

        public static void Main(string[] args) =>
            new Program().Start(args ?? new [] {tokin}).GetAwaiter().GetResult();

        private static string tokin;
        private async Task Start(string[] arguments)
        {
            
            // Define the DiscordSocketClient
            Client = new DiscordSocketClient();

            var token = arguments.Length > 0 ? arguments[0] : tokin;

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