using System;
using System.Diagnostics;
using System.Text;

namespace Project_Kittan.Helpers
{
    public static class StringExtensions
    {
        [DebuggerStepThrough]
        public static string Replace(this string str, string oldValue, string @newValue, StringComparison comparisonType)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            if (str.Length == 0)
            {
                return str;
            }

            if (oldValue == null)
            {
                throw new ArgumentNullException(nameof(oldValue));
            }

            if (oldValue.Length == 0)
            {
                throw new ArgumentException("String cannot be of zero length.");
            }

            StringBuilder resultStringBuilder = new StringBuilder(str.Length);

            bool isReplacementNullOrEmpty = string.IsNullOrEmpty(@newValue);

            const int valueNotFound = -1;
            int foundAt;
            int startSearchFromIndex = 0;
            while ((foundAt = str.IndexOf(oldValue, startSearchFromIndex, comparisonType)) != valueNotFound)
            {
                int @charsUntilReplacment = foundAt - startSearchFromIndex;
                bool isNothingToAppend = @charsUntilReplacment == 0;
                if (!isNothingToAppend)
                {
                    resultStringBuilder.Append(str, startSearchFromIndex, @charsUntilReplacment);
                }

                if (!isReplacementNullOrEmpty)
                {
                    resultStringBuilder.Append(@newValue);
                }

                startSearchFromIndex = foundAt + oldValue.Length;
                if (startSearchFromIndex == str.Length)
                {
                    return resultStringBuilder.ToString();
                }
            }

            int @charsUntilStringEnd = str.Length - startSearchFromIndex;
            resultStringBuilder.Append(str, startSearchFromIndex, @charsUntilStringEnd);

            return resultStringBuilder.ToString();
        }
    }
}
