using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace Ukrainization.Patches
{
    [HarmonyPatch(typeof(InputManager))]
    [HarmonyPatch(
        "GetInputButtonName",
        new Type[] { typeof(string), typeof(bool), typeof(string[]) }
    )]
    internal static class InputManagerPatch
    {
        private static readonly Dictionary<string, string> Map = new Dictionary<string, string>
        {
            { "Space", "Пробіл" },
            { "Caps Lock", "Замок Капсу" },
            { "Tab", "Таб" },
            { "ESC", "ЕСК" },
            { "Return", "Ентер" },
            { "Left Shift", "Лівий Шифт" },
            { "Right Shift", "Правий Шифт" },
            { "Left Control", "Лівий Контрол" },
            { "Right Control", "Правий Контрол" },
            { "Left Alt", "Лівий Альт" },
            { "Right Alt", "Правий Альт" },
            { "Up Arrow", "Стрілка Вгору" },
            { "Down Arrow", "Стрілка Вниз" },
            { "Left Arrow", "Стрілка Вліво" },
            { "Right Arrow", "Стрілка Вправо" },
            { "Back Quote", "Апостроф" },
            { "Left Mouse Button", "Ліва Клавіша Миші" },
            { "Right Mouse Button", "Права Клавіша Миші" },
            { "Mouse Button 3", "Клавіша Миші 3" },
            { "Mouse Wheel", "Коліщатко Миші" },
            { "Mouse Wheel Up", "Коліщатко Миші Вверх" },
            { "Mouse Wheel Down", "Коліщатко Миші Вниз" },
            { "Mouse Horizontal", "Миша По Горизонталі" },
            { "Mouse Vertical", "Миша По Вертикалі" },
            { "Mouse Up", "Миша Вверх" },
            { "Mouse Down", "Миша Вниз" },
            { "Mouse Left", "Миша Вліво" },
            { "Mouse Right", "Миша Вправо" },
            { "Left Shoulder", "Ліве Плече" },
            { "Right Shoulder", "Праве Плече" },
            { "Right Stick Button", "Кнопка Правого Стіку" },
            { "Left Stick Button", "Кнопка Лівого Стіку" },
            { "Right Stick X", "Правий Стік X" },
            { "Left Stick X", "Лівий Стік X" },
            { "Right Stick Y", "Правий Стік Y" },
            { "Left Stick Y", "Лівий Стік Y" },
            { "Left Trigger", "Лівий Тригер" },
            { "Right Trigger", "Правий Тригер" },
            { "Start", "Старт" },
            { "Back", "Назад" },
        };

        private static readonly Regex UkrainianRegex = new Regex(
            @"[\u0400-\u04FF]",
            RegexOptions.Compiled
        );

        static void Postfix(ref string __result)
        {
            if (string.IsNullOrEmpty(__result))
                return;

            if (UkrainianRegex.IsMatch(__result))
                return;

            foreach (var pair in Map)
            {
                var pattern = @"\b" + Regex.Escape(pair.Key) + @"\b";
                __result = Regex.Replace(__result, pattern, pair.Value);
            }
        }
    }
}
