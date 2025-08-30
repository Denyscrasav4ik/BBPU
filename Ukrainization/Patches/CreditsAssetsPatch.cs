using System.IO;
using System.Linq;
using HarmonyLib;
using MTM101BaldAPI.AssetTools;
using Ukrainization.API;
using UnityEngine;

namespace Ukrainization.Patches
{
    [HarmonyPatch(typeof(Credits), "Start")]
    internal class CreditsAssetsPatch
    {
        private static bool textureReplaced = false;

        [HarmonyPostfix]
        [HarmonyPriority(Priority.High)]
        private static void CreditsStartPostfix(Credits __instance)
        {
            if (textureReplaced || !ConfigManager.AreTexturesEnabled())
            {
                return;
            }

            string textureName = "AwaitingSubmission";

            try
            {
                Texture2D originalTexture = Resources
                    .FindObjectsOfTypeAll<Texture2D>()
                    .FirstOrDefault(t => t.name == textureName);
                if (originalTexture == null)
                {
                    return;
                }

                string modPath = AssetLoader.GetModPath(TPPlugin.Instance);
                string filePath = Path.Combine(modPath, "Textures", textureName + ".png");

                if (!File.Exists(filePath))
                {
                    return;
                }

                Texture2D newTexture = AssetLoader.TextureFromFile(filePath);
                if (newTexture == null)
                {
                    API.Logger.Warning($"Не вдалось загрузити текстуру з {filePath}");
                    return;
                }

                if (
                    originalTexture.width != newTexture.width
                    || originalTexture.height != newTexture.height
                )
                {
                    API.Logger.Warning(
                        $"Texture size for '{textureName}' does not match original. Replacement cancelled."
                    );
                    Object.Destroy(newTexture);
                    return;
                }

                newTexture = AssetLoader.AttemptConvertTo(newTexture, originalTexture.format);
                AssetLoader.ReplaceTexture(originalTexture, newTexture);
                textureReplaced = true;
                API.Logger.Info($"Texture '{textureName}' in credits replaced.");
            }
            catch (System.Exception ex)
            {
                API.Logger.Error($"Помилка заміни текстур в титрах: {ex}");
            }
        }
    }
}
