using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using HikariGGNPatcher.Misc;
using Microsoft.Win32;

namespace HikariGGNPatcher
{
    public class Patcher
    {
        #region Constants

        private static readonly Dictionary<string, Dictionary<int, string>> PATCH_DATA = new Dictionary<string, Dictionary<int, string>>
        {
            { "resources.assets", new Dictionary<int, string>
                {
                    { 673, "go2english.dat" }
                }
            }
        };

        private static readonly string TMP_FOLDER = Path.Combine(Path.GetTempPath(), "hikari_ggn");
        private const string GGN_NAME = @"GoGoNippon\GoGoNippon2015";
        private const string FILE_SUBPATH = "ggn2015_data";
        private const string EXE_GGN = "ggn2015.exe";
        private const string EXE_STEAM = "steam.exe";
        private const string STEAM_PARAMS = "-applaunch 251870";

        #endregion

        #region Properties & Fields

        private string _gamePath;
        private string _steamPath;

        public bool IsSteamPathFound => !string.IsNullOrWhiteSpace(_steamPath);

        #endregion

        #region DLL-Imports

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool SetDllDirectory(string lpPathName);

        [DllImport("AssetReplacorWrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern bool openAssetFile(string inputAssetPath);

        [DllImport("AssetReplacorWrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern bool replaceAsset(int pathId, string file);

        [DllImport("AssetReplacorWrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern bool saveAssetFile(string outputAssetPath);

        [DllImport("AssetReplacorWrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern bool cleanup();

        #endregion

        #region Methods

        public void Setup()
        {
            if (Directory.Exists(TMP_FOLDER))
                Directory.Delete(TMP_FOLDER, true);
            Directory.CreateDirectory(TMP_FOLDER);

            ResourceHelper.WriteResourceToFile(Path.Combine(TMP_FOLDER, "AssetsTools.dll"), "HikariGGNPatcher.Resources.Libs.AssetsTools.dll");
            ResourceHelper.WriteResourceToFile(Path.Combine(TMP_FOLDER, "AssetReplacorWrapper.dll"), "HikariGGNPatcher.Resources.Libs.AssetReplacorWrapper.dll");
            foreach (string data in PATCH_DATA.Select(x => x.Value).SelectMany(x => x.Values))
                ResourceHelper.WriteResourceToFile(Path.Combine(TMP_FOLDER, data), "HikariGGNPatcher.Resources.Patch." + data);

            if (Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\Installer\Products\1D5E3C0FEDA1E123187686FED06E995A") == null)
                MessageBox.Show("Es scheint, als wäre 'Microsoft Visual C++ 2010 Redistributable (x86)' nicht installiert.\r\nDie Redistributables werden benötigt, um den Patch ausführen zu können. Du kannst sie kostenlos aus dem Microsoft-Download-Center herunterladen.");

            SetDllDirectory(TMP_FOLDER);
        }

        public void Cleanup()
        {
            try { Directory.Delete(TMP_FOLDER, true); }
            catch {/* fking IO - catch'em all */}
        }

        public void StartGame()
        {
            if (_steamPath == null || !Directory.Exists(_steamPath) || !File.Exists(Path.Combine(_steamPath, EXE_STEAM))) return;

            try
            {
                Process.Start(Path.Combine(_steamPath, EXE_STEAM), STEAM_PARAMS);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.Replace("%1", "'" + Path.Combine(_gamePath, EXE_GGN) + "'"), "Beim Starten des Spiels ist ein Fehler aufgetreten!");
            }
        }

        public bool ApplyPatch()
        {
            if (_steamPath == null || !Directory.Exists(_steamPath) || !File.Exists(Path.Combine(_steamPath, EXE_STEAM))) return false;

            try
            {
                foreach (KeyValuePair<string, Dictionary<int, string>> fileData in PATCH_DATA)
                {
                    string assetFilePath = Path.Combine(_gamePath, FILE_SUBPATH, fileData.Key);
                    string tmpAssetFilePath = Path.Combine(_gamePath, FILE_SUBPATH, fileData.Key + ".hikaritmp");

                    openAssetFile(assetFilePath);
                    foreach (KeyValuePair<int, string> data in fileData.Value)
                    {
                        string filePath = Path.Combine(TMP_FOLDER, data.Value);
                        replaceAsset(data.Key, filePath);
                    }
                    saveAssetFile(tmpAssetFilePath);
                    cleanup();

                    File.Delete(assetFilePath);
                    File.Move(tmpAssetFilePath, assetFilePath);
                }

                MessageBox.Show("Viel Spaß mit der deutschen Übersetzung von Go! Go! Nippon!", "Patchvorgang erfolgreich!");
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Beim Patchen ist ein Fehler aufgetreten!");
                return false;
            }
        }

        public bool InitializeGamePath()
        {
            List<string> checkFiles = PATCH_DATA.Select(x => Path.Combine(FILE_SUBPATH, x.Key)).ToList();
            checkFiles.Add(EXE_GGN);
            _gamePath = SteamHelper.GetGamePath(GGN_NAME, checkFiles);
            return _gamePath != null;
        }

        public bool InitializeSteamPath()
        {
            _steamPath = SteamHelper.GetSteamInstallDir();
            return _steamPath != null;
        }

        public bool AskUserForPath()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog
            {
                Description = "Installiere bitte Steam,'Go! Go! Nippon!' sowie den DLC.\r\n" +
                              "Dann wähle dein 'Go! Go! Nippon!'-Installationsverzeichnis (das mit der 'ggn2015.exe') aus."
            };

            if (dialog.ShowDialog() == DialogResult.OK)
                if (File.Exists(Path.Combine(dialog.SelectedPath, EXE_GGN)))
                {
                    _gamePath = dialog.SelectedPath;

                    try
                    {
                        if (!IsSteamPathFound &&
                            File.Exists(Path.GetFullPath(Path.Combine(_gamePath, @"..\..\..\", EXE_STEAM))))
                            _steamPath = Path.GetFullPath(Path.Combine(_gamePath, @"..\..\..\"));
                    }
                    catch {/* fking IO - catch'em all */}

                    return true;
                }
                else
                    MessageBox.Show("Der gewählte Ordner scheint nicht das korrekte 'Go! Go! Nippon!'-Installationsverzeichnis zu sein.", "Ungültiger Ordner");

            return false;
        }

        #endregion
    }
}
