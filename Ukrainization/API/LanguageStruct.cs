﻿using System.Collections.Generic;
using UnityEngine;

namespace Ukrainization.API
{
    [System.Serializable]
    public class LanguageStruct
    {
        public string LanguageCodeName = "English";

        private Dictionary<string, string> localizationTable = new Dictionary<string, string>();

        public void AddKey(string key, string value)
        {
            if (!localizationTable.ContainsKey(key))
            {
                localizationTable.Add(key, value);
            }
            else
            {
                Logger.Warning(
                    $"Однаковий ключ {{key}} вже знайдено!\nВикористовуйте language_LANGCODENAME.config для налаштування необхідних дій: замініть ключі та значення на нові або пропустіть."
                );
            }
        }

        public bool ContainsKey(string key)
        {
            return localizationTable.ContainsKey(key);
        }

        public bool ContainsValue(string value)
        {
            return localizationTable.ContainsValue(value);
        }

        public string GetLocalizatedText(string key)
        {
            return localizationTable[key];
        }
    }
}
