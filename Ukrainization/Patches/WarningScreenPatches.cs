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
                    "�����Ͳ�� {0} ��� ����������",
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
                    "����-��� ���²��"
                );
                WarningScreenContainer.pressAny = string.Format(
                    "�����Ͳ�� {0} ��� ����������",
                    "����-��� ���²��"
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
                    "<color=yellow>�����!</color>\n"
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
                        "<color=red>�������!</color>\n"
                        + WarningScreenContainer.screens[WarningScreenContainer.currentPage].Item1
                        + "\n\n"
                        + WarningScreenContainer.pressAny;
                }
                else
                {
                    __instance.textBox.text =
                        "<color=red>�������!</color>\n"
                        + WarningScreenContainer.screens[WarningScreenContainer.currentPage].Item1
                        + "\n\n�����Ͳ�� ALT+F4 ��� �����";
                }
            }
            return false;
        }
    }

    // ��� Postfix-���� ������� *����* ����, �� MTM101BMDE ������ ����� �����������.
    // �� ������ �������� ���������� ����� �� ����������.
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

            // �������� ���������
            currentText = currentText.Replace("WARNING!", "�����!");
            currentText = currentText.Replace("ERROR!", "�������!");

            // �������� ������ ����������� (���� �� �)
            if (
                currentText.Contains(
                    "This game is not suitable for children or those who are easily disturbed."
                )
            )
            {
                currentText =
                    "�� ��� �� �������� ��� ����, �� ����� �� ������� ��������.\n���� ������ ���� �����, �� ������� ������.";
            }

            // �������� ���������� "Press any button to continue"
            if (currentText.Contains("PRESS ANY BUTTON TO CONTINUE"))
            {
                currentText = currentText.Replace(
                    "PRESS ANY BUTTON TO CONTINUE",
                    "�����Ͳ�� ����-��� ���²�� ��� ����������"
                );
            }
            else
            {
                // ��� �������� ���� ������� ��������� ������
                currentText = System.Text.RegularExpressions.Regex.Replace(
                    currentText,
                    @"PRESS (.+) TO CONTINUE",
                    "�����Ͳ�� $1 ��� ����������"
                );
            }

            // �������� ���������� �����
            currentText = currentText.Replace("PRESS ALT+F4 TO EXIT", "�����Ͳ�� ALT+F4 ��� �����");

            textBox.text = currentText;
        }
    }
}
