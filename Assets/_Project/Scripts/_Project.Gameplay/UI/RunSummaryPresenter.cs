using Castlebound.Gameplay.Stats;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Castlebound.Gameplay.UI
{
    public sealed class RunSummaryPresenter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI summaryText;
        [SerializeField] private Color panelColor = new Color(0.16f, 0.18f, 0.20f, 0.96f);
        [SerializeField] private Color textColor = new Color(0.86f, 0.88f, 0.90f, 1f);

        private RectTransform panelRect;

        public void Present(RunStats stats)
        {
            EnsureText();
            if (summaryText == null)
                return;

            summaryText.text = RunSummaryFormatter.Format(stats);
            summaryText.color = textColor;
            summaryText.alignment = TextAlignmentOptions.Center;
            summaryText.enableAutoSizing = true;
            summaryText.fontSizeMin = 18f;
            summaryText.fontSizeMax = 36f;
            summaryText.enableWordWrapping = false;

            var rect = summaryText.rectTransform;
            rect.anchorMin = new Vector2(0.06f, 0.06f);
            rect.anchorMax = new Vector2(0.94f, 0.94f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private void EnsureText()
        {
            if (panelRect == null)
                CreatePanel();

            if (summaryText == null || summaryText.transform == transform)
                CreateSummaryText();
        }

        private void CreatePanel()
        {
            if (transform is RectTransform rootRect)
            {
                rootRect.anchorMin = new Vector2(0.04f, 0.04f);
                rootRect.anchorMax = new Vector2(0.96f, 0.96f);
                rootRect.offsetMin = Vector2.zero;
                rootRect.offsetMax = Vector2.zero;
            }

            var panelObject = new GameObject("RunSummaryPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panelObject.transform.SetParent(transform, false);
            panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.12f, 0.28f);
            panelRect.anchorMax = new Vector2(0.88f, 0.88f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            panelObject.GetComponent<Image>().color = panelColor;
        }

        private void CreateSummaryText()
        {
            var authoredText = summaryText != null ? summaryText : GetComponent<TextMeshProUGUI>();
            if (authoredText != null)
                authoredText.enabled = false;

            var textObject = new GameObject("RunSummaryText", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(panelRect, false);
            summaryText = textObject.GetComponent<TextMeshProUGUI>();
            if (authoredText != null)
                summaryText.font = authoredText.font;
        }
    }
}
