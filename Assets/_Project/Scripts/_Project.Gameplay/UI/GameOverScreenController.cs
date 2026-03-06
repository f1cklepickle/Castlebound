using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Castlebound.Gameplay.UI
{
    public class GameOverScreenController : MonoBehaviour
    {
        [SerializeField] private Button restartButton;
        [SerializeField] private Sprite restartButtonSprite;
        [SerializeField] private string restartButtonText = "Rise Again";
        [SerializeField] private Color restartButtonTextColor = new Color(0.84f, 0.95f, 1f, 1f);

        private UnityAction restartHandler;

        private void OnEnable()
        {
            EnsureRestartButton();
            ApplyVisuals();
            BindRestartHandler();
        }

        private void OnDisable()
        {
            if (restartButton != null && restartHandler != null)
            {
                restartButton.onClick.RemoveListener(restartHandler);
            }
        }

        public void SetRestartHandler(UnityAction handler)
        {
            if (restartButton != null && restartHandler != null)
            {
                restartButton.onClick.RemoveListener(restartHandler);
            }

            restartHandler = handler;
            EnsureRestartButton();
            ApplyVisuals();
            BindRestartHandler();
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        private void BindRestartHandler()
        {
            if (restartButton == null || restartHandler == null)
            {
                return;
            }

            restartButton.onClick.RemoveListener(restartHandler);
            restartButton.onClick.AddListener(restartHandler);
        }

        private void EnsureRestartButton()
        {
            if (restartButton == null)
            {
                restartButton = GetComponentInChildren<Button>(true);
            }

            if (restartButton == null)
            {
                restartButton = CreateDefaultRestartButton(transform);
            }
        }

        private static Button CreateDefaultRestartButton(Transform parent)
        {
            var buttonObject = new GameObject("RestartButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.2f);
            rect.anchorMax = new Vector2(0.5f, 0.2f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(300f, 92f);

            return buttonObject.GetComponent<Button>();
        }

        private void ApplyVisuals()
        {
            if (restartButton == null)
            {
                return;
            }

            var image = restartButton.GetComponent<Image>();
            if (image != null && restartButtonSprite != null)
            {
                image.sprite = restartButtonSprite;
                image.type = Image.Type.Simple;
                image.preserveAspect = true;
            }

            var label = restartButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label == null)
            {
                var labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                labelObject.transform.SetParent(restartButton.transform, false);
                label = labelObject.GetComponent<TextMeshProUGUI>();
            }

            var labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 30f;
            label.text = string.IsNullOrWhiteSpace(restartButtonText) ? "Rise Again" : restartButtonText;
            label.color = restartButtonTextColor;
        }
    }
}
