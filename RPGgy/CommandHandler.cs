using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace RPGgy
{
    public class CommandHandler
    {
        private DiscordSocketClient _client;
        private CommandService _commands;
        private IDependencyMap _map;
        public IEnumerable<CommandInfo> Commands => _commands.Commands;

        public async Task Install(IDependencyMap map)
        {
            // Create Command Service, inject it into Dependency Map
            _client = map.Get<DiscordSocketClient>();
            _commands = new CommandService();
            map.Add(_commands);
            _map = map;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());

            _client.MessageReceived += HandleCommand;
        }

        public async Task HandleCommand(SocketMessage parameterMessage)
        {
            // Don't handle the command if it is a system message
            var message = parameterMessage as SocketUserMessage;
            if (message == null) return;

            // Mark where the prefix ends and the command begins
            var argPos = 0;
            // Determine if the message has a valid prefix, adjust argPos 
            if (
                !(message.HasMentionPrefix(_client.CurrentUser, ref argPos) ||
                  message.HasStringPrefix("RPG.", ref argPos) ||
                  message.HasStringPrefix("@>", ref argPos)) ||
                message.Author.IsBot) return;

            // Create a Command Context
            var context = new CommandContext(_client, message);
            // Execute the Command, store the result
            var result = await _commands.ExecuteAsync(context, argPos, _map);

            // If the command failed, notify the user

            if (!result.IsSuccess)
                if (result.Error != CommandError.UnknownCommand)
                    await message.Channel.SendMessageAsync($"**Error:** {result.ErrorReason ?? "nothing specified idk why O_o that's very weird you know, i'm gonna call the police someone murdered the error string, who the hell would do that, i'm impressed by this woa"} :'(");
        }
    }
}