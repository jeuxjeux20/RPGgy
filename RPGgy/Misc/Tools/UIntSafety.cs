using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGgy.Misc.Tools
{
    public static class UIntSafety
    {
        public static uint SafeSubstract(this uint original, uint toSubstract)
        {
            if (toSubstract > original)
            {
                return 0;
            }
            else
            {
                return original - toSubstract;
            }
        }
    }
}
