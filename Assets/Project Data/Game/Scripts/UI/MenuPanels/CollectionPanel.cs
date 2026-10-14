using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.BusStop;

namespace Watermelon
{
    /// <summary>
    /// COLLECTION: every country dish, one tab per continent and a flag strip to pick the
    /// country. Discovered dishes show their picture on a tile in their tray colour with how
    /// often they were served; the rest are silhouettes until the player serves them (or
    /// completes a level of that country).
    /// Serving each of a country's 10 dishes at least once earns its "Country Master" badge
    /// and a reward, claimed here (saved as a PlayerStats flag).
    /// </summary>
    public sealed class CollectionPanel : MenuPanel
    {
        private static readonly string[] ContinentTabs = { "ASIA", "N.AM", "S.AM", "EUROPE", "AFRICA", "OCEANIA", "POLAR" };

        public const int MasterCoins = 300;
        public const int MasterDiamonds = 5;

        private const int Columns = 3;
        private const float CellWidth = 238f;
        private const float CellHeight = 300f;
        private const float Spacing = 16f;

        private int countryInContinent;

        protected override string Title => "COLLECTION";
        protected override string[] Tabs => ContinentTabs;

        private int SelectedCountry => SelectedTab * WorldCatalog.CountriesPerContinent + countryInContinent;

        /// <summary>Dishes of a country served at least once.</summary>
        public static int DishesServedIn(int country)
        {
            int served = 0;
            for (int colour = 0; colour < CountryFoodArt.ColoursPerCountry; colour++)
            {
                if (PlayerStats.GetDishServed(PlayerStats.GetDishIndex(country, colour)) > 0)
                    served++;
            }
            return served;
        }

        public static bool IsMasterClaimed(int country) => PlayerStats.GetFlag("country_master_" + country);

        public static int MastersClaimed
        {
            get
            {
                int count = 0;
                for (int country = 0; country < WorldCatalog.CountryCount; country++)
                {
                    if (IsMasterClaimed(country))
                        count++;
                }
                return count;
            }
        }

        protected override void OnOpened()
        {
            // Open on the country being played.
            int country = CountryFood.CurrentCountry;
            if (!WorldCatalog.IsValidCountry(country))
                return;
            countryInContinent = country % WorldCatalog.CountriesPerContinent;
            int continent = WorldCatalog.ContinentOfCountry(country);
            if (continent != SelectedTab)
                SelectTab(continent);
        }

        protected override void BuildContent()
        {
            int country = SelectedCountry;
            BuildSummary();
            BuildCountryStrip();
            BuildMaster(country);
            BuildGrid(country);
        }

        private Sprite Flag(int country)
        {
            Sprite flag = WorldCatalog.ContinentOfCountry(country) == 0 ? Art.GetFlag(country) : null;
            return flag != null ? flag : WorldArt.ForCountry(country, WorldArt.FlagBadge);
        }

        private void BuildSummary()
        {
            int discovered = PlayerStats.DishesDiscovered;
            RectTransform card = MenuUI.Card("Summary", Content, 170f, false);

            Image book = MenuUI.Image("Book", card, Art.iconCollection);
            MenuUI.Anchor(book.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(20f, 0f), new Vector2(140f, 130f));

            TextMeshProUGUI title = MenuUI.Text("Title", card, $"DISHES DISCOVERED  {discovered}/{PlayerStats.TotalDishes}", 34f, new Color32(160, 56, 18, 255), TextAlignmentOptions.Left);
            MenuUI.Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(176f, -20f), new Vector2(560f, 44f));

            RectTransform bar = MenuUI.ProgressBar(card, (float)discovered / PlayerStats.TotalDishes, MenuUI.Gold);
            MenuUI.Anchor(bar, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(176f, -72f), new Vector2(560f, 38f));

            TextMeshProUGUI masters = MenuUI.Text("Masters", card, $"Country Master badges: {MastersClaimed}/{WorldCatalog.CountryCount}", 28f, MenuUI.TextSoft, TextAlignmentOptions.Left);
            MenuUI.Anchor(masters.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(176f, 14f), new Vector2(560f, 40f));
        }

