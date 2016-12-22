using System.Threading.Tasks;
using Discord;

namespace RPGgy.Misc.Tools
{
    public static class Extensions
    {
        public static async Task DeleteAfter(this IMessage message, ushort milliseconds)
        {
                await Task.Delay(milliseconds);
                await message.DeleteAsync();
        }
        public static async Task DeleteAfter(this Task<IMessage> message, ushort milliseconds)
        {
                var kek = await message;
                await Task.Delay(milliseconds);
                await kek.DeleteAsync();
        }
    }
}
