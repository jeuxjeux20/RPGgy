using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using RPGgy.Game.Player;

namespace RPGgy.Permissions.Attributes
{
    public class UserMusntBeDeadParameterAttribute : ParameterPreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(CommandContext context, ParameterInfo parameter, object value, IDependencyMap map)
        {
            var user = value as IUser;
            if (user == null)
            {
                return
                    Task.FromResult(
                        PreconditionResult.FromError("The following parameter isn't a fooking user, the dev was drunk."));
            }
            var kek = WarriorUser.GetUser(user);
            return Task.FromResult(kek.IsDead ? PreconditionResult.FromError("The following user is dead.") : PreconditionResult.FromSuccess());
        }
    }
}