        // The continent's five flags; the chosen country sits on a gold tile.
        private void BuildCountryStrip()
        {
            RectTransform strip = MenuUI.Rect("Countries", Content);
            strip.gameObject.AddComponent<LayoutElement>().preferredHeight = 150f;
            HorizontalLayoutGroup layout = strip.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 12f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            for (int i = 0; i < WorldCatalog.CountriesPerContinent; i++)
            {
                int index = i;
                int country = SelectedTab * WorldCatalog.CountriesPerContinent + i;
                bool selected = i == countryInContinent;
                Color fill = selected ? (Color)new Color32(255, 226, 140, 255) : MenuUI.CardFill;
                Image tile = MenuUI.Image("Country " + country, strip, MenuUI.Rounded(22, selected ? 5 : 3, fill, selected ? MenuUI.Gold : MenuUI.CardBorder), null, false);
                MenuUI.Button(tile, () =>
                {
                    if (index == countryInContinent)
                        return;
                    countryInContinent = index;
                    Refresh();
                });

                Image flag = MenuUI.Image("Flag", tile.transform, Flag(country));
                MenuUI.Anchor(flag.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -10f), new Vector2(84f, 84f));

                TextMeshProUGUI name = MenuUI.Text("Name", tile.transform, CountryCatalog.GetName(country), 22f, MenuUI.TextDark);
                MenuUI.Anchor(name.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 26f), new Vector2(130f, 30f));
                TextMeshProUGUI count = MenuUI.Text("Served", tile.transform, DishesServedIn(country) + "/" + CountryFoodArt.ColoursPerCountry, 20f,
                    IsMasterClaimed(country) ? MenuUI.Green : MenuUI.TextSoft);
                MenuUI.Anchor(count.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 4f), new Vector2(130f, 26f));
            }
        }

        // Country Master: serve every dish of the country once, then claim the reward.
        private void BuildMaster(int country)
        {
            int served = DishesServedIn(country);
            int total = CountryFoodArt.ColoursPerCountry;
            bool done = served >= total;
            bool claimed = IsMasterClaimed(country);

            RectTransform card = MenuRow.Create("Country Master", Content, 200f, done && !claimed, Flag(country), !done && !claimed);
            MenuRow.Title(card, CountryCatalog.GetName(country).ToUpperInvariant() + " MASTER", claimed ? new Color32(176, 120, 0, 255) : (Color?)null);
            MenuRow.Subtitle(card, claimed ? "Badge earned!" : "Serve all " + total + " dishes once");
            MenuRow.Bar(card, (float)served / total, done ? MenuUI.Gold : MenuUI.Green, served + " / " + total + " served");

            if (claimed)
            {
                MenuRow.Done(card, Art.check, "MASTER");
            }
            else if (done)
            {
                MenuRow.RightPill(card, Art.coin, MasterCoins.ToString("N0"), 52f);
                MenuRow.Button(card, Art.buttonGreen, "CLAIM", () =>
                {
                    if (IsMasterClaimed(country) || DishesServedIn(country) < total)
                        return;
                    PlayerStats.SetFlag("country_master_" + country);
                    Grant(CurrencyType.Coins, MasterCoins);
                    Grant(CurrencyType.Diamonds, MasterDiamonds);
                    Toast(CountryCatalog.GetName(country) + " Master! +" + MasterCoins + " coins, +" + MasterDiamonds + " gems", new Color32(150, 100, 0, 255));
                    Refresh();
                }, new Color32(30, 90, 20, 255), null, -30f, 88f);
            }
            else
            {
                Image rewardBack = MenuUI.Image("Reward Box", card, MenuUI.Rounded(26, 3, new Color32(255, 238, 200, 255), MenuUI.CardBorder), null, false);
                MenuUI.Anchor(rewardBack.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-MenuRow.Margin, 0f), new Vector2(MenuRow.RightWidth, 150f));
                TextMeshProUGUI label = MenuUI.Text("Label", rewardBack.transform, "REWARD", 24f, MenuUI.TextSoft);
                MenuUI.Anchor(label.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(180f, 30f));
                RectTransform coins = MenuUI.Pill("Coins", rewardBack.transform, Art.coin, MasterCoins.ToString("N0"), 32f, MenuUI.TextDark);
                MenuUI.Anchor(coins, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -42f), new Vector2(180f, 46f));
                RectTransform gems = MenuUI.Pill("Gems", rewardBack.transform, Art.diamond, MasterDiamonds.ToString(), 32f, MenuUI.TextDark);
                MenuUI.Anchor(gems, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -94f), new Vector2(180f, 46f));
            }
        }

        private void BuildGrid(int country)
        {
            int count = CountryFoodArt.ColoursPerCountry;
            int rows = Mathf.CeilToInt(count / (float)Columns);

            RectTransform grid = MenuUI.Rect("Grid", Content);
            grid.gameObject.AddComponent<LayoutElement>().preferredHeight = rows * CellHeight + (rows - 1) * Spacing;
            GridLayoutGroup layout = grid.gameObject.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(CellWidth, CellHeight);
            layout.spacing = new Vector2(Spacing, Spacing);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = Columns;

            for (int colour = 0; colour < count; colour++)
                BuildDish(grid, country, colour);
        }

        private void BuildDish(Transform grid, int country, int colour)
        {
            int dish = PlayerStats.GetDishIndex(country, colour);
            bool discovered = PlayerStats.IsDishDiscovered(dish);
            int served = PlayerStats.GetDishServed(dish);
            Color tint = CountryFood.GetTypeColour((LevelElement.Type)((int)LevelElement.Type.Block_Red + colour));

            Image cell = MenuUI.Image("Dish " + dish, grid, MenuUI.Rounded(26, 4, MenuUI.CardFill, MenuUI.CardBorder), null, false);

            Color tileColour = discovered ? Color.Lerp(tint, Color.white, 0.55f) : new Color32(214, 206, 196, 255);
            Image tile = MenuUI.Image("Tile", cell.transform, MenuUI.Rounded(22, 0, tileColour, tileColour), null, false);
            MenuUI.Anchor(tile.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -12f), new Vector2(CellWidth - 24f, 190f));

            Sprite sprite = CountryFood.GetDish(country, colour);
            Image picture = MenuUI.Image("Picture", tile.transform, sprite);
            MenuUI.Stretch(picture.rectTransform, 10f, 8f, 10f, 8f);
            if (!discovered)
            {
                picture.color = new Color(0.18f, 0.12f, 0.1f, 0.55f);
                TextMeshProUGUI mark = MenuUI.OutlinedText("Unknown", tile.transform, "?", 90f, new Color32(60, 40, 30, 255));
                MenuUI.Stretch(mark.rectTransform);
            }

            TextMeshProUGUI name = MenuUI.Text("Name", cell.transform, discovered ? WorldCatalog.GetDishName(country, colour) : "???", 31f, MenuUI.TextDark);
            MenuUI.Anchor(name.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 48f), new Vector2(CellWidth - 20f, 40f));

            TextMeshProUGUI detail = MenuUI.Text("Served", cell.transform,
                discovered ? (served > 0 ? "Served x" + served.ToString("N0") : "Discovered") : "Not found yet", 26f, MenuUI.TextSoft);
            MenuUI.Anchor(detail.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 14f), new Vector2(CellWidth - 20f, 32f));
        }
    }
}
