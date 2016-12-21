using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace RPGgy.Permissions.Attributes
{
    class UserParameterMusntBeItself : ParameterPreconditionAttribute
    {
        public UserParameterMusntBeItself(string message = null)
        {
            Message = message;
        }
        private string Message { get; }
        public override Task<PreconditionResult> CheckPermissions(CommandContext context, ParameterInfo parameter, object value, IDependencyMap map)
        {
            IUser checkUser = value as IUser;
            if (checkUser == null)
                return Task.FromResult(PreconditionResult.FromError("The parameter isn't an user, the dev was on aids."));
            if (checkUser == context.User)
                return Task.FromResult(PreconditionResult.FromError(Message ?? "Providing yourself as a parameter is not allowed."));
            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
