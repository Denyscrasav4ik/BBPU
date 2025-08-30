using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using TMPro;
using Ukrainization.API;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Ukrainization
{
    internal class PickChallengeMenuPatch
    {
        private static bool fixesApplied = false;

        private static readonly List<KeyValuePair<string, Vector2>> SizeDeltaTargets = new List<
            KeyValuePair<string, Vector2>
        >
        {
            new KeyValuePair<string, Vector2>("Stealthy", new Vector2(175f, 32f)),
        };

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

                if (__instance.name == "PickChallenge" && value && !fixesApplied)
                {
                    ApplyButtonSizeFixes(__instance.transform);
                    fixesApplied = true;
                }
            }
        }

        private static void ApplyButtonSizeFixes(Transform pickChallengeTransform)
        {
            pickChallengeTransform.SetSizeDeltas(SizeDeltaTargets);
        }
    }
}
