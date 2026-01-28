using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SHLAPI.Utilities
{
    public static class ArabicTextProcessor
    {
        // Map of Arabic characters with their contextual forms: {isolated, final, initial, medial}
        private static readonly Dictionary<char, char[]> ARABIC_CHAR_MAP = new Dictionary<char, char[]>();

        // List of Arabic characters that don't connect to the following letter
        private static readonly HashSet<char> NON_CONNECTING_CHARS = new HashSet<char>();

        static ArabicTextProcessor()
        {
            // Populate character map
            ARABIC_CHAR_MAP['ا'] = new[] { '\uFE8D', '\uFE8E', '\uFE8D', '\uFE8E' };
            ARABIC_CHAR_MAP['ب'] = new[] { '\uFE8F', '\uFE90', '\uFE91', '\uFE92' };
            ARABIC_CHAR_MAP['ت'] = new[] { '\uFE95', '\uFE96', '\uFE97', '\uFE98' };
            ARABIC_CHAR_MAP['ث'] = new[] { '\uFE99', '\uFE9A', '\uFE9B', '\uFE9C' };
            ARABIC_CHAR_MAP['ج'] = new[] { '\uFE9D', '\uFE9E', '\uFE9F', '\uFEA0' };
            ARABIC_CHAR_MAP['ح'] = new[] { '\uFEA1', '\uFEA2', '\uFEA3', '\uFEA4' };
            ARABIC_CHAR_MAP['خ'] = new[] { '\uFEA5', '\uFEA6', '\uFEA7', '\uFEA8' };
            ARABIC_CHAR_MAP['د'] = new[] { '\uFEA9', '\uFEAA', '\uFEA9', '\uFEAA' };
            ARABIC_CHAR_MAP['ذ'] = new[] { '\uFEAB', '\uFEAC', '\uFEAB', '\uFEAC' };
            ARABIC_CHAR_MAP['ر'] = new[] { '\uFEAD', '\uFEAE', '\uFEAD', '\uFEAE' };
            ARABIC_CHAR_MAP['ز'] = new[] { '\uFEAF', '\uFEB0', '\uFEAF', '\uFEB0' };
            ARABIC_CHAR_MAP['س'] = new[] { '\uFEB1', '\uFEB2', '\uFEB3', '\uFEB4' };
            ARABIC_CHAR_MAP['ش'] = new[] { '\uFEB5', '\uFEB6', '\uFEB7', '\uFEB8' };
            ARABIC_CHAR_MAP['ص'] = new[] { '\uFEB9', '\uFEBA', '\uFEBB', '\uFEBC' };
            ARABIC_CHAR_MAP['ض'] = new[] { '\uFEBD', '\uFEBE', '\uFEBF', '\uFEC0' };
            ARABIC_CHAR_MAP['ط'] = new[] { '\uFEC1', '\uFEC2', '\uFEC3', '\uFEC4' };
            ARABIC_CHAR_MAP['ظ'] = new[] { '\uFEC5', '\uFEC6', '\uFEC7', '\uFEC8' };
            ARABIC_CHAR_MAP['ع'] = new[] { '\uFEC9', '\uFECA', '\uFECB', '\uFECC' };
            ARABIC_CHAR_MAP['غ'] = new[] { '\uFECD', '\uFECE', '\uFECF', '\uFED0' };
            ARABIC_CHAR_MAP['ف'] = new[] { '\uFED1', '\uFED2', '\uFED3', '\uFED4' };
            ARABIC_CHAR_MAP['ق'] = new[] { '\uFED5', '\uFED6', '\uFED7', '\uFED8' };
            ARABIC_CHAR_MAP['ك'] = new[] { '\uFED9', '\uFEDA', '\uFEDB', '\uFEDC' };
            ARABIC_CHAR_MAP['ل'] = new[] { '\uFEDD', '\uFEDE', '\uFEDF', '\uFEE0' };
            ARABIC_CHAR_MAP['م'] = new[] { '\uFEE1', '\uFEE2', '\uFEE3', '\uFEE4' };
            ARABIC_CHAR_MAP['ن'] = new[] { '\uFEE5', '\uFEE6', '\uFEE7', '\uFEE8' };
            ARABIC_CHAR_MAP['ه'] = new[] { '\uFEE9', '\uFEEA', '\uFEEB', '\uFEEC' };
            ARABIC_CHAR_MAP['و'] = new[] { '\uFEED', '\uFEEE', '\uFEED', '\uFEEE' };
            ARABIC_CHAR_MAP['ي'] = new[] { '\uFEF1', '\uFEF2', '\uFEF3', '\uFEF4' };
            ARABIC_CHAR_MAP['ئ'] = new[] { '\uFE8B', '\uFE8C', '\uFE8B', '\uFE8C' };
            ARABIC_CHAR_MAP['ء'] = new[] { '\uFE80', '\uFE80', '\uFE80', '\uFE80' };
            ARABIC_CHAR_MAP['ؤ'] = new[] { '\uFE85', '\uFE86', '\uFE85', '\uFE86' };
            ARABIC_CHAR_MAP['أ'] = new[] { '\uFE83', '\uFE84', '\uFE83', '\uFE84' };
            ARABIC_CHAR_MAP['إ'] = new[] { '\uFE87', '\uFE88', '\uFE87', '\uFE88' };
            ARABIC_CHAR_MAP['ة'] = new[] { '\uFE93', '\uFE94', '\uFE93', '\uFE94' };
            ARABIC_CHAR_MAP['ى'] = new[] { '\uFEEF', '\uFEF0', '\uFEEF', '\uFEF0' };
            ARABIC_CHAR_MAP['آ'] = new[] { '\uFE81', '\uFE82', '\uFE81', '\uFE82' };

            NON_CONNECTING_CHARS.UnionWith(new[]
            {
                'ا', 'أ', 'إ', 'آ', 'د', 'ذ', 'ر', 'ز', 'و', 'ؤ', 'ة', 'ى'
            });
        }

        public static string FixText(string input)
        {
            var result = new StringBuilder();
            var words = System.Text.RegularExpressions.Regex.Split(input, @"(?<=\s)|(?=\s)");

            foreach (var word in words)
            {
                if (ContainsArabic(word))
                {
                    string shaped = ShapeArabicWord(word.Trim());
                    result.Insert(0, shaped + " ");
                }
                else
                {
                    result.Insert(0, word);
                }
            }

            return result.ToString().Trim();
        }

        private static string ShapeArabicWord(string word)
        {
            var shaped = new StringBuilder();

            for (int i = 0; i < word.Length; i++)
            {
                char curr = word[i];
                char prev = i > 0 ? word[i - 1] : '\0';
                char next = i < word.Length - 1 ? word[i + 1] : '\0';

                if (!IsArabicLetter(curr))
                {
                    shaped.Append(curr);
                    continue;
                }

                bool connectPrev = i > 0 && IsArabicLetter(prev) && ConnectsToNext(prev);
                bool connectNext = i < word.Length - 1 && IsArabicLetter(next) && ConnectsToNext(curr);

                int formIndex = connectPrev && connectNext ? 3 :
                                connectPrev ? 1 :
                                connectNext ? 2 : 0;

                shaped.Append(ARABIC_CHAR_MAP[curr][formIndex]);
            }

            return new string(shaped.ToString().Reverse().ToArray());
        }

        private static bool IsArabicLetter(char ch)
        {
            return ARABIC_CHAR_MAP.ContainsKey(ch);
        }

        private static bool ConnectsToNext(char ch)
        {
            return !NON_CONNECTING_CHARS.Contains(ch);
        }

        private static bool ContainsArabic(string s)
        {
            return s.Any(c => CharUnicodeInfo.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.OtherLetter &&
                              c >= 0x0600 && c <= 0x06FF);
        }
    }
}
