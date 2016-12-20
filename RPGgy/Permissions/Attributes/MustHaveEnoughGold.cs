using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using RPGgy.Game;

namespace RPGgy.Permissions.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MustHaveEnoughGold : PreconditionAttribute
    {
        public MustHaveEnoughGold(uint omgTheGold)
        {
            Gawld = omgTheGold;
        }
        private uint Gawld { get; set; }
        public override Task<PreconditionResult> CheckPermissions(CommandContext context, CommandInfo command, IDependencyMap map)
        {
            var kek = GameContext.WarriorsList.FirstOrDefault(war => war.IsOk(context.User));
            string easterEgg = new Random(DateTime.Now.Millisecond).Next(1,10) == 1 ? " Try to use the command ?ahh how_to_get_rich !" : "";
            if (kek != null && kek.Gold < Gawld)
            {
                return
                    Task.FromResult(
                        PreconditionResult.FromError(
                            $"You don't have enough gold, needed is {Gawld} ; you have only {kek.Gold}.{easterEgg}"));
            }
            else
            {
                if (kek == null)
                    return
                        Task.FromResult(
                            PreconditionResult.FromError(
                                "God dammit the attributes order are messed up O_o just tell me why btw"));
                return Task.FromResult(PreconditionResult.FromSuccess());
            }
        }
    }
}
