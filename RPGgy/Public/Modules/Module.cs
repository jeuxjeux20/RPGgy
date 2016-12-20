﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using RPGgy.Game;
using RPGgy.Game.Core;
using RPGgy.Game.Fights;
using RPGgy.Game.Player;
using RPGgy.Misc.Tools;
using RPGgy.Permissions.Attributes;
using Discord.Addons.InteractiveCommands;
using RPGgy.Public.Modules.Tools;
using ParameterInfo = Discord.Commands.ParameterInfo;

// ReSharper disable UnusedMember.Global
// Because m8, you didn't know that discord.net uses some magic that we call reflection ! No, it's not about mirrors.

namespace RPGgy.Public.Modules
{
    public class PublicModule : ModuleBase
    {
        private InteractiveService Interactive { get; }
        public PublicModule(InteractiveService inter)
        {
            Interactive = inter;
        }
        [Command("createuser")]
        [Summary("Creates a new user in the warrior list")]
        public async Task CreateWarrior()
        {
            if (GameContext.WarriorsList.All(u => u.AttachedUser != Context.User))
            {
                GameContext.WarriorsList.Add(new WarriorUser(Context.User));
                await ReplyAsync($":tada: {Context.User.Username} has been added to the list !");
            }
            else
            {
                var lol = await ReplyAsync($":tada: {Context.User.Username} has been added to the list !");
                await Task.Delay(250);
                await lol.ModifyAsync(mod => mod.Content = ":tada: Eh....");
                await lol.ModifyAsync(mod => mod.Content = "You are alerady in the list, sorry !");
            }
        }

        [Command("lol")]
        [Summary("lol")]
        public async Task lol()
        {
            await ReplyAsync("lol ?");
            var kek = await Interactive.WaitForMessage(Context.User,
                                                       Context.Channel);
            await ReplyAsync($"KEEEEEK : {kek.Content}");
        }

        [Command("reloadjson")]
        [Summary("Reloads the json")]
        [RequireOwner]
        public async Task ReloadJson()
        {
            var msg = await ReplyAsync(":sparkle: Reloading the JSON...");
            await GameContext.Deserialize();
            await msg.ModifyAsync(param => param.Content = ":white_check_mark: JSON reloaded !");
        }


