#pragma warning disable 0067

using UnityEngine;
using Watermelon.IAPStore;
using Watermelon.SkinStore;

namespace Watermelon.BusStop
{
    public class RaycastController : MonoBehaviour
    {
        private UIIAPStore iapStorePage;
        private UIMainMenu mainMenuPage;
        private UISkinStore storePage;

        private static bool isActive;

        public static event SimpleCallback OnInputActivated;

        /// <summary>True while the board takes taps (off when paused, won or lost).</summary>
        public static bool IsEnabled => isActive;
        public static event SimpleCallback OnMovementInputActivated;

        public void Initialise()
        {
            isActive = true;

            iapStorePage = UIController.GetPage<UIIAPStore>();
            mainMenuPage = UIController.GetPage<UIMainMenu>();
            storePage = UIController.GetPage<UISkinStore>();
        }

        private void Update()
        {
            if (!isActive) return;

            if (Input.GetMouseButtonDown(0) && !IsRaycastBlockedByUI())
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    IClickableObject clickableObject = hit.transform.GetComponent<IClickableObject>();
                    if (clickableObject != null)
                    {
                        if (!GameController.IsGameActive)
                        {
                            if(LivesManager.Lives > 0)
                            {
                                GameController.StartGame();
                                clickableObject.OnObjectClicked();
                            } else 
                            {
                                GameController.StartGame();   // no lives: OUT OF LIVES
                            }

                            
                        } else
                        {
                            clickableObject.OnObjectClicked();
                        }
                    }
                }
            }
        }

        private bool IsRaycastBlockedByUI()
        {
            // the game's own overlays (Shop and other menu panels, power-up offer, Settings) too
            if (MenuPanel.AnyOpen || PowerUpOfferPopup.IsOpen || OutOfLivesPopup.IsOpen || ConveyorSettingsPanel.IsOpen || ProgressPopup.IsOpen)
                return true;

            return mainMenuPage.NoAdsPopUp.IsOpened || iapStorePage.IsPageDisplayed || storePage.IsPageDisplayed|| AddLivesPanel.IsPanelOpened;
        }

        public void ResetControl()
        {

        }

        public static void Enable()
        {
            isActive = true;
            OnInputActivated?.Invoke();
        }

        public static void Disable()
        {
            isActive = false;
        }
    }
}
