using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.API;
using Discord.Commands;

namespace RPGgy.Permissions.Attributes
{
    class UserMustBeOnlineParameter : Discord.Commands.ParameterPreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(CommandContext context, ParameterInfo parameter, object value, IDependencyMap map)
        {
            var user = value as IUser;
            if (user == null)
                return Task.FromResult(PreconditionResult.FromError("The given user is offline."));
            else
                return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
