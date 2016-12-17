using System;
using System.Threading.Tasks;
using Discord.Commands;
using RPGgy.Permissions.Checkers;

namespace RPGgy.Permissions.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CooldownedAttribute : PreconditionAttribute
    {
        public CooldownedAttribute(int cooldownSeconds)
        {
            MethodCooldown = new Cooldown(cooldownSeconds);
        }

        private Cooldown MethodCooldown { get; }

        public override Task<PreconditionResult> CheckPermissions(CommandContext context, CommandInfo command,
            IDependencyMap map)
        {
            if (!MethodCooldown.IsFinished)
                return Task.FromResult(
                    PreconditionResult.FromError(
                        $"This command has been cooldowned for {MethodCooldown.SecondsLeft:## 'seconds'}"));
            MethodCooldown.Restart();
            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}