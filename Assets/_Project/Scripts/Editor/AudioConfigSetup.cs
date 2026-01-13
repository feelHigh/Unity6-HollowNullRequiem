// ============================================
// AudioConfigSetup.cs
// Editor tool for auto-linking audio files to AudioConfigSO
// ============================================

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using HNR.Audio;

namespace HNR.Editor
{
    /// <summary>
    /// Editor tool for automatically discovering and linking audio files
    /// to the AudioConfigSO asset based on folder structure and naming conventions.
    /// </summary>
    public static class AudioConfigSetup
    {
        private const string CONFIG_PATH = "Assets/_Project/Data/Config/AudioConfig.asset";
        private const string MUSIC_PATH = "Assets/_Project/Audio/Music";
        private const string SFX_PATH = "Assets/_Project/Audio/SFX";

        // ============================================
        // Audio File Definitions
        // ============================================

        // Music tracks - filename to ID mapping with loop settings
        private static readonly (string filename, string id, bool loop)[] MusicDefinitions = new[]
        {
            ("music_main_menu", "music_main_menu", true),
            ("music_bastion", "music_bastion", true),
            ("music_zone1", "music_zone1", true),
            ("music_zone2", "music_zone2", true),
            ("music_zone3", "music_zone3", true),
            ("combat_theme", "combat_theme", true),
            ("boss_theme", "boss_theme", true),
            ("zone1_elite_theme", "zone1_elite_theme", true),
            ("zone2_elite_theme", "zone2_elite_theme", true),
            ("shop_theme", "shop_theme", true),
            ("sanctuary_theme", "sanctuary_theme", true)
        };

        // Combat SFX - filename to ID mapping
        private static readonly (string filename, string id)[] CombatDefinitions = new[]
        {
            ("damage_hit", "damage_hit"),
            ("critical_hit", "critical_hit"),
            ("heal", "heal"),
            ("enemy_attack", "enemy_attack"),
            ("enemy_defeated", "enemy_defeated")
        };

        // Status effect SFX
        private static readonly (string filename, string id)[] StatusDefinitions = new[]
        {
            ("status_burn", "status_burn"),
            ("status_poison", "status_poison"),
            ("status_weakness", "status_weakness"),
            ("status_vulnerability", "status_vulnerability"),
            ("status_strength", "status_strength"),
            ("status_regen", "status_regen"),
            ("status_tick", "status_tick")
        };

        // Requiem Art SFX
        private static readonly (string filename, string id)[] ArtsDefinitions = new[]
        {
            ("art_activate", "art_activate"),
            ("art_kira", "art_kira"),
            ("art_mordren", "art_mordren"),
            ("art_elara", "art_elara"),
            ("art_thornwick", "art_thornwick")
        };

        // Element attack SFX
        private static readonly (string filename, string id)[] ElementsDefinitions = new[]
        {
            ("flame_attack", "flame_attack"),
            ("shadow_attack", "shadow_attack"),
            ("light_attack", "light_attack"),
            ("nature_attack", "nature_attack")
        };

        // Shop SFX
        private static readonly (string filename, string id)[] ShopDefinitions = new[]
        {
            ("purchase", "purchase"),
            ("relic_acquire", "relic_acquire"),
            ("card_acquire", "card_acquire"),
            ("void_shards_gain", "void_shards_gain")
        };

        // Stingers SFX
        private static readonly (string filename, string id, bool loop)[] StingersDefinitions = new[]
        {
            ("victory", "victory", false),
            ("defeat", "defeat", false),
            ("low_health", "low_health", true)
        };

        // ============================================
        // Public Methods
        // ============================================

