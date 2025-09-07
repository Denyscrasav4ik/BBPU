using System;
using BepInEx;
using MTM101BaldAPI;
using UnityEngine;

namespace Ukrainization.API
{
    public static class VersionCheck
    {
        public static bool CheckGameVersion(string expectedVersion, PluginInfo info)
        {
            if (Application.version != expectedVersion)
            {
                string errorMessage =
                    $"Версія гри ({Application.version}) не відповідає необхідній версії ({expectedVersion}). Мод може працювати некоректно.";
                API.Logger.Error(errorMessage);
                return false;
            }
            return true;
        }
    }
}
