using HarmonyLib;
using UnityEngine;

namespace Ukrainization.Patches
{
    [HarmonyPatch(typeof(Shader))]
    [HarmonyPatch("SetGlobalTexture", typeof(string), typeof(Texture))]
    class CubemapPatch
    {
        static void Prefix(string name, ref Texture value)
        {
            if (
                name == "_Skybox"
                && value is Cubemap cubemap
                && TPPlugin.CubemapReplacements.TryGetValue(cubemap, out var replacement)
            )
            {
                value = replacement;
            }
        }
    }
}
