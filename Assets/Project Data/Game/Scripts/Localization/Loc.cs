using System;
using UnityEngine;

namespace Watermelon
{
    public enum GameLanguage
    {
        English, Hinglish, Spanish, Portuguese, Indonesian, French, German, Turkish,
    }

    /// <summary>
    /// The game's languages (Settings > LANGUAGE). <see cref="T"/> gives a text in the chosen
    /// language: keys are either an id ("tut.tray") or the English text itself ("DAILY GIFT").
    /// A text without a translation shows in English. The first time, the phone's language
    /// is used when the game has it (Hindi phones get Hinglish), otherwise English.
    /// Texts update at once when the language changes (<see cref="Changed"/>).
    /// </summary>
    public static class Loc
    {
        private const string PrefKey = "CC_Language";

        public static readonly GameLanguage[] All = (GameLanguage[])Enum.GetValues(typeof(GameLanguage));

        /// <summary>Each language's own name, as the picker shows it.</summary>
        public static string NativeName(GameLanguage language)
        {
            switch (language)
            {
                case GameLanguage.Hinglish: return "Hinglish";
                case GameLanguage.Spanish: return "Español";
                case GameLanguage.Portuguese: return "Português";
                case GameLanguage.Indonesian: return "Indonesia";
                case GameLanguage.French: return "Français";
                case GameLanguage.German: return "Deutsch";
                case GameLanguage.Turkish: return "Türkçe";
                default: return "English";
            }
        }

        private static bool loaded;
        private static GameLanguage current;

        /// <summary>Raised after the language changed; open screens re-read their texts.</summary>
        public static event Action Changed;

        public static GameLanguage Current
        {
            get
            {
                if (!loaded)
                {
                    loaded = true;
                    int saved = PlayerPrefs.GetInt(PrefKey, -1);
                    current = saved >= 0 && saved < All.Length ? (GameLanguage)saved : FromSystem(Application.systemLanguage);
                }
                return current;
            }
        }

        public static void Set(GameLanguage language)
        {
            loaded = true;
            if (current == language && PlayerPrefs.HasKey(PrefKey))
                return;
            current = language;
            PlayerPrefs.SetInt(PrefKey, (int)language);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        /// <summary>The text for <paramref name="key"/> in the current language (English when missing).</summary>
        public static string T(string key)
        {
            if (string.IsNullOrEmpty(key))
                return key;
            if (LocTable.Texts.TryGetValue(key, out string[] row))
            {
                int i = (int)Current;
                if (i < row.Length && !string.IsNullOrEmpty(row[i]))
                    return row[i];
                return string.IsNullOrEmpty(row[0]) ? key : row[0];
            }
            return key;
        }

        /// <summary>Capital letters the way the current language writes them (Turkish i -> İ).</summary>
        public static string Upper(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            return Current == GameLanguage.Turkish
                ? text.ToUpper(new System.Globalization.CultureInfo("tr-TR"))
                : text.ToUpperInvariant();
        }

        /// <summary><see cref="T"/> with {0}, {1}... filled in.</summary>
        public static string F(string key, params object[] args)
        {
            try { return string.Format(T(key), args); }
            catch (FormatException) { return T(key); }
        }

        private static GameLanguage FromSystem(SystemLanguage system)
        {
            switch (system)
            {
                case SystemLanguage.Spanish: return GameLanguage.Spanish;
                case SystemLanguage.Portuguese: return GameLanguage.Portuguese;
                case SystemLanguage.Indonesian: return GameLanguage.Indonesian;
                case SystemLanguage.French: return GameLanguage.French;
                case SystemLanguage.German: return GameLanguage.German;
                case SystemLanguage.Turkish: return GameLanguage.Turkish;
                case SystemLanguage.Hindi: return GameLanguage.Hinglish;
                default: return GameLanguage.English;
            }
        }
    }
}
