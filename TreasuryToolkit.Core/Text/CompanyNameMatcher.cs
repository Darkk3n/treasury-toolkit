using System.Globalization;
using System.Text;

namespace TreasuryToolkit.Core.Text
{
    /// <summary>
    /// Compares company names as they appear in receipts against the names in the company list. Case, accents,
    /// spaces and punctuation are ignored, since PDF text extraction often splits or merges words
    /// ("SISTEMAS PACK" vs "SISTEMASPACK") and legal suffixes come with or without dots ("S.A." vs "SA").
    /// </summary>
    public static class CompanyNameMatcher
    {
        /// <summary>True when <paramref name="companyName"/> appears within <paramref name="foundName"/>.</summary>
        public static bool Contains(string foundName, string companyName)
        {
            string company = Normalize(companyName);
            return company.Length > 0 && Normalize(foundName).Contains(company, StringComparison.Ordinal);
        }

        internal static string Normalize(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            var builder = new StringBuilder(text.Length);
            foreach (char c in text.Normalize(NormalizationForm.FormD))
            {
                // FormD splits "Á" into "A" + a combining accent; only letters and digits are kept
                if (char.IsLetterOrDigit(c))
                {
                    builder.Append(char.ToUpperInvariant(c));
                }
            }

            return builder.ToString();
        }
    }
}
