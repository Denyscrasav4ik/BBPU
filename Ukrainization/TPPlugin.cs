using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;
using Ukrainization.API;
using UnityEngine;

namespace Ukrainization
{
    [Serializable]
    public class PosterTextData
    {
        public string textKey = string.Empty;

        public IntVector2 position;

        public IntVector2 size;

        public int fontSize;

        public Color color;
    }

    [Serializable]
    public class PosterTextTable
    {
        public List<PosterTextData> items = new List<PosterTextData>();
    }

    [BepInPlugin(
        UkrainizationTemp.ModGUID,
        UkrainizationTemp.ModName,
        UkrainizationTemp.ModVersion
    )]
    [BepInDependency(
        "mtm101.rulerp.bbplus.baldidevapi",
        BepInDependency.DependencyFlags.HardDependency
    )]
    [BepInProcess("BALDI.exe")]
    public class TPPlugin : BaseUnityPlugin
    {
        public static TPPlugin Instance { get; private set; } = null!;
        public static Dictionary<string, AudioClip> AllClips { get; private set; } =
            new Dictionary<string, AudioClip>();
        private Harmony? harmonyInstance = null!;
        private const string expectedGameVersion = "0.13";

        private static readonly string[] menuTextureNames =
        {
            "About_Lit",
            "About_Unlit",
            "Options_Lit",
            "Options_Unlit",
            "Play_Lit",
            "Play_Unlit",
            "TempMenu_Low",
        };

        void Awake()
        {
            Instance = this;
            API.Logger.Init(Logger);
            ConfigManager.Initialize(this, Logger);

            API.Logger.Info($"Плагін {UkrainizationTemp.ModName} ініціалізовано.");
            API.Logger.Info(
                $"Текстури: {(ConfigManager.AreTexturesEnabled() ? "Увімкнені" : "Вимкнені")}, "
                    + $"Звуки: {(ConfigManager.AreSoundsEnabled() ? "Увімкнені" : "Вимкнені")}, "
                    + $"Логування: {(ConfigManager.IsLoggingEnabled() ? "Увімкнене" : "Вимкнене")}, "
                    + $"Режим розробки: {(ConfigManager.IsDevModeEnabled() ? "УВІМКНЕНИЙ" : "Вимкнений")}"
            );

            harmonyInstance = new Harmony(UkrainizationTemp.ModGUID);
            harmonyInstance.PatchAll();

            VersionCheck.CheckGameVersion(expectedGameVersion, Info);

            string modPath = AssetLoader.GetModPath(this);
            string langPath = Path.Combine(modPath, "Language", "Ukrainian");
            if (Directory.Exists(langPath))
            {
                API.Logger.Info($"Знайдено теку локалізації: {langPath}");
                AssetLoader.LoadLocalizationFolder(langPath, Language.English);
            }

            LoadingEvents.RegisterOnAssetsLoaded(Info, OnAssetsLoaded(), LoadingEventOrder.Post);

            gameObject.AddComponent<MenuTextureManager>();
        }

        private IEnumerator OnAssetsLoaded()
        {
            yield return 3;

            yield return "Завантаження ресурсів українізатора...";
            API.Logger.Info("Завантаження локалізованих ресурсів...");

            string modPath = AssetLoader.GetModPath(this);

            yield return "Завантаження текстур...";
            ApplyAllTextures();

            yield return "Завантаження звуків...";
            if (ConfigManager.AreSoundsEnabled())
            {
                string audiosPath = Path.Combine(modPath, "Audios");
                if (Directory.Exists(audiosPath))
                {
                    API.Logger.Info(
                        $"Знайдено теку зі звуками: {audiosPath}, виконується кешування та заміна..."
                    );

                    string[] audioFiles = Directory
                        .GetFiles(audiosPath, "*.wav")
                        .Concat(Directory.GetFiles(audiosPath, "*.ogg"))
                        .ToArray();
                    foreach (string audioFile in audioFiles)
                    {
                        string clipName = Path.GetFileNameWithoutExtension(audioFile);
                        if (!AllClips.ContainsKey(clipName))
                        {
                            AudioClip newClip = AssetLoader.AudioClipFromFile(audioFile);
                            if (newClip)
                            {
                                newClip.name = clipName;
                                AllClips.Add(clipName, newClip);
                                API.Logger.Info($"Аудіокліп '{clipName}' кешовано.");
                            }
                        }
                    }

                    SoundObject[] allSounds = Resources.FindObjectsOfTypeAll<SoundObject>();
                    foreach (SoundObject soundObject in allSounds)
                    {
                        if (AllClips.TryGetValue(soundObject.name, out AudioClip newClip))
                        {
                            soundObject.soundClip = newClip;
                            API.Logger.Info($"Звук '{soundObject.name}' замінено.");
                        }
                    }
                }
            }

            yield return "Оновлення плакатів...";
            UpdatePosters(modPath);

            // Сканування нових плакатів у режимі розробки

            if (ConfigManager.IsDevModeEnabled())
            {
                yield return "Сканування нових плакатів (DEV MODE)...";

                PosterScanner.ScanAndExportNewPosters(modPath);
            }

            API.Logger.Info("Завантаження ресурсів завершено!");
        }

