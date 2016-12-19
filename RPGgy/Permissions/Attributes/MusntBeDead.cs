using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using RPGgy.Game;

namespace RPGgy.Permissions.Attributes
{
    public class MusntBeDead : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(CommandContext context, CommandInfo command,
            IDependencyMap map)
        {
            var kek = GameContext.WarriorsList.FirstOrDefault(war => war.IsOk(context.User));
            if (kek != null && kek.IsDead)
                return Task.FromResult(PreconditionResult.FromError("Hey, you're dead. You can't then :p"));
            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}