using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using TMPro;
using UnityEngine;

// using Ukrainization.Runtime;

namespace Ukrainization.Patches
{
    internal class PlaceholderWinManagerPatch
    {
        private static bool initialized = false;

        [HarmonyPatch(typeof(PlaceholderWinManager), "Initialize")]
        private static class InitializePatch
        {
            [HarmonyPostfix]
            private static void Postfix(PlaceholderWinManager __instance)
            {
                if (!initialized)
                {
                    ApplyLocalizationToErrorScreen(__instance);
                    initialized = true;
                }
            }
        }

        private static void ApplyLocalizationToErrorScreen(PlaceholderWinManager manager)
        {
            FieldInfo endingErrorField = typeof(PlaceholderWinManager).GetField(
                "endingError",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            if (endingErrorField == null)
            {
                return;
            }

            GameObject? endingError = endingErrorField.GetValue(manager) as GameObject;
            if (endingError == null)
            {
                return;
            }

            Transform? textTransform = endingError.transform.Find("Text (TMP)");
            if (textTransform != null)
            {
                TextMeshProUGUI? textComponent = textTransform.GetComponent<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    TextLocalizer? localizer =
                        textTransform.gameObject.GetComponent<TextLocalizer>();
                    if (localizer == null)
                    {
                        localizer = textTransform.gameObject.AddComponent<TextLocalizer>();
                        localizer.key = "Ukr_EndingError_Text";
                        localizer.RefreshLocalization();
                    }
                    else
                    {
                        localizer.key = "Ukr_EndingError_Text";
                        localizer.RefreshLocalization();
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CoreGameManager), "Quit")]
        private static class QuitPatch
        {
            [HarmonyPrefix]
            private static void Prefix()
            {
                initialized = false;
            }
        }
    }
}
