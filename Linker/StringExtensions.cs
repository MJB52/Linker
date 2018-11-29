using System;
using System.Collections.Generic;
using System.Text;

namespace Linker
{
    public static class StringExtensions
    {
        public static string IncrementInHex(this string value, string incValue)
        {
            System.Globalization.NumberStyles num = System.Globalization.NumberStyles.HexNumber;
            int temp = int.Parse(value, num) + int.Parse(incValue, num);
            return temp.ToString("X");
        }
        public static string DecrementInHex(this string value, string incValue)
        {
            System.Globalization.NumberStyles num = System.Globalization.NumberStyles.HexNumber;
            int temp = int.Parse(value, num) - int.Parse(incValue, num);
            return temp.ToString("X");
        }
    }
}
