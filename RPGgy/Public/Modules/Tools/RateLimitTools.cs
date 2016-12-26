using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Net;

namespace RPGgy.Public.Modules.Tools
{
    public static class RateLimitTools
    {
        public static async Task<IMessage> RetryRatelimits(this Func<Task<IMessage>> action)
        {
            IMessage kekMessage;
            while (true)
            {
                try
                {
                    kekMessage = await action();
                    break;
                }
                catch (RateLimitedException)
                {
                    await Task.Delay(750);
                }
            }
            return kekMessage;
        }
        public static async Task<IUserMessage> RetryRatelimits(this Task<IUserMessage> action)
        {
            IUserMessage kekMessage;
            while (true)
            {
                try
                {
                    kekMessage = await action;
                    break;
                }
                catch (RateLimitedException)
                {
                    await Task.Delay(750);
                }
            }
            return kekMessage;
        }
    }
}
