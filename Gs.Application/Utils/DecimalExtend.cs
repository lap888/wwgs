using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Application.Utils
{
    public static class DecimalExtend
    {
        public static decimal ToFixed(this decimal d, int n)
        {
            var strDecimal = d.ToString();
            var index = strDecimal.IndexOf(".", StringComparison.CurrentCulture);
            if (index == -1 || strDecimal.Length < index + n + 1)
            {
                strDecimal = string.Format("{0:F" + n + "}", d);
            }
            else
            {
                int length = index;
                if (n != 0)
                {
                    length = index + n + 1;
                }
                strDecimal = strDecimal.Substring(0, length);
            }
            return decimal.Parse(strDecimal);
        }
    }
}