        /// <summary>
        /// Setup AudioConfig by discovering and linking all audio files.
        /// </summary>
        public static void SetupAudioConfig()
        {
            // Load or create AudioConfig
            var config = AssetDatabase.LoadAssetAtPath<AudioConfigSO>(CONFIG_PATH);
            if (config == null)
            {
                Debug.LogError($"[AudioConfigSetup] AudioConfig not found at {CONFIG_PATH}. Create it first via HNR > 3. Audio & VFX > Generate Audio Config");
                return;
            }

            EditorUtility.DisplayProgressBar("Audio Config Setup", "Scanning audio files...", 0f);

            try
            {
                // Clear existing entries
                config.ClearAllEntries();

                int totalLinked = 0;

                // Link Music
                EditorUtility.DisplayProgressBar("Audio Config Setup", "Linking music...", 0.1f);
                totalLinked += LinkMusicFiles(config);

                // Link Combat SFX
                EditorUtility.DisplayProgressBar("Audio Config Setup", "Linking combat SFX...", 0.25f);
                totalLinked += LinkCategorySFX(config, $"{SFX_PATH}/Combat", CombatDefinitions, AudioCategory.Combat);

                // Link Status SFX
                EditorUtility.DisplayProgressBar("Audio Config Setup", "Linking status SFX...", 0.4f);
                totalLinked += LinkCategorySFX(config, $"{SFX_PATH}/Status", StatusDefinitions, AudioCategory.Status);

                // Link Arts SFX
                EditorUtility.DisplayProgressBar("Audio Config Setup", "Linking arts SFX...", 0.55f);
                totalLinked += LinkCategorySFX(config, $"{SFX_PATH}/Arts", ArtsDefinitions, AudioCategory.Arts);

                // Link Elements SFX
                EditorUtility.DisplayProgressBar("Audio Config Setup", "Linking elements SFX...", 0.7f);
                totalLinked += LinkCategorySFX(config, $"{SFX_PATH}/Elements", ElementsDefinitions, AudioCategory.Elements);

                // Link Shop SFX
                EditorUtility.DisplayProgressBar("Audio Config Setup", "Linking shop SFX...", 0.85f);
                totalLinked += LinkCategorySFX(config, $"{SFX_PATH}/Shop", ShopDefinitions, AudioCategory.Shop);

                // Link Stingers SFX
                EditorUtility.DisplayProgressBar("Audio Config Setup", "Linking stingers...", 0.95f);
                totalLinked += LinkStingersSFX(config);

                // Save changes
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();

                EditorUtility.ClearProgressBar();

                Debug.Log($"[AudioConfigSetup] Successfully linked {totalLinked} audio files to AudioConfig");

                EditorUtility.DisplayDialog("Audio Config Setup Complete",
                    $"Successfully linked {totalLinked} audio files:\n\n" +
                    $"- Music: {config.MusicEntries.Count}\n" +
                    $"- Combat: {config.CombatEntries.Count}\n" +
                    $"- Status: {config.StatusEntries.Count}\n" +
                    $"- Arts: {config.ArtsEntries.Count}\n" +
                    $"- Elements: {config.ElementsEntries.Count}\n" +
                    $"- Shop: {config.ShopEntries.Count}\n" +
                    $"- Stingers: {config.StingersEntries.Count}\n\n" +
                    "Check the Inspector for AudioConfig.asset to verify.",
                    "OK");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        /// <summary>
        /// Verify AudioConfig by checking for missing clips.
        /// </summary>
        public static void VerifyAudioConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<AudioConfigSO>(CONFIG_PATH);
            if (config == null)
            {
                Debug.LogError($"[AudioConfigSetup] AudioConfig not found at {CONFIG_PATH}");
                return;
            }

            var missingClips = new List<string>();
            int totalEntries = 0;

            // Check all categories
            CheckCategoryForMissing(config.MusicEntries, "Music", missingClips, ref totalEntries);
            CheckCategoryForMissing(config.CombatEntries, "Combat", missingClips, ref totalEntries);
            CheckCategoryForMissing(config.StatusEntries, "Status", missingClips, ref totalEntries);
            CheckCategoryForMissing(config.ArtsEntries, "Arts", missingClips, ref totalEntries);
            CheckCategoryForMissing(config.ElementsEntries, "Elements", missingClips, ref totalEntries);
            CheckCategoryForMissing(config.ShopEntries, "Shop", missingClips, ref totalEntries);
            CheckCategoryForMissing(config.StingersEntries, "Stingers", missingClips, ref totalEntries);

            if (missingClips.Count == 0)
            {
                Debug.Log($"[AudioConfigSetup] All {totalEntries} audio entries have valid clips!");
                EditorUtility.DisplayDialog("Audio Config Verification",
                    $"All {totalEntries} audio entries are properly configured with valid clips.",
                    "OK");
            }
            else
            {
                string missingList = string.Join("\n", missingClips);
                Debug.LogWarning($"[AudioConfigSetup] Found {missingClips.Count} entries with missing clips:\n{missingList}");
                EditorUtility.DisplayDialog("Audio Config Verification",
                    $"Found {missingClips.Count} entries with missing clips:\n\n{missingList}\n\n" +
                    "Run 'Setup Audio Config' to re-link files.",
                    "OK");
            }
        }

        /// <summary>
        /// List all audio files found in the Audio folder.
        /// </summary>
        public static void ListAudioFiles()
        {
            var musicFiles = FindAudioFiles(MUSIC_PATH);
            var sfxFiles = FindAllSFXFiles();

            Debug.Log($"[AudioConfigSetup] Audio Files Found:\n\n" +
                $"=== MUSIC ({musicFiles.Count}) ===\n{string.Join("\n", musicFiles)}\n\n" +
                $"=== SFX ({sfxFiles.Count}) ===\n{string.Join("\n", sfxFiles)}");

            EditorUtility.DisplayDialog("Audio Files Found",
                $"Music: {musicFiles.Count} files\nSFX: {sfxFiles.Count} files\n\n" +
                "Check console for detailed list.",
                "OK");
        }

        // ============================================
        // Private Methods
        // ============================================

        private static int LinkMusicFiles(AudioConfigSO config)
        {
            int linked = 0;

            foreach (var (filename, id, loop) in MusicDefinitions)
            {
                var clip = FindAudioClip(MUSIC_PATH, filename);
                if (clip != null)
                {
                    var entry = new AudioEntry(id, clip, AudioCategory.Music, 0.8f, loop);
                    config.AddEntry(entry);
                    linked++;
                    Debug.Log($"[AudioConfigSetup] Linked music: {id} -> {clip.name}");
                }
                else
                {
                    Debug.LogWarning($"[AudioConfigSetup] Music file not found: {filename}");
                }
            }

            return linked;
        }

        private static int LinkCategorySFX(AudioConfigSO config, string folderPath, (string filename, string id)[] definitions, AudioCategory category)
        {
            int linked = 0;

            foreach (var (filename, id) in definitions)
            {
                var clip = FindAudioClip(folderPath, filename);
                if (clip != null)
                {
                    var entry = new AudioEntry(id, clip, category, 1f, false);
                    config.AddEntry(entry);
                    linked++;
                    Debug.Log($"[AudioConfigSetup] Linked {category}: {id} -> {clip.name}");
                }
                else
                {
                    Debug.LogWarning($"[AudioConfigSetup] SFX file not found: {filename} in {folderPath}");
                }
            }

            return linked;
        }

        private static int LinkStingersSFX(AudioConfigSO config)
        {
            int linked = 0;
            string folderPath = $"{SFX_PATH}/Stingers";

            foreach (var (filename, id, loop) in StingersDefinitions)
            {
                var clip = FindAudioClip(folderPath, filename);
                if (clip != null)
                {
                    var entry = new AudioEntry(id, clip, AudioCategory.Stingers, 1f, loop);
                    config.AddEntry(entry);
                    linked++;
                    Debug.Log($"[AudioConfigSetup] Linked stinger: {id} -> {clip.name}");
                }
                else
                {
                    Debug.LogWarning($"[AudioConfigSetup] Stinger file not found: {filename} in {folderPath}");
                }
            }

            return linked;
        }

        private static AudioClip FindAudioClip(string folderPath, string filename)
        {
            // Try common audio extensions
            string[] extensions = { ".mp3", ".wav", ".ogg" };

            foreach (var ext in extensions)
            {
                string assetPath = $"{folderPath}/{filename}{ext}";
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
                if (clip != null)
                {
                    return clip;
                }
            }

            return null;
        }

        private static List<string> FindAudioFiles(string folderPath)
        {
            var files = new List<string>();

            if (!Directory.Exists(folderPath))
            {
                return files;
            }

            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { folderPath });
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                files.Add(Path.GetFileName(path));
            }

            return files;
        }

        private static List<string> FindAllSFXFiles()
        {
            var files = new List<string>();
            string[] subfolders = { "Combat", "Status", "Arts", "Elements", "Shop", "Stingers" };

            foreach (var folder in subfolders)
            {
                string path = $"{SFX_PATH}/{folder}";
                if (Directory.Exists(path))
                {
                    var folderFiles = FindAudioFiles(path);
                    foreach (var file in folderFiles)
                    {
                        files.Add($"{folder}/{file}");
                    }
                }
            }

            return files;
        }

        private static void CheckCategoryForMissing(IReadOnlyList<AudioEntry> entries, string categoryName, List<string> missingClips, ref int totalEntries)
        {
            foreach (var entry in entries)
            {
                totalEntries++;
                if (entry.Clip == null)
                {
                    missingClips.Add($"[{categoryName}] {entry.Id}");
                }
            }
        }
    }
}
