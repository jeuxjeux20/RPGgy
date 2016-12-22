using System;

namespace RPGgy.Misc.Tools
{
    internal class AsciiBar
    {
        public static string DrawProgressBar(uint percent)
        {
            var progress = new char[22];
            progress[0] = '[';
            progress[21] = ']';
            if (percent > 100)
                percent = 100;
            var charsToFill = percent / (float) 5;
            byte lel = 1;
            for (var i = 0; i < charsToFill; i++)
            {
                progress[lel] = '#';
                lel++;
            }
            for (var i = 0; i < progress.Length; i++)
                if (progress[i] != '[' && progress[i] != ']' && progress[i] != '#')
                    progress[i] = '-';
            return new string(progress);
        }

        public static string DrawProgressBar(uint first, uint max)
        {
            var percent = (float) (first / (double) max) * 100;
            var progress = new char[22];
            progress[0] = '[';
            progress[21] = ']';
            if (percent > 100)
                percent = 100;
            var charsToFill = percent / 5;
            byte lel = 1;
            for (var i = 0; i < charsToFill; i++)
            {
                progress[lel] = '#';
                lel++;
            }
            for (var i = 0; i < progress.Length; i++)
                if (progress[i] != '[' && progress[i] != ']' && progress[i] != '#')
                    progress[i] = '-';
            return new string(progress);
        }

        internal static object DrawProgressBar(int lifePoints, int maxLife)
        {
            return DrawProgressBar((uint) lifePoints, (uint) maxLife);
        }
    }
}