        [Command("fightstart")]
        [Summary("Let's start a fight against someone")]
        [MustBeRegistered]
        [MusntBeDead]
        [MusntBeInFight]
        public async Task Fightstart([MustBeRegisteredParameter, MusntBeInFightParameter]
        [UserMustBeOnlineParameter] IUser toFight)
        {
            var user = GameContext.WarriorsList.FirstOrDefault(war => war.IsOk(Context.User));
            if (user == null)
            {
                await ReplyAsync("You aren't registered ! register using RPG.createuser :smiley:");
                return;
            }
            var usertoFight = GameContext.WarriorsList.FirstOrDefault(war => war.IsOk(toFight));
            if (usertoFight == null)
            {
                await ReplyAsync("This guy isn't registered");
                return;
            }
            await ReplyAsync("owo");
            var fight = new FightContext(user, usertoFight);
            fight.Done += async (sender, e) =>
        {
            float randomMult = (new Random(DateTime.Now.Millisecond).Next(1, 20) / (float)100) + 1;
            float levelMult = (e.WhoDiedUser.Level / (float)e.WinUser.Level) * randomMult;
            int beforeMult = 100 + (int)((int)(e.WhoDiedUser.Level * randomMult) * levelMult); // the difference matters !
            int finalResult = 100 + (int)((int)(e.WhoDiedUser.Level * randomMult) * 3 * levelMult * randomMult);
            await RateLimitTools.RetryRatelimits(async () => await ReplyAsync($"Woo ! {e.WhoDiedUser.Name} died !"));
            await RateLimitTools.RetryRatelimits(async () => await ReplyAsync($@"Rewards for {e.WinUser.Name} :
Before applying the multiplier : {beforeMult} XP
After applying the {randomMult:0.00%} multiplier : {finalResult} XP !"));          
            e.WinUser.Experience += finalResult;
            
        };
            fight.OnTurnChanged += async (sender, e) =>
            {
                await RateLimitTools.RetryRatelimits(async () => await ReplyAsync($":crossed_swords: It's now {e.CurrentTurnUser.AttachedUser.Mention}'s turn !"));
            };
        }

        [Command("leavebattle"), Alias("surrender")]
        [Summary("Leave the battle, YOU WILL DIE")]
        [MustBeRegistered]
        [MustBeInFight]
        public async Task Leavebattle()
        {
            var user = GameContext.WarriorsList.FirstOrDefault(war => war.IsOk(Context.User));
            await ReplyAsync("You choosed to surrender ! (and die)");
            user.LifePoints = 0;
        }

        [Command("heal")]
        [Summary("heals yourself")]
        [MustBeRegistered]
        [MusntBeInFight]
        [MustHaveEnoughGold(20)]
        public async Task Heal()
        {
            var user = GameContext.WarriorsList.FirstOrDefault(war => war.IsOk(Context.User));
            await ReplyAsync($@"Are you sure you wanna buy a full heal for 20 gold ? You have {user.Gold} gold.
Type `RPG.yes` or `RPG.no` wait no because InteractiveCommands doesn't work so");
            var messageContainsResponsePrecondition = new MessageContainsResponsePrecondition("RPG.yes", "RPG.no",
                                                                                              "RPG.confirm");
            var result = await Interactive.WaitForMessage(Context.User,
                                                    Context.Channel,
                                                    TimeSpan.FromSeconds(3),
                                                    messageContainsResponsePrecondition);
            if (result?.Content == "RPG.yes" || result?.Content == "RPG.confirm" || true/* temporary */)
            {
                await user.Buy(20, u =>
                {
                    u.LifePoints = u.MaxLife;
                },Context.Channel);
                
            }
            else
            {
                await ReplyAsync("You choosed to not buy this.");
            }
        }

        [Command("Attack")]
        [Summary("Attacks if you're in  a fight")]
        [MustBeRegistered]
        [MustBeInFight]
        [FightMustBeHisTurn]
        public async Task Attack()
        {
            var user = GameContext.WarriorsList.FirstOrDefault(war => war.IsOk(Context.User));

            if (user?.AttachedFightContext == null)
            {
                await ReplyAsync("You aren't in a fight");
                
                return;
            }
            
            // await ReplyAsync($" Ouch ! {user.AttachedFightContext.TurnOfUser.Username} dealt {user.AttachedFightContext.Attack()} damage !");
            user.AttachedFightContext.Attack(async r =>
            {
                IGameEntity[] actual = { user.AttachedFightContext.TurnOfEntity, user.AttachedFightContext.TurnOfEnemy }; // actual[0] is current, actual[1] is ennemy
                await ReplyAsync(
                    $@"{(r.IsCritical ? "CRITICAL ! " : "")}{actual[0].Name} dealt {r
                        .AttackValue} damage !
{actual[1].Name} : {actual[1].LifePoints} HP
{AsciiBar.DrawProgressBar(actual[1].LifePoints,
                          actual[1].MaxLife)}
{actual[0].Name} : {actual[0].LifePoints} HP
{AsciiBar.DrawProgressBar(actual[0].LifePoints,
                          actual[0].MaxLife)}");
            });
            
        }

        [Command("forcejson")]
        [Summary("Forces the json to serialize")]
        [RequireOwner]
        public async Task ForceJson()
        {
            var msg = await ReplyAsync(":sparkle: Serializing the JSON...");
            await GameContext.Serialize();
            await msg.ModifyAsync(param => param.Content = ":white_check_mark: JSON serialized !");
        }

        [Command("xp")]
        [Summary("wunt xp")]
        [MustBeRegistered]
        public Task Exp()
        {
            var firstOrDefault = GameContext.WarriorsList.FirstOrDefault(war => war.IsOk(Context.User));
            if (firstOrDefault != null)
                firstOrDefault.Experience += 200;

            return Task.CompletedTask;
        }

        [Command("usepoint")]
        [Summary("Use a stat point ;)")]
        [MustBeRegistered]
        public async Task Usepoint(string type, ushort howMuch = 1)
        {
            var user = GameContext.WarriorsList.FirstOrDefault(war => war.IsOk(Context.User));

            StatPoint stat;
            if (type.ToLower().Contains("attack"))
            {
                stat = StatPoint.Attack;
            }
            else if (type.ToLower().Contains("defense"))
            {
                stat = StatPoint.Defense;
            }
            else
            {
                await ReplyAsync("The first parameter is invalid, it should be either attack or defense");
                return;
            }
            try
            {
                await user.UseStatPoint(stat, howMuch);
            }
            catch (WarriorUser.NoStatpointsException)
            {
                await ReplyAsync("You don't have any stat points :p");
                return;
            }
            catch (WarriorUser.NotEnoughStatpointsException)
            {
                await ReplyAsync(
                    $"You don't have enough stat points for this. (Requested : {howMuch} ; Available {user.StatPoints})");
                return;
            }
            await ReplyAsync($"Succesfully added {howMuch} points to the {stat.ToString()} stat");
        }

        [Command("exception")]
        [Summary("throw it against the wall")]
        public Task Exception()
        {
            throw new Exception("WAAAAAAA THROWN POPOPOOOOOOOOO");
        }

        [Command("stats")]
        [Summary("Get the stats of your fighter !")]
        [MustBeRegistered]
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")] // because it's checked with MuseBeRegistered and STOP nagging me pls
        public async Task Stats()
        {
            var user = GameContext.WarriorsList.FirstOrDefault(war => war.IsOk(Context.User));

            await ReplyAsync("", embed: new EmbedBuilder()
                                 .WithThumbnailUrl(Context.User.AvatarUrl)
                                 .AddField(builder =>
                                 {
                                     builder.Name = "Attack stat :";
                                     builder.Value = user.Attack.ToString();
                                     builder.IsInline = true;
                                 })
                                 .AddField(builder =>
                                 {
                                     builder.Name = "Total attack :";
                                     builder.Value = user.AttackTotal.ToString();
                                     builder.IsInline = true;
                                 })
                                 .AddField(builder =>
                                 {
                                     builder.Name = "Defense stat :";
                                     builder.Value = user.Defense.ToString();
                                     builder.IsInline = true;
                                 })
                                 .AddField(builder =>
                                 {
                                     builder.Name = "Total Defense :";
                                     builder.Value = user.DefenseTotal.ToString();
                                     builder.IsInline = true;
                                 })
                                 .AddField(builder =>
                                 {
                                     builder.Name = "Offensive item";
                                     builder.Value = $"{user.AttItem.Name} | {user.AttItem.Value} ATK";
                                     builder.IsInline = true;
                                 })
                                 .AddField(builder =>
                                 {
                                     builder.Name = "Defensive item";
                                     builder.Value = $"{user.DefItem.Name} | {user.DefItem.Value} DEF";
                                     builder.IsInline = true;
                                 })
                                 .AddField(builder =>
                                 {
                                     builder.Name = "Current Level";
                                     builder.Value = user.Level.ToString();
                                     builder.IsInline = true;
                                 })
                                 .AddField(builder =>
                                 {
                                     builder.Name = "XP";
                                     builder.Value = user.Experience.ToString();
                                     builder.IsInline = true;
                                 })
                                 .AddField(builder =>
                                 {
                                     builder.Name = "XP to reach";
                                     builder.Value = user.ExperienceObjective.ToString();
                                     builder.IsInline = true;
                                 })
                                 .AddField(builder =>
                                 {
                                     builder.Name = "Stat points available";
                                     builder.Value = user.StatPoints.ToString();
                                     builder.IsInline = true;
                                 })
                                 .AddField(builder =>
                                               builder.WithName("HP")
                                                   .WithValue($"{user.LifePoints}/{user.MaxLife}")
                                                   .WithIsInline(true)
                                 )
                                 .AddField(builder => builder
                                                      .WithName("Gold")
                                                      .WithValue(user.Gold.ToString())
                                                      .WithIsInline(true))
                                 .WithTitle($"Stats for {user.AttachedUser.Username}"));
        }
        [Command("logout")]
        [Alias("disconnect")]
        [RequireOwner]
        public async Task Disconnect()
        {
            await ReplyAsync("Goodbye !");

            await Context.Client.DisconnectAsync();
            Context.Client.Dispose();
        }

        [Command("embed")]
        [Cooldowned(10)]
        public async Task Embed()
        {
            var embed = new EmbedBuilder();
            embed.AddField(build => build.Name = "nice test");
            embed.Description = "woaw";
            embed.Title = "cookies are the best :ok_hand:";
            await ReplyAsync(":ok_hand:", embed:
                             new EmbedBuilder()
                                 .AddField(b =>
                                 {
                                     b.Name = "my lord";
                                     b.Value = "wowowowow";
                                 })
                                 .WithColor(Color.Default)
                                 .WithTitle("how are you m8"));
        }

        [Command("report")]
        [Summary("report an error to the dev !")]
        public async Task Report([Remainder] string toSend)
        {
            Task<IDMChannel> dmChannelAsync =
                (await Context.Client.GetUserAsync("jeuxjeux20", "4664"))?.CreateDMChannelAsync();
            if (dmChannelAsync != null)
                // ReSharper disable once PossibleNullReferenceException
                await (await dmChannelAsync)?.SendMessageAsync
                    ($"Error report from {Context.User.Mention} : \n {toSend}");
            //if ((await Context.Guild.GetCurrentUserAsync()).GetPermissions((IGuildChannel)Context.Channel).AddReactions)
            await Context.Message.AddReactionAsync("✅");
        }

        [Command("invite")]
        [Summary("Returns the OAuth2 Invite URL of the bot")]
        public async Task Invite()
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            await ReplyAsync(
                $"A user with `MANAGE_SERVER` can invite me to your server here: <https://discordapp.com/oauth2/authorize?client_id={application.Id}&scope=bot&permissions=1073741888>");
        }

        [Command("leave")]
        [Summary("Instructs the bot to leave this Guild.")]
        [RequireOwner]
        public async Task Leave()
        {
            if (Context.Guild == null)
            {
                await ReplyAsync("This command can only be ran in a server.");
                return;
            }
            await ReplyAsync("Leaving~");
            await Context.Guild.LeaveAsync();
        }

        [Command("say")]
        [Alias("echo")]
        [Summary("Echos the provided input")]
        public async Task Say([Remainder] string input)
        {
            if (Context.User.IsBot) return;
            await ReplyAsync(input);
        }

        [Command("info")]
        [Summary("You really need a summary for this ?")]
        public async Task Info()
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            var discordSocketClient = Context.Client as DiscordSocketClient;
            if (discordSocketClient != null)
                await ReplyAsync(
                    $"**RPGgy** Version : **{Assembly.GetEntryAssembly().GetName().Version}**\n" +
                    $"- Author: {application.Owner.Username} (ID {application.Owner.Id})\n" +
                    $"- Library: Discord.Net ({DiscordConfig.Version})\n" +
                    $"- Runtime: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.OSArchitecture}\n" +
                    $"- Uptime: {GetUptime()}\n\n" +
                    $"{Format.Bold("Stats")}\n" +
                    $"- Heap Size: {GetHeapSize()} MB\n" +
                    $"- Guilds: {discordSocketClient.Guilds.Count}\n" +
                    $"- Channels: {discordSocketClient.Guilds.Sum(g => g.Channels.Count)}\n" +
                    $"- Users: {discordSocketClient.Guilds.Sum(g => g.Users.Count)}\n" +
                    "The source code of this bot is available of http://github.com/jeuxjeux20/RPGGY"                    
                );
        }

        [Command("Help")]
        public async Task Help()
        {
            IEnumerable<IGrouping<string, CommandInfo>> cmds = Program.Handler.Commands
                .Where(c => c.CheckPreconditionsAsync(Context, Program.Map).Result.IsSuccess)
                .GroupBy(c => c.Module.Aliases.FirstOrDefault());
            var str = new StringBuilder("Here are the commands you can use")
                    .AppendLine(
                        $"`{string.Join("`, `", cmds.SelectMany(g => g.Select(c => c.Aliases.FirstOrDefault())).Distinct())}`\n")
                ;
            await ReplyAsync(str.ToString());
        }

        [Command("help")]
        [Summary("Display how you can use a command.")]
        // This code is fropm Joe4evr
        public async Task HelpCmd(string cmdname)
        {
            var sb = new StringBuilder();
            IEnumerable<CommandInfo> cmds = Program.Handler.Commands
                .Where(c =>
                {
                    var firstOrDefault = c.Aliases.FirstOrDefault();
                    return firstOrDefault != null && firstOrDefault.Equals(cmdname, StringComparison.OrdinalIgnoreCase);
                });

            IEnumerable<CommandInfo> commandInfos = cmds as IList<CommandInfo> ?? cmds.ToList();
            if (commandInfos.Any())
            {
                sb.AppendLine($"`{commandInfos.First().Aliases.FirstOrDefault()}`");
                foreach (var cmd in commandInfos)
                {
                    sb.AppendLine('\t' +
                                  (string.IsNullOrWhiteSpace(cmd.Summary)
                                      ? "No description available, the dev is too lazy to add one :smiley:"
                                      : cmd.Summary));
                    if (cmd.Parameters.Count > 0)
                        sb.AppendLine($"\t\tParameters: {string.Join(" ", cmd.Parameters.Select(p => FormatParam(p)))}");
                }
            }
            else
            {
                return;
            }

            await ReplyAsync(sb.ToString());
        }

        // This code is fropm Joe4evr
        private string FormatParam(ParameterInfo param)
        {
            var sb = new StringBuilder();
            if (param.IsMultiple)
                sb.Append($"`[({param.Type.Name}): {param.Name}...]`");
            else if (param.IsRemainder) //&& IsOptional - decided not to check for the combination
                sb.Append($"`<({param.Type.Name}): {param.Name}...>`");
            else if (param.IsOptional)
                sb.Append($"`[({param.Type.Name}): {param.Name}]`");
            else
                sb.Append($"`<({param.Type.Name}): {param.Name}>`");

            if (!string.IsNullOrWhiteSpace(param.Summary))
                sb.Append($" ({param.Summary})");
            return sb.ToString();
        }

        private static string GetUptime()
            => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");

        private static string GetHeapSize()
            => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString(CultureInfo.InvariantCulture);
    }
}