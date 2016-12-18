using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using RPGgy.Game;

namespace RPGgy.Permissions.Attributes
{
    class MustBeRegisteredAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(CommandContext context, CommandInfo command, IDependencyMap map)
        {
            var isThere = GameContext.WarriorsList.Any(war => war.IsOk(context.User));
            if (!isThere)
            {
                return Task.FromResult(PreconditionResult.FromError("You aren't registered, register using RPG.createuser ;)"));
            }
            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }

    class MustBeRegisteredParameterAttribute : ParameterPreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(CommandContext context, ParameterInfo parameter, object value, IDependencyMap map)
        {
            var userTo = value as IUser;
            if (userTo == null)
                return Task.FromResult(PreconditionResult.FromError("The parameter isn't an IUser"));
            var isThere = GameContext.WarriorsList.Any(war => war.IsOk(userTo));
            if (!isThere)
            {
                return Task.FromResult(PreconditionResult.FromError("The provided user isn't registered."));
            }
            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
