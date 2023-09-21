using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

namespace PSXPrev.Common.Utils
{
    public static class StringUtils
    {
        public static string ToBitString(this BitArray bits)
        {
            var sb = new StringBuilder();

            for (var i = 0; i < bits.Count; i++)
            {
                sb.Append(bits[i] ? '1' : '0');
            }

            return sb.ToString();
        }

        // Replaces the text of a Regex Match or Group. This DOES NOT use substitution replacement.
        public static string Replace(this string input, Capture capture, string replacement)
        {
            return input.Substring(0, capture.Index) + replacement + input.Substring(capture.Index + capture.Length);
        }

        public static Regex WildcardToRegex(string wildcard, TimeSpan? matchTimeout = null)
        {
            // This is not Windows compliant: <https://ss64.com/nt/syntax-wildcards.html>
            // Windows wildcard is an absolute mess of edge cases and wonky behavior.
            // It would be more confusing to the user if we DID try to support it.
            var s = wildcard;
            if (s.Length > 0)
            {
                // We don't want repeated instances of the same "match-anything" character.
                // Replace even-number-repeating and then odd-number-repeating
                s = s.Replace("**", "*").Replace("**", "*");
                s = Regex.Escape(s);
                s = s.Replace(@"\*", ".*").Replace(@"\?", "[^.]");
                s = "^" + s + "$";
            }
            else
            {
                s = "^.*$";
            }
            if (matchTimeout.HasValue)
            {
                return new Regex(s, RegexOptions.IgnoreCase, matchTimeout.Value);
            }
            else
            {
                return new Regex(s, RegexOptions.IgnoreCase);
            }
        }
    }
}
