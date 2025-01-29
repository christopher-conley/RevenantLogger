using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosettaTools.Pwsh.Text.RevenantLogger.Common.ExtensionMethods
{
    public static class StringExtensions2
    {
        public static bool IsNotNullOrWhiteSpace(this string? input)
        {
            return !string.IsNullOrEmpty(input);
        }

        // I'm lazy and it's two less characters, stop judging me
        public static bool NotNullOrWhiteSpace(this string? input)
        {
            return !string.IsNullOrEmpty(input);
        }
    }
}
