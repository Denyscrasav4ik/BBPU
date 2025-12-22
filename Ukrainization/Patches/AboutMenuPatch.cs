using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Ukrainization
{
    internal class AboutMenuPatch
    {
        private static bool fixesApplied = false;

        private static readonly Dictionary<string, string> LocalizationKeys = new Dictionary<
            string,
            string
        >()
        {
            { "DevUpdateTitle", "Ukr_About_DevUpdateTitle" },
            { "DevUpdateText", "Ukr_About_DevUpdateText" },
            { "Credits", "Ukr_About_CreditsText" },
            { "WebsiteButton", "Ukr_About_WebsiteButtonText" },
            { "DevlogsButton", "Ukr_About_DevlogsButtonText" },
            { "BugsButton", "Ukr_About_BugsButtonText" },
            { "SaveFolderButton", "Ukr_About_SaveFolderButtonText" },
            { "AnniversaryButton", "Ukr_About_AnniversaryButtonText" },
            { "RoadmapButton", "Ukr_About_RoadmapButtonText" },
        };

        private static readonly List<KeyValuePair<string, Vector2>> SizeDeltaTargets = new List<
            KeyValuePair<string, Vector2>
        >
        {
            new KeyValuePair<string, Vector2>("SaveFolderButton", new Vector2(150f, 50f)),
        };

        private static Dictionary<string, Transform> BuildTransformPathMap(Transform parent)
        {
            var map = new Dictionary<string, Transform>();
            var children = parent.GetComponentsInChildren<Transform>(true);

            foreach (var child in children)
            {
                if (child == parent)
                    continue;

                StringBuilder pathBuilder = new StringBuilder();
                Transform current = child;
                while (current != null && current != parent)
                {
                    if (pathBuilder.Length > 0)
                        pathBuilder.Insert(0, "/");
                    pathBuilder.Insert(0, current.name);
                    current = current.parent;
                }

                if (current == parent)
                {
                    string path = pathBuilder.ToString();
                    if (!map.ContainsKey(path))
                    {
                        map.Add(path, child);
                    }
                }
            }
            return map;
        }

        [HarmonyPatch(typeof(MenuButton), "Press")]
        private static class MenuButtonPressPatch
        {
            [HarmonyPostfix]
            private static void Postfix(MenuButton __instance)
            {
                if (__instance != null && __instance.name == "About")
                {
                    fixesApplied = false;
                }
            }
        }

        [HarmonyPatch(typeof(GameObject), "SetActive")]
        private static class SetActivePatch
        {
            [HarmonyPostfix]
            private static void Postfix(GameObject __instance, bool value)
            {
                if (__instance.name == "Menu" && value)
                {
                    fixesApplied = false;
                }

                if (__instance.name == "About" && value && !fixesApplied)
                {
                    var transformMap = BuildTransformPathMap(__instance.transform);
                    ApplyLocalization(transformMap);
                    ApplySizeDeltaChanges(transformMap);
                    fixesApplied = true;

                    ForceRefreshLocalization(transformMap);
                }
            }
        }

        private static void ForceRefreshLocalization(Dictionary<string, Transform> transformMap)
        {
            foreach (var entry in LocalizationKeys)
            {
                if (transformMap.TryGetValue(entry.Key, out Transform targetTransform))
                {
                    TextLocalizer localizer = targetTransform.GetComponent<TextLocalizer>();
                    if (localizer != null)
                    {
                        localizer.RefreshLocalization();
                    }
                }
                else { }
            }
        }

        private static void ApplySizeDeltaChanges(Dictionary<string, Transform> transformMap)
        {
            foreach (var target in SizeDeltaTargets)
            {
                if (transformMap.TryGetValue(target.Key, out Transform elementTransform))
                {
                    RectTransform rectTransform = elementTransform.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        rectTransform.sizeDelta = target.Value;
                    }
                    else { }
                }
                else { }
            }
        }

        private static void ApplyLocalization(Dictionary<string, Transform> transformMap)
        {
            foreach (var entry in LocalizationKeys)
            {
                if (transformMap.TryGetValue(entry.Key, out Transform targetTransform))
                {
                    TextMeshProUGUI textComponent = targetTransform.GetComponent<TextMeshProUGUI>();
                    if (textComponent != null)
                    {
                        Component[] components = targetTransform.GetComponents<Component>();
                        foreach (Component component in components)
                        {
                            if (
                                component != null
                                && component.GetType().Name == "TextLocalizer"
                                && component.GetType() != typeof(TextLocalizer)
                            )
                            {
                                Object.Destroy(component);
                            }
                        }

                        TextLocalizer localizer = textComponent.GetComponent<TextLocalizer>();
                        if (localizer == null)
                        {
                            localizer = textComponent.gameObject.AddComponent<TextLocalizer>();
                            localizer.key = entry.Value;
                        }
                        else if (localizer.key != entry.Value)
                        {
                            localizer.key = entry.Value;
                            localizer.RefreshLocalization();
                        }
                    }
                    else { }
                }
                else { }
            }
        }
    }
}
