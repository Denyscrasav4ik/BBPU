using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace Ukrainization.Patches
{
    [HarmonyPatch(typeof(Rewired.UI.ControlMapper.ControlMapper), "Initialize")]
    internal class ControlMapperPatch
    {
        [HarmonyPostfix]
        private static void OnInitializePostfix(Rewired.UI.ControlMapper.ControlMapper __instance)
        {
            if (__instance == null)
                return;

            ApplyStaticCategoryButtonLocalization(__instance);
            ApplyDynamicActionGridLocalization(__instance);
        }

        private static void ApplyStaticCategoryButtonLocalization(
            Rewired.UI.ControlMapper.ControlMapper controlMapper
        )
        {
            ApplyLocalization(
                controlMapper,
                "Canvas/MainPageGroup/MainContent/MainContentInner/SettingAndMapCategoriesGroup/MapCategoriesGroup/ButtonLayoutGroup/DefaultButton/Text",
                "Ukr_Ctrl_GameplayButton"
            );

            ApplyLocalization(
                controlMapper,
                "Canvas/MainPageGroup/MainContent/MainContentInner/SettingAndMapCategoriesGroup/MapCategoriesGroup/ButtonLayoutGroup/MenuButton/Text",
                "Ukr_Ctrl_MenuButton"
            );
        }

        private static void ApplyLocalization(
            Rewired.UI.ControlMapper.ControlMapper controlMapper,
            string path,
            string key
        )
        {
            Transform target = controlMapper.transform.Find(path);
            if (target == null)
                return;

            TextMeshProUGUI text = target.GetComponent<TextMeshProUGUI>();
            if (text == null)
                return;

            var localizer =
                text.gameObject.GetComponent<TextLocalizer>()
                ?? text.gameObject.AddComponent<TextLocalizer>();

            localizer.key = key;
            localizer.RefreshLocalization();
        }

        #region Dynamic Title & Content Localization for Window

        [HarmonyPatch(typeof(Rewired.UI.ControlMapper.Window), "Enable")]
        private class WindowPatch
        {
            private static readonly Dictionary<string, string> ActionLocalizationMap =
                new Dictionary<string, string>
                {
                    { "Restore Defaults", "Ukr_Ctrl_RestoreDefaults" },
                    { "Horizontal movement", "Ukr_Ctrl_Action_1" },
                    { "Strafe right", "Ukr_Ctrl_Action_2" },
                    { "Strafe left", "Ukr_Ctrl_Action_3" },
                    { "Vertical movement", "Ukr_Ctrl_Action_4" },
                    { "Move forward", "Ukr_Ctrl_Action_5" },
                    { "Move backward", "Ukr_Ctrl_Action_6" },
                    { "Turn (Joystick)", "Ukr_Ctrl_Action_7" },
                    { "Turn right", "Ukr_Ctrl_Action_8" },
                    { "Turn left", "Ukr_Ctrl_Action_9" },
                    { "Turn (Mouse)", "Ukr_Ctrl_Action_10" },
                    { "Interact", "Ukr_Ctrl_Action_14" },
                    { "Use item", "Ukr_Ctrl_Action_15" },
                    { "Run", "Ukr_Ctrl_Action_16" },
                    { "Look back", "Ukr_Ctrl_Action_17" },
                    { "Quick map", "Ukr_Ctrl_Action_18" },
                    { "Advanced map", "Ukr_Ctrl_Action_19" },
                    { "Item select left", "Ukr_Ctrl_Action_20" },
                    { "Item select right", "Ukr_Ctrl_Action_21" },
                    { "Item select axis", "Ukr_Ctrl_Action_22" },
                    { "ItemSelect +", "Ukr_Ctrl_Action_23" },
                    { "ItemSelect -", "Ukr_Ctrl_Action_24" },
                    { "Item 1", "Ukr_Ctrl_Action_25" },
                    { "Item 2", "Ukr_Ctrl_Action_26" },
                    { "Item 3", "Ukr_Ctrl_Action_27" },
                    { "Item 4", "Ukr_Ctrl_Action_28" },
                    { "Item 5", "Ukr_Ctrl_Action_29" },
                    { "Item 6", "Ukr_Ctrl_Action_30" },
                    { "Item 7", "Ukr_Ctrl_Action_31" },
                    { "Item 8", "Ukr_Ctrl_Action_32" },
                    { "Item 9", "Ukr_Ctrl_Action_33" },
                    { "Pause", "Ukr_Ctrl_Action_34" },
                    { "Cursor Horizontal (Joystick)", "Ukr_Ctrl_Action_36" },
                    { "Cursor Horizontal (Joystick) +", "Ukr_Ctrl_Action_37" },
                    { "Cursor Horizontal (Joystick) -", "Ukr_Ctrl_Action_38" },
                    { "Cursor Vertical (Joystick)", "Ukr_Ctrl_Action_39" },
                    { "Cursor Vertical (Joystick) +", "Ukr_Ctrl_Action_40" },
                    { "Cursor Vertical (Joystick) -", "Ukr_Ctrl_Action_41" },
                    { "Cursor Horizontal (Mouse)", "Ukr_Ctrl_Action_42" },
                    { "Cursor Horizontal (Mouse) +", "Ukr_Ctrl_Action_43" },
                    { "Cursor Horizontal (Mouse) -", "Ukr_Ctrl_Action_44" },
                    { "Cursor Vertical (Mouse)", "Ukr_Ctrl_Action_45" },
                    { "Cursor Vertical (Mouse) +", "Ukr_Ctrl_Action_46" },
                    { "Cursor Vertical (Mouse) -", "Ukr_Ctrl_Action_47" },
                    { "Click", "Ukr_Ctrl_Action_48" },
                    { "Zoom Map In", "Ukr_Ctrl_Action_49" },
                    { "Zoom Map Out", "Ukr_Ctrl_Action_50" },
                    { "Cursor Speed Boost", "Ukr_Ctrl_Action_51" },
                    { "Map Horizontal (Joystick)", "Ukr_Ctrl_Action_52" },
                    { "Map Horizontal (Joystick) +", "Ukr_Ctrl_Action_53" },
                    { "Map Horizontal (Joystick) -", "Ukr_Ctrl_Action_54" },
                    { "Map Vertical (Joystick)", "Ukr_Ctrl_Action_55" },
                    { "Map Vertical (Joystick) +", "Ukr_Ctrl_Action_56" },
                    { "Map Vertical (Joystick) -", "Ukr_Ctrl_Action_57" },
                    { "Map Horizontal (Mouse)", "Ukr_Ctrl_Action_58" },
                    { "Map Horizontal (Mouse) +", "Ukr_Ctrl_Action_59" },
                    { "Map Horizontal (Mouse) -", "Ukr_Ctrl_Action_60" },
                    { "Map Vertical (Mouse)", "Ukr_Ctrl_Action_61" },
                    { "Map Vertical (Mouse) +", "Ukr_Ctrl_Action_62" },
                    { "Map Vertical (Mouse) -", "Ukr_Ctrl_Action_63" },
                };

            [HarmonyPostfix]
            private static void Postfix(Rewired.UI.ControlMapper.Window __instance)
            {
                ApplyDynamicTitleLocalization(__instance);
                ApplyDynamicContentTextLocalization(__instance);
            }

            private static void ApplyDynamicTitleLocalization(
                Rewired.UI.ControlMapper.Window window
            )
            {
                TextMeshProUGUI titleText = FindText(window, "Title Text");
                if (titleText == null)
                    return;

                foreach (var pair in ActionLocalizationMap.OrderByDescending(p => p.Key.Length))
                {
                    if (titleText.text.Contains(pair.Key))
                    {
                        string localized = Singleton<LocalizationManager>.Instance.GetLocalizedText(
                            pair.Value
                        );
                        titleText.text = titleText.text.Replace(pair.Key, localized);
                        break;
                    }
                }
            }

            private static void ApplyDynamicContentTextLocalization(
                Rewired.UI.ControlMapper.Window window
            )
            {
                Transform content = window.transform.Find("Content");
                if (content == null)
                    return;

                foreach (Transform child in content)
                {
                    if (!child.name.Contains("Content Text"))
                        continue;

                    TextMeshProUGUI contentText = child.GetComponent<TextMeshProUGUI>();
                    if (contentText == null)
                        continue;

                    string text = contentText.text;
                    string modified = text;

                    foreach (var pair in ActionLocalizationMap.OrderByDescending(p => p.Key.Length))
                    {
                        if (modified.Contains(pair.Key))
                        {
                            string localized =
                                Singleton<LocalizationManager>.Instance.GetLocalizedText(
                                    pair.Value
                                );
                            modified = modified.Replace(pair.Key, localized);
                        }
                    }

                    if (modified != text)
                    {
                        contentText.text = modified;

                        var localizer =
                            contentText.gameObject.GetComponent<TextLocalizer>()
                            ?? contentText.gameObject.AddComponent<TextLocalizer>();
                        localizer.key = "";
                        localizer.RefreshLocalization();
                    }
                }
            }

            private static TextMeshProUGUI FindText(
                Rewired.UI.ControlMapper.Window window,
                string name
            )
            {
                foreach (var t in window.transform.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == name)
                    {
                        var text = t.GetComponent<TextMeshProUGUI>();
                        if (text != null)
                            return text;
                    }
                }
                return null;
            }
        }

        #endregion

        private static void ApplyDynamicActionGridLocalization(
            Rewired.UI.ControlMapper.ControlMapper controlMapper
        )
        {
            Transform actionColumn = controlMapper.transform.Find(
                "Canvas/MainPageGroup/MainContent/MainContentInner/InputGridGroup/InputGridContainer/Container/ScrollRect/InputGridInnerGroup/ActionLabelColumn"
            );

            if (actionColumn == null)
                return;

            int index = -1;
            foreach (Transform child in actionColumn)
            {
                if (!child.name.Contains("InputGridLabel(Clone)"))
                    continue;

                TextMeshProUGUI text = child.GetComponent<TextMeshProUGUI>();
                if (text == null)
                    continue;

                var localizer =
                    text.gameObject.GetComponent<TextLocalizer>()
                    ?? text.gameObject.AddComponent<TextLocalizer>();

                localizer.key = $"Ukr_Ctrl_Action_{index + 1}";
                localizer.RefreshLocalization();
                index++;
            }
        }
    }
}
