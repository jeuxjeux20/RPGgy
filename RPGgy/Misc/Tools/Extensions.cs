using System.Threading.Tasks;
using Discord;

namespace RPGgy.Misc.Tools
{
    public static class Extensions
    {
        public static Task DeleteAfter(this IMessage message, ushort milliseconds)
        {
            return new Task(async () =>
            {
                await Task.Delay(milliseconds);
                await message.DeleteAsync();
            });
        }
        public static Task DeleteAfter(this Task<IMessage> message, ushort milliseconds)
        {
            return new Task(async () =>
            {
                await Task.Delay(milliseconds);
                await (await message).DeleteAsync();
            });
        }
    }
}
