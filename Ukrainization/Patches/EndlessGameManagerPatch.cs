using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace Ukrainization.Patches
{
    [HarmonyPatch(typeof(EndlessGameManager))]
    internal class EndlessGameManagerPatch
    {
        private static bool initialized = false;

        private static readonly Dictionary<string, string> localizationKeys = new Dictionary<
            string,
            string
        >()
        {
            { "EndlessGameManager(Clone)/Score/Text", "Ukr_Endless_Text" },
            { "EndlessGameManager(Clone)/Score/Congrats", "Ukr_Endless_Score" },
            { "Medium_EndlessGameManager(Clone)/Score/Text", "Ukr_Endless_Text" },
            { "Medium_EndlessGameManager(Clone)/Score/Congrats", "Ukr_Endless_Score" },
        };

        private static readonly string[] rankPaths = new string[]
        {
            "EndlessGameManager(Clone)/Score/Rank",
            "Medium_EndlessGameManager(Clone)/Score/Rank",
        };

        private static readonly string[] scorePaths = new string[]
        {
            "EndlessGameManager(Clone)/Score/Score",
            "Medium_EndlessGameManager(Clone)/Score/Score",
        };

        [HarmonyPatch("RestartLevel")]
        [HarmonyPostfix]
        private static void RestartLevelPostfix(EndlessGameManager __instance)
        {
            if (!initialized)
            {
                __instance.StartCoroutine(InitEndlessScreenComponents());
                initialized = true;
            }
        }

        private static IEnumerator InitEndlessScreenComponents()
        {
            for (int i = 0; i < 3; i++)
                yield return null;

            PrepareLocalizationComponents();
            AdjustRankPositions();
            AdjustScorePositions();
        }

        private static void AdjustRankPositions()
        {
            foreach (string rankPath in rankPaths)
            {
                GameObject rankObject = GameObject.Find(rankPath);
                if (rankObject != null)
                {
                    RectTransform rectTransform = rankObject.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        rectTransform.anchoredPosition = new Vector2(-21f, -14f);
                    }
                }
            }
        }

        private static void AdjustScorePositions()
        {
            foreach (string scorePath in scorePaths)
            {
                GameObject scoreObject = GameObject.Find(scorePath);
                if (scoreObject != null)
                {
                    RectTransform rectTransform = scoreObject.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        Vector2 pos = rectTransform.anchoredPosition;
                        rectTransform.anchoredPosition = new Vector2(pos.x, pos.y - 20f);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CoreGameManager), "ReturnToMenu")]
        [HarmonyPrefix]
        private static void ResetInitialization()
        {
            initialized = false;
        }

        private static void PrepareLocalizationComponents()
        {
            foreach (KeyValuePair<string, string> pair in localizationKeys)
            {
                GameObject menuItem = GameObject.Find(pair.Key);
                if (menuItem != null)
                {
                    TextMeshProUGUI textComponent = menuItem.GetComponent<TextMeshProUGUI>();
                    if (textComponent != null)
                    {
                        string originalText = textComponent.text;

                        Component[] components = menuItem.GetComponents<Component>();
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
                        TextLocalizer localizer = menuItem.GetComponent<TextLocalizer>();
                        if (localizer == null)
                        {
                            localizer = menuItem.AddComponent<TextLocalizer>();
                            localizer.key = pair.Value;

                            localizer.RefreshLocalization();
                        }
                        else
                        {
                            localizer.key = pair.Value;
                            localizer.RefreshLocalization();
                        }
                    }
                }
            }
        }
    }
}
