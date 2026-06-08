using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Castlebound.Gameplay.UI
{
    public sealed class UpgradeMenuTabStrip
    {
        private readonly Func<string, Transform, string, float, int, Button> createButton;
        private readonly Action<UpgradeMenuTab> selectTab;
        private readonly List<TabBinding> tabs = new List<TabBinding>();

        private RectTransform root;

        public UpgradeMenuTabStrip(
            Func<string, Transform, string, float, int, Button> createButton,
            Action<UpgradeMenuTab> selectTab)
        {
            this.createButton = createButton;
            this.selectTab = selectTab;
        }

        public RectTransform Root => root;

        public void Ensure(RectTransform parent)
        {
            if (parent == null || root != null)
            {
                return;
            }

            var tabObject = new GameObject("UpgradeMenuTabs", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            tabObject.transform.SetParent(parent, false);
            root = tabObject.GetComponent<RectTransform>();

            var layout = tabObject.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;

            var fitter = tabObject.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            AddTab("CastleTab", "Castle", UpgradeMenuTab.Castle);
            AddTab("DefenseTab", "Defense", UpgradeMenuTab.Defense);
        }

        public void Refresh(UpgradeMenuTab activeTab, Color normalColor, Color activeColor)
        {
            for (int i = 0; i < tabs.Count; i++)
            {
                var tab = tabs[i];
                if (tab.Button == null)
                {
                    continue;
                }

                var isActive = tab.Tab == activeTab;
                var image = tab.Button.GetComponent<Image>();
                if (image != null)
                {
                    image.color = isActive ? activeColor : normalColor;
                }
            }
        }

        private void AddTab(string name, string label, UpgradeMenuTab tab)
        {
            var button = createButton.Invoke(name, root, label, 140f, 18);
            button.onClick.AddListener(() => selectTab.Invoke(tab));
            tabs.Add(new TabBinding(button, tab));
        }

        private readonly struct TabBinding
        {
            public TabBinding(Button button, UpgradeMenuTab tab)
            {
                Button = button;
                Tab = tab;
            }

            public Button Button { get; }
            public UpgradeMenuTab Tab { get; }
        }
    }
}
