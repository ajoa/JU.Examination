using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace JU.Examination.CustomExtensions
{
    /// <summary>
    /// Extension methods for string.
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// Guessing if the string looks like a Ladok stored Civic number.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns><c>true</c> if looks like a ladok stored Civic number, <c>false</c> otherwise.</returns>
        public static bool CanBeLadokCivicNumber(this string str)
        {
            if (String.IsNullOrEmpty(str))
            {
                return false;
            }

            // Ex: 19700126 6639
            string civicnumber = str.Replace("-", "").Replace("+", "");
            return ValidateCivicNumber(civicnumber);
        }

        /// <summary>
        /// Formats a civic number to Ladoks data format, ie 12 characters long.
        /// Because it is Ladok format, we assume that the person is over 14 years old when we determine which century it was born.
        /// </summary>
        /// <param name="str">A correct civic number in any format 10-13 characters long. Example: yymmddxxxx, yymmdd-xxxx, yyyymmddxxxx, yyyymmdd-xxxx</param>
        /// <returns>12 characters long civic number. Note If parameter is incorrect, return it to existing format.</returns>
        public static string CivicNumberToLadokFormat(this string str)
        {
            string civicNumber = str.Replace("-", "").Replace("+", "");

            // fix civicNumber to 12 characters
            if (civicNumber.Length == 10 && civicNumber.Substring(0, 6).IsNumeric())
            {
                DateTime twentyFirstCentury = new DateTime(Convert.ToInt32("20" + civicNumber.Substring(0, 2)), Convert.ToInt32(civicNumber.Substring(2, 2)), Convert.ToInt32(civicNumber.Substring(4, 2)));
                if (twentyFirstCentury < DateTime.Now.AddYears(-14))
                {
                    // Aha it may be the 2000's anyway.
                    civicNumber = "20" + civicNumber;
                }
                else
                {
                    // Otherwise, it may be 1900's
                    civicNumber = "19" + civicNumber;
                }
            }

            if (ValidateCivicNumber(civicNumber))
            {
                return civicNumber;
            }
            else
            {
                return str;
            }
        }

        /// <summary>
        /// A civic number in display format, that is yymmmdd-xxxx.
        /// </summary>
        /// <param name="str">A correct civic number in any format 10-13 characters long. Example: yymmddxxxx, yymmdd-xxxx, yyyymmddxxxx, yyyymmdd-xxxx</param>
        /// <returns>9 characters long civic number, that is yymmmdd-xxxx. Note If parameter is incorrect, return it to existing format.</returns>
        public static string CivicNumberToDisplayFormat(this string str)
        {
            string civicNumber = str.CivicNumberToLadokFormat();

            if (ValidateCivicNumber(civicNumber))
            {
                return civicNumber.Substring(2, 6) + "-" + civicNumber.Substring(8);
            }
            else
            {
                return str;
            }
        }

        /// <summary>
        /// Capitalize a string.
        /// Default according to Swedish rules. For example, Anna Maria = Anna-Maria
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="onelyFirstLetter">If only the first letter should Capitalize. Otherwise, capitalization according to Swedish rules. For example, Anna Maria = Anna-Maria.</param>
        /// <returns>The capitalized string</returns>
        public static string Capitalize(this string str, bool onelyFirstLetter = false)
        {
            try
            {
                if (onelyFirstLetter)
                {
                    return str.ToLower().First().ToString().ToUpper() + String.Join("", str.Skip(1)); // Only first letter big
                }

                // Capitalize according to Swedish rules. For example, Anna Maria = Anna-Maria
                System.Globalization.CultureInfo cultureInfo = new System.Globalization.CultureInfo("sv-SE");
                System.Globalization.TextInfo textInfo = cultureInfo.TextInfo;
                return textInfo.ToTitleCase(str.ToLower());
            }
            catch
            {
                return str;
            }
        }

        /// <summary>
        /// Determines whether the specified string is numeric.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns><c>true</c> if the specified string is numeric; otherwise, <c>false</c>.</returns>
        public static bool IsNumeric(this string str)
        {
            if (String.IsNullOrEmpty(str))
            {
                return false;
            }

            str = str.TrimStart('0'); // Removes zeroes at the beginning or our control will fail

            bool isNumeric;
            double retNum;
            isNumeric = Double.TryParse(str, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out retNum);
            if (isNumeric && str == Convert.ToInt64(retNum).ToString())
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the provided civic number is valid.
        /// </summary>
        /// <param name="civicNumber">The civic number.</param>
        /// <returns>
        /// <c>true</c> if the parameter passes validation, otherwise <c>false</c>.
        /// </returns>
        private static bool ValidateCivicNumber(string civicNumber)
        {
            // Test pre-conditions
            if (String.IsNullOrWhiteSpace(civicNumber))
            {
                return false;
            }

            if (12 != civicNumber.Length)
            {
                return false;
            }

            string year = civicNumber.Substring(0, 4);
            string month = civicNumber.Substring(4, 2);
            string day = civicNumber.Substring(6, 2);
            string endingChars = civicNumber.Substring(8, 4);
            int verificationDigit;

            if (!int.TryParse(civicNumber.Substring(11, 1), out verificationDigit))
            {
                return false;
            }

            // validate year, month, day (day spans from 01-31 for default days and 61-91 for coordination number days (samordningsnummer))
            if (!Regex.IsMatch(year, "^19|20[0-9]{2}$") || !Regex.IsMatch(month, "^(0[1-9]|1[012])$") ||
                !Regex.IsMatch(day, "^(0[1-9]|1[0-9]|2[0-9]|3[0-1]|6[1-9]|7[0-9]|8[0-9]|9[0-1])$"))
            {
                return false;
            }

            int digits;

            // ending chars are numeric
            if (int.TryParse(endingChars, out digits))
            {
                // Luhn algorithm
                string civicNumberValidationValue = civicNumber.Substring(2, 9);
                char[] civicNumberArray = civicNumberValidationValue.ToCharArray();
                int checksum = 0;
                int multiplier = 2;

                for (int i = 0; i < civicNumberArray.Length; i++)
                {
                    int digit;

                    if (!int.TryParse(civicNumberArray[i].ToString(), out digit))
                    {
                        return false;
                    }

                    int checkSumValue = digit * multiplier;

                    if (checkSumValue >= 10)
                    {
                        checksum += checkSumValue / 10;
                        checksum += checkSumValue % 10;
                    }
                    else
                    {
                        checksum += checkSumValue;
                    }

                    multiplier = multiplier == 1 ? 2 : 1;
                }

                int checkSumVerificationDigit = (checksum % 10 == 0) ? 0 : 10 - (checksum % 10);

                if (checkSumVerificationDigit != verificationDigit)
                {
                    return false;
                }
            }
            else
            {
                // 'reservnummer' last chars (may include letters, e.g 't' and 'p' followed by three digits)
                // Change pos can be any letter
                // Egentligen ska J, K, L, M, R, S, T, U, W, X vara godkänt enligt https://confluence.its.umu.se/confluence/display/LDSV/Interimspersonnummer
                if (!Regex.IsMatch(endingChars, "^([a-zA-Z][0-9]{3})$")) // "^([t|T|p|P][0-9]{3})$")) 
                {
                    return false;
                }
            }

            return true;
        }
    }
}