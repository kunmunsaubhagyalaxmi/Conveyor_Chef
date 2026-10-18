using TMPro;
using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// Coin and diamond counters: the amount always stays on one line. On narrow phones a
    /// counter's text box can be too narrow for "4,890", and TextMeshPro then broke it into
    /// "4,89" / "0". Every counter goes through here: no wrapping, the text shrinks a little
    /// when a long amount doesn't fit its box, and big amounts are shortened (125.3K, 1.2M).
    /// </summary>
    public static class CurrencyText
    {
        private const int ShortenFrom = 100000;   // 99,999 is shown in full, 100,000 as 100K
        private const float MinScale = 0.55f;     // never smaller than 55% of the designed size

        /// <summary>The amount as a counter shows it: 4,890 / 99,999 / 125K / 1.2M.</summary>
        public static string Amount(int amount)
        {
            return amount < ShortenFrom ? amount.ToString("N0") : CurrenciesHelper.Format(amount);
        }

        /// <summary>Shows the amount on one line in the counter's text.</summary>
        public static void Set(TMP_Text text, int amount)
        {
            if (text == null)
                return;
            Fit(text);
            string value = Amount(amount);
            if (text.text != value)
                text.text = value;
        }

        /// <summary>One line, and shrinks to fit its box instead of wrapping. Safe to call often.</summary>
        public static void Fit(TMP_Text text)
        {
            if (text == null)
                return;
            if (text.textWrappingMode != TextWrappingModes.NoWrap)
                text.textWrappingMode = TextWrappingModes.NoWrap;
            if (!text.enableAutoSizing)
            {
                float size = text.fontSize;
                text.fontSizeMax = size;
                text.fontSizeMin = Mathf.Max(8f, size * MinScale);
                text.enableAutoSizing = true;
            }
            if (text.overflowMode == TextOverflowModes.Truncate || text.overflowMode == TextOverflowModes.Ellipsis)
                text.overflowMode = TextOverflowModes.Overflow;
        }
    }
}
