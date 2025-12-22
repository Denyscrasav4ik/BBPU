using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ukrainization.API
{
    public static class UpdateChecker
    {
        private const string RepoOwner = "Denyscrasav4ik";
        private const string RepoName = "BBPU";
        private const string UpdateUrl = "https://gamebanana.com/mods/617076";

        public static bool IsUpdateAvailable { get; private set; } = false;
        public static string LatestVersionString { get; private set; } = string.Empty;
        public static string CurrentVersionString { get; private set; } =
            UkrainizationTemp.ModVersion;

        public static string GetReleasesPageUrl()
        {
            return UpdateUrl;
        }

        public static async Task CheckForUpdates()
        {
            IsUpdateAvailable = false;
            LatestVersionString = string.Empty;
            CurrentVersionString = UkrainizationTemp.ModVersion;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.Add(
                        new ProductInfoHeaderValue("UkrUpdateChecker", "1.0")
                    );

                    string url =
                        $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest";
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();

                        Regex tagRegex = new Regex("\"tag_name\"\\s*:\\s*\"([^\"]+)\"");
                        Match match = tagRegex.Match(jsonResponse);

                        if (match.Success && match.Groups.Count >= 2)
                        {
                            string latestVersionTag = match.Groups[1].Value;

                            if (!string.IsNullOrEmpty(latestVersionTag))
                            {
                                Version currentModVersion = new Version(CurrentVersionString);
                                string sanitizedLatestVersion = latestVersionTag.StartsWith("v")
                                    ? latestVersionTag.Substring(1)
                                    : latestVersionTag;

                                try
                                {
                                    Version latestGitHubVersion = new Version(
                                        sanitizedLatestVersion
                                    );

                                    if (latestGitHubVersion > currentModVersion)
                                    {
                                        Logger.Warning(
                                            $"Доступна нова версія мода: {latestVersionTag}! Поточна версія: v{CurrentVersionString}"
                                        );
                                        IsUpdateAvailable = true;
                                        LatestVersionString = latestVersionTag;
                                    }
                                    else
                                    {
                                        Logger.Info("Встановлено останню версію мода.");
                                    }
                                }
                                catch (Exception) { }
                            }
                            else { }
                        }
                        else { }
                    }
                    else { }
                }
            }
            catch (Exception) { }
        }
    }
}
