using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using RPGgy.Game;
using RPGgy.Game.Fights;

namespace RPGgy.Permissions.Attributes
{
    internal class MustBeInFightAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(CommandContext context, CommandInfo command,
            IDependencyMap map)
        {
            var isThere = FightContext.ActualContexts.Keys.Any(
                test => test.Item1.Id == context.User.Id || test.Item2.Id == context.User.Id);
            if (!isThere)
                return Task.FromResult(PreconditionResult.FromError("You aren't in a fight, i guess."));
            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }

    internal class FightMustBeHisTurn : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(CommandContext context, CommandInfo command,
            IDependencyMap map)
        {
            var isThere = GameContext.WarriorsList.FirstOrDefault(war => war.IsOk(context.User));
            if (!isThere.AttachedFightContext.TurnOfEntity.Equals(isThere))
                return Task.FromResult(PreconditionResult.FromError("It isn't your turn."));
            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }

    internal class MusntBeInFight : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(CommandContext context, CommandInfo command,
            IDependencyMap map)
        {
            var isThere = FightContext.ActualContexts.Keys.Any(
                test => test.Item1.Id == context.User.Id || test.Item2.Id == context.User.Id);
            return
                Task.FromResult(isThere
                                    ? PreconditionResult.FromError("You're in a fight, idiot")
                                    : PreconditionResult.FromSuccess());
        }
    }

    internal class MusntBeInFightParameter : ParameterPreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(CommandContext context, ParameterInfo parameter,
            object value, IDependencyMap map)
        {
            var isThere = FightContext.ActualContexts.Keys.Any(
                test => test.Item1.Id == ((IUser) value).Id || test.Item2.Id == ((IUser) value).Id);
            return
                Task.FromResult(isThere
                                    ? PreconditionResult.FromError("The provided user is in a fight idiot !")
                                    : PreconditionResult.FromSuccess());
        }
    }
}