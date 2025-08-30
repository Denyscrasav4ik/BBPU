using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Bootstrap;
using HarmonyLib;
using UnityEngine;

namespace Ukrainization.Patches
{
    [HarmonyPatch]
    public class FloorsPatch
    {
        private static readonly Dictionary<string, string> floorLocalizationKeys = new Dictionary<
            string,
            string
        >
        {
            { "MainLevel_1", "Ukr_Floor_Level1" },
            { "MainLevel_2", "Ukr_Floor_Level2" },
            { "MainLevel_3", "Ukr_Floor_Level3" },
            { "MainLevel_4", "Ukr_Floor_Level4" },
            { "MainLevel_5", "Ukr_Floor_Level5" },
            { "PlaceholderEnding", "Ukr_Floor_Ending" },
            { "Pitstop", "Ukr_Floor_Pitstop" },
            { "Tutorial", "Ukr_Floor_Tutorial" },
            { "EndlessPremadeMedium", "Ukr_Floor_EndlessPremade" },
            { "Endless_Factory_Large", "Ukr_Floor_Endless_Factory_Large" },
            { "Endless_Factory_Medium", "Ukr_Floor_Endless_Factory_Medium" },
            { "Endless_Laboratory_Large", "Ukr_Floor_Endless_Laboratory_Large" },
            { "Endless_Laboratory_Medium", "Ukr_Floor_Endless_Laboratory_Medium" },
            { "Endless_Maintenance_Large", "Ukr_Floor_Endless_Maintenance_Large" },
            { "Endless_Maintenance_Medium", "Ukr_Floor_Endless_Maintenance_Medium" },
            { "Endless_Schoolhouse_Large", "Ukr_Floor_Endless_Schoolhouse_Large" },
            { "Endless_Schoolhouse_Medium", "Ukr_Floor_Endless_Schoolhouse_Medium" },
            { "Endless_Schoolhouse_Small", "Ukr_Floor_Endless_Schoolhouse_Small" },
            { "StealthyChallenge", "Ukr_Floor_StealthyChallenge" },
            { "GrappleChallenge", "Ukr_Floor_GrappleChallenge" },
            { "SpeedyChallenge", "Ukr_Floor_SpeedyChallenge" },
        };

        [HarmonyPatch(typeof(ElevatorScreen), "UpdateFloorDisplay")]
        [HarmonyPrefix]
        private static bool ElevatorScreenUpdateFloorDisplayPrefix(ElevatorScreen __instance)
        {
            if (Chainloader.PluginInfos.ContainsKey("com.pixelguy.bbtimes"))
            {
                try
                {
                    Type bbTimesManagerType = Type.GetType(
                        "BBTimes.Manager.BBTimesManager, BBTimes"
                    );
                    if (bbTimesManagerType != null)
                    {
                        FieldInfo currentLevelField = bbTimesManagerType.GetField(
                            "CurrentLevel",
                            BindingFlags.Public | BindingFlags.Static
                        );
                        if (currentLevelField != null)
                        {
                            SceneObject currentLevel = (SceneObject)
                                currentLevelField.GetValue(null);
                            if (currentLevel != null)
                            {
                                UpdateFloorTitle(currentLevel);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning(
                        $"Ukrainization: Failed to get CurrentLevel from BBTimes via reflection: {e.Message}"
                    );
                }

                return true;
            }

            if (
                Singleton<CoreGameManager>.Instance != null
                && Singleton<CoreGameManager>.Instance.sceneObject != null
            )
            {
                SceneObject currentScene = Singleton<CoreGameManager>.Instance.sceneObject;

                UpdateFloorTitle(currentScene);
            }

            return true;
        }

        private static void UpdateFloorTitle(SceneObject scene)
        {
            if (floorLocalizationKeys.TryGetValue(scene.name, out string localizationKey))
            {
                string localizedTitle = GetLocalizedFloorTitle(localizationKey);
                if (!string.IsNullOrEmpty(localizedTitle))
                {
                    scene.levelTitle = localizedTitle;
                }
            }
        }

        private static string GetLocalizedFloorTitle(string localizationKey)
        {
            string localizedTitle = Singleton<LocalizationManager>.Instance.GetLocalizedText(
                localizationKey
            );
            if (localizedTitle != localizationKey)
            {
                return localizedTitle;
            }
            return string.Empty;
        }
    }
}
