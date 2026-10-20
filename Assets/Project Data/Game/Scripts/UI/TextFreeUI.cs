using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Watermelon
{
    /// <summary>
    /// Puts the words (and icons) on the text-free art in the scenes, in the player's language:
    /// the Game scene's popups (level completed / failed, quit, in-game menu) and buttons made
    /// from the new pills. Objects are recognised by their name and their new art, so an object
    /// that does not use the new art is left alone. Runs when a scene loads (popups included,
    /// even while hidden) and again when the language changes.
    /// </summary>
    public static class TextFreeUI
    {
        private struct Rule
        {
            public string Colour, Icon, Key;
            public Rule(string colour, string icon, string key) { Colour = colour; Icon = icon; Key = key; }
        }

        // Buttons by object name (their picture must already be one of the new pills).
        private static readonly Dictionary<string, Rule> Buttons = new Dictionary<string, Rule>
        {
            { "Resume Button", new Rule("green", "play", "CONTINUE") },
            { "No Thanks Button", new Rule("green", null, "CONTINUE") },
            { "Close Button", new Rule("green", "play", "CONTINUE") },         // quit pop-up: keep playing
            { "Restart Button", new Rule("red", "restart", "RESTART") },
            { "Replay Button", new Rule("red", "restart", "RESTART") },
            { "Quit Button", new Rule("red", "restart", "RESTART") },
            { "Multiply Reward Button", new Rule("pink", "video", "GET X3") },
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            Loc.Changed -= ApplyAll;
            Loc.Changed += ApplyAll;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Apply(scene);

        private static void ApplyAll()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
                Apply(SceneManager.GetSceneAt(i));
        }

        public static void Apply(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
                return;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Image image in root.GetComponentsInChildren<Image>(true))
                    ApplyTo(image);
            }
        }

        private static void ApplyTo(Image image)
        {
            Sprite sprite = image.sprite;
            if (sprite == null)
                return;
            string art = sprite.name;

            if (art.StartsWith("pill_") && Buttons.TryGetValue(image.name, out Rule rule))
            {
                UIArt.Button(image, rule.Colour, rule.Icon, rule.Key);
                return;
            }
            switch (art)
            {
                case "title_ribbon_blue":
                    UIArt.Title(image, null, image.name.Contains("Settings") ? "SETTINGS" : "MENU", UIArt.BlueRibbonArea, UIArt.BlueTitleOutline);
                    break;
                case "level_completed_blank":
                    UIArt.Title(image, null, "LEVEL COMPLETED", UIArt.LevelCompletedArea, UIArt.BlueTitleOutline);
                    break;
                case "level_failed_blank":
                    UIArt.Title(image, null, "LEVEL FAILED", UIArt.LevelFailedArea, UIArt.BlueTitleOutline);
                    break;
            }
        }
    }
}
