using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace Ukrainization.Patches
{
    internal static class WarningScreenContainer
    {
        internal static (string, bool)[] screens =>
            nonCriticalScreens
                .Select(x => (x, false))
                .ToArray()
                .AddRangeToArray(criticalScreens.Select(x => (x, true)).ToArray())
                .ToArray();

        internal static List<string> nonCriticalScreens = new List<string>();

        internal static List<string> criticalScreens = new List<string>();

        internal static int currentPage = 0;

        internal static string pressAny = "";
    }

    [HarmonyPatch(typeof(WarningScreen))]
    [HarmonyPatch("Start")]
    [HarmonyPriority(800)]
    static class WarningScreenStartPatch
    {
        static bool Prefix(WarningScreen __instance)
        {
            string text = "";
            if (Singleton<InputManager>.Instance.SteamInputActive)
            {
                text = string.Format(
                    Singleton<LocalizationManager>.Instance.GetLocalizedText("Men_Warning"),
                    Singleton<InputManager>.Instance.GetInputButtonName(
                        "MouseSubmit",
                        "Interface",
                        false
                    )
                );
                WarningScreenContainer.pressAny = string.Format(
                    "НАТИСНІТЬ {0} ЩОБ ПРОДОВЖИТИ",
                    Singleton<InputManager>.Instance.GetInputButtonName(
                        "MouseSubmit",
                        "Interface",
                        false
                    )
                );
            }
            else
            {
                text = string.Format(
                    Singleton<LocalizationManager>.Instance.GetLocalizedText("Men_Warning"),
                    "БУДЬ-ЯКУ КЛАВІШУ"
                );
                WarningScreenContainer.pressAny = string.Format(
                    "НАТИСНІТЬ {0} ЩОБ ПРОДОВЖИТИ",
                    "БУДЬ-ЯКУ КЛАВІШУ"
                );
            }
            WarningScreenContainer.nonCriticalScreens.Insert(0, text);
            __instance.textBox.text = text;
            return false;
        }
    }

    [HarmonyPatch(typeof(WarningScreen))]
    [HarmonyPatch("Advance")]
    [HarmonyPriority(800)]
    static class WarningScreenAdvancePatch
    {
        static bool Prefix(WarningScreen __instance)
        {
            if (WarningScreenContainer.currentPage >= WarningScreenContainer.screens.Length)
            {
                return true;
            }
            if (
                (WarningScreenContainer.screens[WarningScreenContainer.currentPage].Item2)
                && (
                    (WarningScreenContainer.currentPage + 1)
                    >= WarningScreenContainer.screens.Length
                )
            )
            {
                return false;
            }
            WarningScreenContainer.currentPage++;
            if (WarningScreenContainer.currentPage >= WarningScreenContainer.screens.Length)
            {
                return true;
            }
            Singleton<GlobalCam>.Instance.Transition(UiTransition.Dither, 0.01666667f);
            if (!WarningScreenContainer.screens[WarningScreenContainer.currentPage].Item2)
            {
                __instance.textBox.text =
                    "<color=yellow>УВАГА!</color>\n"
                    + WarningScreenContainer.screens[WarningScreenContainer.currentPage].Item1
                    + "\n\n"
                    + WarningScreenContainer.pressAny;
            }
            else
            {
                if (
                    (
                        (WarningScreenContainer.currentPage + 1)
                        < WarningScreenContainer.screens.Length
                    )
                )
                {
                    __instance.textBox.text =
                        "<color=red>ПОМИЛКА!</color>\n"
                        + WarningScreenContainer.screens[WarningScreenContainer.currentPage].Item1
                        + "\n\n"
                        + WarningScreenContainer.pressAny;
                }
                else
                {
                    __instance.textBox.text =
                        "<color=red>ПОМИЛКА!</color>\n"
                        + WarningScreenContainer.screens[WarningScreenContainer.currentPage].Item1
                        + "\n\nНАТИСНІТЬ ALT+F4 ЩОБ ВИЙТИ";
                }
            }
            return false;
        }
    }

    // Цей Postfix-патч спрацює *після* того, як MTM101BMDE підготує екран попередження.
    // Ми просто заміняємо англійський текст на український.
    [HarmonyPatch(typeof(WarningScreen))]
    internal class WarningScreen_Patch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void Start_Postfix(WarningScreen __instance)
        {
            TranslateWarningText(__instance.textBox);
        }

        [HarmonyPatch("Advance")]
        [HarmonyPostfix]
        private static void Advance_Postfix(WarningScreen __instance)
        {
            TranslateWarningText(__instance.textBox);
        }

        private static void TranslateWarningText(TMP_Text textBox)
        {
            if (textBox == null)
                return;

            string currentText = textBox.text;

            // Переклад заголовків
            currentText = currentText.Replace("WARNING!", "УВАГА!");
            currentText = currentText.Replace("ERROR!", "ПОМИЛКА!");

            // Переклад тексту попередження (якщо він є)
            if (
                currentText.Contains(
                    "This game is not suitable for children or those who are easily disturbed."
                )
            )
            {
                currentText =
                    "Ця гра не підходить для дітей, чи людей за слабкою психікою.\nВона включає гучні звуки, та лякаючі образи.";
            }

            // Переклад інструкції "Press any button to continue"
            if (currentText.Contains("PRESS ANY BUTTON TO CONTINUE"))
            {
                currentText = currentText.Replace(
                    "PRESS ANY BUTTON TO CONTINUE",
                    "НАТИСНІТЬ БУДЬ-ЯКУ КЛАВІШУ ЩОБ ПРОДОВЖИТИ"
                );
            }
            else
            {
                // Для випадків, коли вказана конкретна кнопка
                currentText = System.Text.RegularExpressions.Regex.Replace(
                    currentText,
                    @"PRESS (.+) TO CONTINUE",
                    "НАТИСНІТЬ $1 ЩОБ ПРОДОВЖИТИ"
                );
            }

            // Переклад інструкції виходу
            currentText = currentText.Replace("PRESS ALT+F4 TO EXIT", "НАТИСНІТЬ ALT+F4 ЩОБ ВИЙТИ");

            textBox.text = currentText;
        }
    }
}
