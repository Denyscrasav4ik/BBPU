using HarmonyLib;
using MTM101BaldAPI.Reflection;
using UnityEngine;

namespace Ukrainization.Patches
{
    [System.Serializable]
    public class MinutesElement
    {
        public int num;
        public string key;

        public MinutesElement(int n, string k)
        {
            num = n;
            key = k;
        }
    }

    [HarmonyPatch(typeof(NoLateTeacher))]
    [HarmonyPatch("UpdateTimer")]
    public static class MrsPompMinutesPatch
    {
        private static string GetMinutesKey(int minutes)
        {
            if (minutes % 10 == 1 && minutes % 100 != 11)
            {
                return "Vfx_NoL_MinutesLeft_Additional"; // ...1 хвилина
            }
            return "Vfx_NoL_MinutesLeft"; // ...0, 2, 3, 4, 5... хвилин(и)
        }

        static void Postfix(NoLateTeacher __instance)
        {
            SoundObject sound = (SoundObject)
                ReflectionHelpers.ReflectionGetVariable(__instance, "audMinutesLeft");
            if (sound == null)
                return;

            int currentDisplayTime = (int)
                ReflectionHelpers.ReflectionGetVariable(__instance, "currentDisplayTime");
            int minutes = currentDisplayTime / 60;

            sound.soundKey = GetMinutesKey(minutes);
        }
    }
}
