using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class UIGameOver : UIPage
    {
        [SerializeField] UIScaleAnimation levelFailedScalable;
        [SerializeField] UIScaleAnimation heartScalable;

        [SerializeField] UIFadeAnimation backgroundFade;

        [SerializeField] UIScaleAnimation replayButtonScalable;
        [SerializeField] Button replayButton;

        private TweenCase replayPingPongCase;

        public override void Initialise()
        {
            replayButton.onClick.AddListener(ReplayButton);
            PlaceLifeBadge();
        }

        // The "-1" life badge keeps the offset it had on the old, bigger heart, which puts it
        // under the RESTART button. Pin it to the heart's lower-right corner instead.
        private void PlaceLifeBadge()
        {
            RectTransform heart = heartScalable != null ? heartScalable.RectTransform : null;
            if (heart == null)
                return;
            RectTransform badge = null;
            foreach (RectTransform child in heart.GetComponentsInChildren<RectTransform>(true))
                if (child.name == "Amount Back Image")
                    badge = child;
            if (badge == null)
                return;
            if (badge.parent != heart)
                heart = (RectTransform)badge.parent;

            Vector2 size = heart.rect.size;
            float side = Mathf.Max(60f, Mathf.Min(size.x, size.y) * 0.36f);
            badge.anchorMin = badge.anchorMax = new Vector2(0.5f, 0.5f);
            badge.pivot = new Vector2(0.5f, 0.5f);
            badge.sizeDelta = new Vector2(side, side);
            badge.anchoredPosition = new Vector2(size.x * 0.33f, -size.y * 0.27f);
            badge.SetAsLastSibling();
        }

        #region Show/Hide

        public override void PlayShowAnimation()
        {
            levelFailedScalable.Hide(immediately: true);
            heartScalable.Hide(immediately: true);
            replayButtonScalable.Hide(immediately: true);

            float fadeDuration = 0.3f;
            backgroundFade.Show(fadeDuration);

            Tween.DelayedCall(fadeDuration * 0.8f, delegate { 
            
                levelFailedScalable.Show(scaleMultiplier: 1.1f);
                heartScalable.Show( scaleMultiplier: 1.1f);
                
                replayButtonScalable.Show(scaleMultiplier: 1.05f);

                replayPingPongCase = replayButtonScalable.RectTransform.DOPingPongScale(1.0f, 1.05f, 0.9f, Ease.Type.QuadIn, Ease.Type.QuadOut, unscaledTime: true);

                UIController.OnPageOpened(this);
            });
        }

        public override void PlayHideAnimation()
        {
            backgroundFade.Hide(0.3f);

            Tween.DelayedCall(0.3f, delegate {

                if (replayPingPongCase != null && replayPingPongCase.IsActive) replayPingPongCase.Kill();

                UIController.OnPageClosed(this);
            });
        }

        #endregion

        #region Buttons 

        public void ReplayButton()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            UIController.HidePage<UIGameOver>();

            if (LivesManager.IsMaxLives)
                LivesManager.RemoveLife();

            GameController.ReplayLevel();
        }

        #endregion
    }
}