        public void ApplyMenuTextures()
        {
            if (!ConfigManager.AreTexturesEnabled())
                return;

            string modPath = AssetLoader.GetModPath(this);
            string texturesPath = Path.Combine(modPath, "Textures");

            if (Directory.Exists(texturesPath))
            {
                API.Logger.Info("Застосування текстур головного меню...");
                Texture2D[] allGameTextures = Resources.FindObjectsOfTypeAll<Texture2D>();
                foreach (string textureName in menuTextureNames)
                {
                    Texture2D originalTexture = allGameTextures.FirstOrDefault(t =>
                        t.name == textureName
                    );
                    if (originalTexture != null)
                    {
                        string textureFile = Path.Combine(texturesPath, textureName + ".png");
                        if (File.Exists(textureFile))
                        {
                            try
                            {
                                Texture2D newTexture = AssetLoader.TextureFromFile(textureFile);
                                if (newTexture != null)
                                {
                                    if (
                                        originalTexture.width != newTexture.width
                                        || originalTexture.height != newTexture.height
                                    )
                                    {
                                        API.Logger.Warning(
                                            $"Розмір текстури '{textureName}' не збігається з оригіналом. Заміна скасована."
                                        );
                                        continue;
                                    }

                                    newTexture = AssetLoader.AttemptConvertTo(
                                        newTexture,
                                        originalTexture.format
                                    );
                                    AssetLoader.ReplaceTexture(originalTexture, newTexture);
                                }
                            }
                            catch (Exception e)
                            {
                                API.Logger.Error(
                                    $"Помилка при заміні текстури '{textureName}': {e.Message}"
                                );
                            }
                        }
                    }
                }
            }
        }

        public void ApplyAllTextures()
        {
            if (!ConfigManager.AreTexturesEnabled())
                return;

            string modPath = AssetLoader.GetModPath(this);
            string texturesPath = Path.Combine(modPath, "Textures");

            if (Directory.Exists(texturesPath))
            {
                API.Logger.Info(
                    $"Знайдено теку з текстурами: {texturesPath}, виконується заміна..."
                );

                Texture2D[] allGameTextures = Resources.FindObjectsOfTypeAll<Texture2D>();
                string[] textureFiles = Directory.GetFiles(
                    texturesPath,
                    "*.png",
                    SearchOption.AllDirectories
                );

                foreach (string textureFile in textureFiles)
                {
                    string textureName = Path.GetFileNameWithoutExtension(textureFile);
                    Texture2D originalTexture = allGameTextures.FirstOrDefault(t =>
                        t.name == textureName
                    );

                    if (originalTexture != null)
                    {
                        try
                        {
                            Texture2D newTexture = AssetLoader.TextureFromFile(textureFile);
                            if (newTexture != null)
                            {
                                if (
                                    originalTexture.width != newTexture.width
                                    || originalTexture.height != newTexture.height
                                )
                                {
                                    API.Logger.Warning(
                                        $"Розмір текстури '{textureName}' не збігається з оригіналом. Заміна скасована."
                                    );
                                    continue;
                                }

                                newTexture = AssetLoader.AttemptConvertTo(
                                    newTexture,
                                    originalTexture.format
                                );
                                AssetLoader.ReplaceTexture(originalTexture, newTexture);
                                API.Logger.Info($"Текстура '{textureName}' замінена.");
                            }
                        }
                        catch (Exception e)
                        {
                            API.Logger.Error(
                                $"Помилка при заміні текстури '{textureName}': {e.Message}"
                            );
                        }
                    }
                    else
                    {
                        API.Logger.Warning($"Не знайдено відповідну текстуру для: {textureName}");
                    }
                }
            }
        }

        private void UpdatePosters(string modPath)
        {
            string postersPath = Path.Combine(modPath, "PosterFiles");
            if (!Directory.Exists(postersPath))
            {
                API.Logger.Warning("Теку з плакатами не знайдено, заміна не буде виконана.");
                return;
            }

            API.Logger.Info("Початок оновлення плакатів...");
            PosterObject[] allPosters = Resources.FindObjectsOfTypeAll<PosterObject>();
            foreach (PosterObject poster in allPosters)
            {
                string posterDataPath = Path.Combine(postersPath, poster.name, "PosterData.json");
                if (File.Exists(posterDataPath))
                {
                    try
                    {
                        PosterTextTable? posterData = JsonUtility.FromJson<PosterTextTable>(
                            File.ReadAllText(posterDataPath)
                        );

                        if (posterData != null)
                        {
                            for (
                                int i = 0;
                                i < Math.Min(posterData.items.Count, poster.textData.Length);
                                i++
                            )
                            {
                                var sourceData = poster.textData[i];
                                var modifiedData = posterData.items[i];

                                sourceData.textKey = modifiedData.textKey;

                                sourceData.position = new IntVector2(
                                    modifiedData.position.x,
                                    modifiedData.position.z
                                );

                                sourceData.size = new IntVector2(
                                    modifiedData.size.x,
                                    modifiedData.size.z
                                );
                                sourceData.fontSize = modifiedData.fontSize;
                                sourceData.color = modifiedData.color;
                            }

                            API.Logger.Info(
                                $"Оновленний плакат: {poster.name} із локалізованими даними"
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        API.Logger.Error($"Помилка при заміні плаката {poster.name}: {ex.Message}");
                    }
                }
            }
            API.Logger.Info("Оновлення плакатів завершено.");
        }

        void OnDestroy()
        {
            if (harmonyInstance != null)
            {
                harmonyInstance.UnpatchSelf();
                harmonyInstance = null;
            }
        }
    }
}
