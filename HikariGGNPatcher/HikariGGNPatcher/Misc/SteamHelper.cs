using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace HikariGGNPatcher.Misc
{
    public static class SteamHelper
    {
        #region Constants

        private const string STEAM_LIB_PATH = @"SteamApps\common";
        private const string STEAM_CONFIG_FILE = @"config/config.vdf";

        private static readonly Regex REGEX_CONFIG = new Regex(@"""BaseInstallFolder_\d""\s*""(?<path>[\w:\\\/] +)""", RegexOptions.Compiled);

        #endregion

        #region Methods

        public static string GetSteamInstallDir()
        {
            return Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam")?.GetValue("SteamPath")?.ToString();
        }

        public static IEnumerable<string> GetSteamGameInstallDirs()
        {
            string steamDir = GetSteamInstallDir();
            if (string.IsNullOrWhiteSpace(steamDir) || !Directory.Exists(steamDir)) return null;

            List<string> dirs = new List<string> { Path.Combine(steamDir, STEAM_LIB_PATH) };

            string configPath = Path.Combine(steamDir, STEAM_CONFIG_FILE);
            if (File.Exists(configPath))
            {
                string config = File.ReadAllText(configPath);
                MatchCollection matches = REGEX_CONFIG.Matches(config);
                foreach (Match match in matches)
                    if (match.Success)
                    {
                        string dir = Path.Combine(Path.Combine(match.Groups["path"].Value, STEAM_LIB_PATH));
                        if (Directory.Exists(dir))
                            dirs.Add(dir);
                    }
            }

            return dirs;
        }

        public static string GetGamePath(string game, IEnumerable<string> checkForFiles)
        {
            IEnumerable<string> steamInstallDirs = GetSteamGameInstallDirs();
            if (steamInstallDirs != null)
                foreach (string steamInstallDir in steamInstallDirs)
                {
                    string checkDir = Path.Combine(steamInstallDir, game);
                    if (Directory.Exists(checkDir) && (checkForFiles == null || !checkForFiles.Any() || checkForFiles.All(x => File.Exists(Path.Combine(checkDir, x)))))
                        return checkDir;
                }

            return null;
        }

        #endregion
    }
}
