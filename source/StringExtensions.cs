using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cnzhnet.music_decrypt
{
    /// <summary>
    /// 用于扩展字符串的方法.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// 用一个字符串填充当前字符串（如果需要的话则重复填充）.
        /// </summary>
        /// <param name="s">当前字符串.</param>
        /// <param name="targetLength">当前字符串需要填充到的目标长度。如果这个数值小于当前字符串的长度，则返回当前字符串本身。</param>
        /// <param name="padString">填充字符串。如果字符串太长，使填充后的字符串长度超过了目标长度，则只保留最左侧的部分，其他部分会被截断。</param>
        /// <returns></returns>
        public static string PadEnd(this string s, int targetLength, string padString = null)
        {
            if (targetLength < s.Length)
                return s;
            StringBuilder sb = new StringBuilder(s);
            int i = 0, padLen = targetLength - s.Length;
            if (string.IsNullOrEmpty(padString))
            {
                for (; i < padLen; ++i)
                    sb.Append(" ");
                return sb.ToString();
            }

            while (sb.Length < targetLength)
            {
                if (padLen < padString.Length)
                    sb.Append(padString.Substring(0, padLen));
                else
                    sb.Append(padString);
                padLen = targetLength - sb.Length;
            }
            return sb.ToString();
        }
    }
}
