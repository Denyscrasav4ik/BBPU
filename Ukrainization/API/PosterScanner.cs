using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Ukrainization.API
{
    public static class PosterScanner
    {
        /// <param name="modPath">Шлях до папки моду</param>
        public static void ScanAndExportNewPosters(string modPath)
        {
            if (!ConfigManager.IsDevModeEnabled())
            {
                Logger.Debug("Режим розробника вимкнено. Сканування плакатів пропущено.");
                return;
            }

            string postersPath = Path.Combine(modPath, "PosterFiles");
            if (!Directory.Exists(postersPath))
            {
                Directory.CreateDirectory(postersPath);
                Logger.ForceInfo($"[DEV MODE] Створена папка для плакатва: {postersPath}");
            }

            Logger.ForceInfo("=== ПОЧАЛО СКАНУВАННЯ ПЛАКАТІВ (DEV MODE) ===");
            Logger.ForceInfo($"Сканування ігрових ресурсів...");

            PosterObject[] allPosters = Resources.FindObjectsOfTypeAll<PosterObject>();
            Logger.ForceInfo($"Знайдено плакатів у грі: {allPosters.Length}");
            Logger.ForceInfo($"Перевірка папки: {postersPath}");
            Logger.ForceInfo("---");

            int newPostersCount = 0;
            int existingPostersCount = 0;
            int errorCount = 0;

            foreach (PosterObject poster in allPosters)
            {
                try
                {
                    string posterFolderPath = Path.Combine(postersPath, poster.name);
                    string posterDataPath = Path.Combine(posterFolderPath, "PosterData.json");

                    Logger.ForceInfo($"Перевірка плакату: {poster.name}");

                    if (File.Exists(posterDataPath))
                    {
                        existingPostersCount++;
                        Logger.ForceInfo($"  └─ Статус: УЖЕ ІСНУЄ");
                        continue;
                    }

                    newPostersCount++;
                    Logger.ForceInfo($"  └─ Статус: [НОВИЙ ПЛАКАТ]");

                    if (!Directory.Exists(posterFolderPath))
                    {
                        Directory.CreateDirectory(posterFolderPath);
                        Logger.ForceInfo($"  └─ [СТВОРЕНА ПАПКА]: {posterFolderPath}");
                    }
                    else
                    {
                        Logger.ForceInfo($"  └─ Папка вже існує: {posterFolderPath}");
                    }

                    ExportPosterData(poster, posterDataPath);
                    LogPosterDetails(poster);
                    Logger.ForceInfo("---");
                }
                catch (Exception ex)
                {
                    errorCount++;
                    Logger.ForceWarning(
                        $"  └─ [ПОМИЛКА] при обробці плакату '{poster.name}': {ex.Message}"
                    );
                    Logger.ForceInfo("---");
                }
            }

            Logger.ForceInfo("=== СКАНУВАННЯ ПЛАКАТІВ ЗАВЕРШЕНО ===");
            Logger.ForceInfo($"Результати сканування:");
            Logger.ForceInfo($"  Усього плакатів у грі: {allPosters.Length}");
            Logger.ForceInfo($"  Уже існує: {existingPostersCount}");
            Logger.ForceInfo($"  Нових знайдено: {newPostersCount}");

            if (errorCount > 0)
            {
                Logger.ForceWarning($"  Помилок при обробці: {errorCount}");
            }

            if (newPostersCount > 0)
            {
                Logger.ForceInfo("");
                Logger.ForceWarning("!!! УВАГА !!!");
                Logger.ForceWarning($"Знайдено НОВИХ плакатів: {newPostersCount}");
                Logger.ForceWarning("Перевірте папку PosterFiles і додайте переклади!");
                Logger.ForceWarning("Після додавання перекладів вимкніть Dev Mode в конфігу!");
            }
            else
            {
                Logger.ForceInfo("Нових плакатів не знайдено. Усі плакати вже додані.");
            }
        }

        private static void ExportPosterData(PosterObject poster, string outputPath)
        {
            PosterTextTable posterTable = new PosterTextTable();

            if (poster.textData != null && poster.textData.Length > 0)
            {
                Logger.ForceInfo($"  └─ Експорт данних плакату...");
                foreach (var textData in poster.textData)
                {
                    PosterTextData exportData = new PosterTextData
                    {
                        textKey = textData.textKey,
                        position = new IntVector2(textData.position.x, textData.position.z),
                        size = new IntVector2(textData.size.x, textData.size.z),
                        fontSize = textData.fontSize,
                        color = textData.color,
                    };

                    posterTable.items.Add(exportData);
                }
            }

            string json = JsonUtility.ToJson(posterTable, true);
            File.WriteAllText(outputPath, json);

            Logger.ForceInfo($"  └─ [СТВОРЕНИЙ JSON]: PosterData.json");
            Logger.ForceInfo($"     Текстових елементів: {posterTable.items.Count}");
            Logger.ForceInfo($"     Повний шлях: {outputPath}");
        }

        private static void LogPosterDetails(PosterObject poster)
        {
            Logger.ForceInfo($"  └─ Деталі плакату:");
            Logger.ForceInfo($"     Текстових елементів: {poster.textData?.Length ?? 0}");

            if (poster.textData != null && poster.textData.Length > 0)
            {
                for (int i = 0; i < poster.textData.Length; i++)
                {
                    var textData = poster.textData[i];
                    Logger.Debug(
                        $"     Елемент {i}: Key='{textData.textKey}', FontSize={textData.fontSize}, Pos=({textData.position.x}, {textData.position.z})"
                    );
                }
            }
        }
    }
}
