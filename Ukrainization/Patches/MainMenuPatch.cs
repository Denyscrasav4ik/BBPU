using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using MTM101BaldAPI.UI;
using TMPro;
using Ukrainization.API;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Ukrainization.Patches
{
    internal class ArrowPositioner : MonoBehaviour
    {
        private TextMeshProUGUI textComponent = null!;
        private RectTransform arrowRect = null!;

        public void Initialize(TextMeshProUGUI text, RectTransform arrow)
        {
            textComponent = text;
            arrowRect = arrow;
        }

        void LateUpdate()
        {
            if (textComponent != null && arrowRect != null)
            {
                textComponent.ForceMeshUpdate();
                var textInfo = textComponent.textInfo;

                if (textInfo.characterCount > 0)
                {
                    var lastVisibleCharInfo = textInfo.characterInfo[textInfo.characterCount - 1];
                    float textEdgeX = lastVisibleCharInfo.topRight.x;

                    arrowRect.anchorMin = new Vector2(0.5f, 0.5f);
                    arrowRect.anchorMax = new Vector2(0.5f, 0.5f);
                    arrowRect.pivot = new Vector2(0.5f, 0.5f);
                    arrowRect.anchoredPosition = new Vector2(textEdgeX + 5f, 2.4f);
                }
            }
        }
    }

    [HarmonyPatch]
    internal class MainMenuPatch
    {
        private static readonly Dictionary<string, string> LocalizationKeys = new Dictionary<
            string,
            string
        >()
        {
            { "StartTest", "Ukr_Menu_TestMapText" },
            { "StartTest_1", "Ukr_Menu_TestMapText_1" },
            { "Reminder", "Ukr_Menu_Reminder" },
            { "ModInfo", "Ukr_Menu_ModInfo" },
        };

        private static readonly List<SocialMediaInfo> SocialMediaLinks = new List<SocialMediaInfo>()
        {
            new SocialMediaInfo("TelegramButton", "Ukr_Menu_Telegram", ""),
            new SocialMediaInfo("DiscordButton", "Ukr_Menu_Discord", ""),
            new SocialMediaInfo(
                "GameBananaButton",
                "Ukr_Menu_GameBanana",
                "https://gamebanana.com/mods/617076"
            ),
            new SocialMediaInfo(
                "YouTubeButton",
                "Ukr_Menu_YouTube",
                "https://www.youtube.com/@Denyscrasav4ik"
            ),
            new SocialMediaInfo(
                "GithubButton",
                "Ukr_Menu_Github",
                "https://github.com/Denyscrasav4ik/BBPU"
            ),
        };

        private class SocialMediaInfo
        {
            public string ButtonName { get; private set; }
            public string LocalizationKey { get; private set; }
            public string Url { get; private set; }

            public SocialMediaInfo(string buttonName, string localizationKey, string url)
            {
                ButtonName = buttonName;
                LocalizationKey = localizationKey;
                Url = url;
            }
        }

        private static readonly List<KeyValuePair<string, Vector2>> SizeDeltaTargets = new List<
            KeyValuePair<string, Vector2>
        >
        {
            new KeyValuePair<string, Vector2>("StartTest", new Vector2(210f, 32f)),
            new KeyValuePair<string, Vector2>("StartTest_1", new Vector2(228f, 32f)),
        };

        [HarmonyPatch(typeof(GameObject), "SetActive")]
        private static class SetActivePatch
        {
            [HarmonyPostfix]
            private static void Postfix(GameObject __instance, bool value)
            {
                if (__instance.name == "Menu" && value)
                {
                    ApplySizeChanges(__instance.transform);
                    ApplyLocalization(__instance.transform);
                    CreateModInfoButton(__instance.transform);
                }
            }
        }

        private static GameObject? socialLinksPanel = null;
        private static bool dropdownVisible = false;
        private static RectTransform? dropdownArrow = null;

        private static void CreateModInfoButton(Transform rootTransform)
        {
            if (rootTransform == null)
                return;

            Transform? reminderTransform = rootTransform.Find("Reminder");
            if (reminderTransform == null)
                return;

            Transform? existingModInfo = rootTransform.Find("ModInfo");
            if (existingModInfo != null)
                return;

            GameObject modInfo = GameObject.Instantiate(
                reminderTransform.gameObject,
                rootTransform
            );
            modInfo.name = "ModInfo";

            modInfo.transform.localPosition = new Vector3(-180f, 155f, 0f);
            modInfo.transform.SetSiblingIndex(15);

            RectTransform? rectTransform = modInfo.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(150f, 50f);
                rectTransform.offsetMin = new Vector2(-239f, 160f);
            }

            TextMeshProUGUI? textComponent = modInfo.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.raycastTarget = true;

                TextLocalizer? localizer = textComponent.GetComponent<TextLocalizer>();
                if (localizer == null)
                {
                    localizer = textComponent.gameObject.AddComponent<TextLocalizer>();
                }

                GameObject arrowObject = new GameObject("DropdownArrow", typeof(RectTransform));
                arrowObject.transform.SetParent(modInfo.transform, false);

                RectTransform arrowRect = arrowObject.GetComponent<RectTransform>();
                // arrowRect.anchorMin = new Vector2(1, 0.5f);
                // arrowRect.anchorMax = new Vector2(1, 0.5f);
                // arrowRect.pivot = new Vector2(0.5f, 0.5f);
                // arrowRect.anchoredPosition = new Vector2(2, 2.4f);
                arrowRect.sizeDelta = new Vector2(8f, 8f);

                modInfo.AddComponent<ArrowPositioner>().Initialize(textComponent, arrowRect);

                GameObject triangle = new GameObject("Triangle", typeof(RectTransform));
                triangle.transform.SetParent(arrowObject.transform, false);

                RectTransform triangleRect = triangle.GetComponent<RectTransform>();
                triangleRect.anchorMin = Vector2.zero;
                triangleRect.anchorMax = Vector2.one;
                triangleRect.sizeDelta = Vector2.zero;

                TriangleImage triangleUI = triangle.AddComponent<TriangleImage>();
                triangleUI.color = new Color(0.5f, 0.5f, 0.5f, 1f);

                dropdownArrow = arrowRect;

                localizer.key = "Ukr_Menu_ModInfo";
                localizer.RefreshLocalization();

                StandardMenuButton button =
                    textComponent.gameObject.ConvertToButton<StandardMenuButton>(true);
                button.underlineOnHigh = true;

                button.OnPress.AddListener(() =>
                {
                    ToggleSocialLinksDropdown(rootTransform, modInfo);
                });
            }

            CreateSocialLinksPanel(rootTransform, modInfo);
        }

        private static void CreateSocialLinksPanel(
            Transform rootTransform,
            GameObject modInfoButton
        )
        {
            GameObject panel = new GameObject(
                "SocialLinksPanel",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );
            panel.transform.SetParent(rootTransform, false);

            panel.transform.SetSiblingIndex(16);

            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 1);
            panelRect.anchorMax = new Vector2(0, 1);

            panelRect.anchoredPosition = new Vector2(145, -116);

            float buttonHeight = 35f;
            float topBottomPadding = 20f;
            float panelHeight = (SocialMediaLinks.Count * buttonHeight) + topBottomPadding;

            panelRect.sizeDelta = new Vector2(130f, panelHeight);

            Image panelImage = panel.GetComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

            for (int i = 0; i < SocialMediaLinks.Count; i++)
            {
                SocialMediaInfo info = SocialMediaLinks[i];
                CreateSocialButton(panel, info.ButtonName, info.Url, i);
            }

            panel.SetActive(false);
            socialLinksPanel = panel;
        }

        private static void CreateSocialButton(
            GameObject parent,
            string buttonName,
            string url,
            int index
        )
        {
            GameObject buttonObj = new GameObject(
                buttonName,
                typeof(RectTransform),
                typeof(TextMeshProUGUI)
            );
            buttonObj.transform.SetParent(parent.transform, false);

            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0, 1);
            buttonRect.anchorMax = new Vector2(1, 1);
            buttonRect.pivot = new Vector2(0.5f, 1);
            buttonRect.anchoredPosition = new Vector3(0, -10 - (index * 35), 0);
            buttonRect.sizeDelta = new Vector2(0, 30);

            TextMeshProUGUI textComponent = buttonObj.GetComponent<TextMeshProUGUI>();
            textComponent.fontSize = 16;
            textComponent.alignment = TextAlignmentOptions.Center;
            textComponent.raycastTarget = true;

            TextLocalizer localizer = buttonObj.AddComponent<TextLocalizer>();

            string? localizationKey = SocialMediaLinks
                .Find(x => x.ButtonName == buttonName)
                ?.LocalizationKey;
            if (string.IsNullOrEmpty(localizationKey))
            {
                localizationKey = buttonName;
            }

            localizer.key = localizationKey;
            localizer.RefreshLocalization();

            StandardMenuButton button = buttonObj.ConvertToButton<StandardMenuButton>(true);
            button.underlineOnHigh = true;

            button.OnPress.AddListener(() =>
            {
                Application.OpenURL(url);
                ToggleSocialLinksDropdown(null, null);
            });
        }

        private static void ToggleSocialLinksDropdown(
            Transform? rootTransform,
            GameObject? modInfoButton
        )
        {
            if (socialLinksPanel == null || rootTransform == null || modInfoButton == null)
            {
                return;
            }

            dropdownVisible = !dropdownVisible;
            socialLinksPanel.SetActive(dropdownVisible);

            if (dropdownArrow != null)
            {
                dropdownArrow.localRotation = Quaternion.Euler(0, 0, dropdownVisible ? -90 : 0);
            }
        }

        private static void ApplySizeChanges(Transform rootTransform)
        {
            rootTransform.SetSizeDeltas(SizeDeltaTargets);
        }

        private class TriangleImage : UnityEngine.UI.Graphic
        {
            protected override void OnPopulateMesh(VertexHelper vh)
            {
                vh.Clear();

                Vector2 center = rectTransform.rect.center;
                float width = rectTransform.rect.width;
                float height = rectTransform.rect.height;

                UIVertex vert = UIVertex.simpleVert;
                vert.color = color;

                vert.position = new Vector3(center.x + width / 2, center.y, 0);
                vh.AddVert(vert);

                vert.position = new Vector3(center.x - width / 2, center.y + height / 2, 0);
                vh.AddVert(vert);

                vert.position = new Vector3(center.x - width / 2, center.y - height / 2, 0);
                vh.AddVert(vert);

                vh.AddTriangle(0, 1, 2);
            }
        }

        private static void ApplyLocalization(Transform rootTransform)
        {
            rootTransform.ApplyLocalizations(LocalizationKeys);
        }
    }
}
