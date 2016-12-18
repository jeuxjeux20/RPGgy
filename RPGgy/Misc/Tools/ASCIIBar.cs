namespace RPGgy.Misc.Tools
{
    internal class AsciiBar
    {
        public static string DrawProgressBar(uint percent)
        {
            char[] progress = new char[22];
            progress[0] = '[';
            progress[21] = ']';
            if (percent > 100)
                percent = 100;
            float charsToFill = percent / (float)5;
            byte lel = 1;
            for (int i = 0; i < charsToFill; i++)
            {
                progress[lel] = '#';
                lel++;
            }
            for (int i = 0; i < progress.Length; i++)
            {
                if (progress[i] != '[' && progress[i] != ']' && progress[i] != '#')
                {
                    progress[i] = '-';
                }
            }
            return new string(progress);
        }
        public static string DrawProgressBar(int first,int max)
        {
            var percent = (float)(first / (double)max) * 100;
            char[] progress = new char[22];
            progress[0] = '[';
            progress[21] = ']';
            if (percent > 100)
                percent = 100;
            float charsToFill = percent / (float)5;
            byte lel = 1;
            for (int i = 0; i < charsToFill; i++)
            {
                progress[lel] = '#';
                lel++;
            }
            for (int i = 0; i < progress.Length; i++)
            {
                if (progress[i] != '[' && progress[i] != ']' && progress[i] != '#')
                {
                    progress[i] = '-';
                }
            }
            return new string(progress);
        }
    }
}
