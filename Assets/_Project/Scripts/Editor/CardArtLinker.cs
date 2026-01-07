// ============================================
// CardArtLinker.cs
// Editor tool to link generated card art to CardDataSO assets
// ============================================

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using HNR.Cards;

namespace HNR.Editor
{
    /// <summary>
    /// Links AI-generated card art images to CardDataSO assets.
    /// Randomly selects from 4 available variants (v01-v04) per card.
    /// </summary>
    public static class CardArtLinker
    {
        private const string CARD_DATA_PATH = "Assets/_Project/Data/Cards";
        private const string CARD_RESOURCES_PATH = "Assets/_Project/Resources/Data/Cards";
        private const string CARD_ART_PATH = "Assets/_Project/Art/Cards/Generated";

        // Special mappings for base cards that have different naming conventions
        private static readonly Dictionary<string, string> BaseCardMappings = new()
        {
            { "Strike_Basic", "base_strike" },
            { "Guard_Basic", "base_guard" },
            { "Flame_Burst", "base_flame_burst" },
            { "Heal_Light", "base_lights_embrace" },
            { "Draw_Arcane", "base_arcane_insight" }
        };

        // Special mappings for character Plus cards with different naming
        // Card asset name -> image base name (without character prefix)
        private static readonly Dictionary<string, string> PlusCardMappings = new()
        {
            // The _Strike_Plus cards use _basic_strike images
            { "Kira_Strike_Plus", "kira_basic_strike" },
            { "Elara_Strike_Plus", "elara_basic_strike" },
            { "Mordren_Strike_Plus", "mordren_basic_strike" },
            { "Thornwick_Strike_Plus", "thornwick_basic_strike" }
        };

        /// <summary>
        /// Links all card art to CardDataSO assets, randomly selecting from variants.
        /// </summary>
        public static void LinkAllCardArt()
        {
            LinkAllCardArt(null);
        }

        /// <summary>
        /// Links all card art to CardDataSO assets with optional seed for reproducibility.
        /// </summary>
        /// <param name="seed">Random seed (null for random selection each time)</param>
        public static void LinkAllCardArt(int? seed)
        {
            var random = seed.HasValue ? new System.Random(seed.Value) : new System.Random();

            // Find all CardDataSO assets from both Data and Resources folders
            var guidList = new List<string>();
            guidList.AddRange(AssetDatabase.FindAssets("t:CardDataSO", new[] { CARD_DATA_PATH }));
            guidList.AddRange(AssetDatabase.FindAssets("t:CardDataSO", new[] { CARD_RESOURCES_PATH }));
            string[] guids = guidList.ToArray();

            int linked = 0;
            int skipped = 0;
            int notFound = 0;
            var notFoundCards = new List<string>();

            EditorUtility.DisplayProgressBar("Linking Card Art", "Processing cards...", 0f);

            try
            {
                for (int i = 0; i < guids.Length; i++)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                    string assetName = Path.GetFileNameWithoutExtension(assetPath);

                    EditorUtility.DisplayProgressBar("Linking Card Art",
                        $"Processing {assetName}...", (float)i / guids.Length);

                    var cardData = AssetDatabase.LoadAssetAtPath<CardDataSO>(assetPath);
                    if (cardData == null) continue;

                    // Find matching art variants
                    var artVariants = FindArtVariants(assetName, assetPath);

                    if (artVariants.Count == 0)
                    {
                        notFound++;
                        notFoundCards.Add(assetName);
                        continue;
                    }

                    // Randomly select one variant
                    int selectedIndex = random.Next(artVariants.Count);
                    string selectedArtPath = artVariants[selectedIndex];

                    // Load the sprite
                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(selectedArtPath);
                    if (sprite == null)
                    {
                        Debug.LogWarning($"[CardArtLinker] Could not load sprite at: {selectedArtPath}");
                        notFound++;
                        notFoundCards.Add(assetName);
                        continue;
                    }

                    // Assign to CardDataSO
                    var so = new SerializedObject(cardData);
                    var artProperty = so.FindProperty("_cardArt");

                    if (artProperty.objectReferenceValue == sprite)
                    {
                        skipped++;
                        continue;
                    }

                    artProperty.objectReferenceValue = sprite;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(cardData);

                    linked++;
                    Debug.Log($"[CardArtLinker] Linked {assetName} -> {Path.GetFileName(selectedArtPath)}");
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Report results
            string message = $"Card Art Linking Complete!\n\n" +
                           $"Linked: {linked}\n" +
                           $"Skipped (already set): {skipped}\n" +
                           $"Not found: {notFound}";

            if (notFoundCards.Count > 0)
            {
                message += $"\n\nMissing art for:\n• {string.Join("\n• ", notFoundCards)}";
            }

            Debug.Log($"[CardArtLinker] {message.Replace("\n", " | ")}");
            EditorUtility.DisplayDialog("Card Art Linker", message, "OK");
        }

        /// <summary>
        /// Finds all art variants (v01-v04) for a given card.
        /// </summary>
        private static List<string> FindArtVariants(string assetName, string assetPath)
        {
            var variants = new List<string>();

            // Determine the folder and base image name
            string folder;
            string imageBaseName;

            // Check Plus card mappings first (e.g., Kira_Strike_Plus -> kira_basic_strike)
            if (TryGetPlusCardMapping(assetName, out string plusMapping, out string plusFolder))
            {
                folder = plusFolder;
                imageBaseName = plusMapping;
            }
            else if (TryGetBaseCardMapping(assetName, out string baseName))
            {
                // Special base card mapping
                folder = "Base";
                imageBaseName = baseName;
            }
            else if (assetName.StartsWith("Shared_"))
            {
                // Shared cards
                folder = "Shared";
                imageBaseName = assetName.ToLower().Replace("_", "_");
            }
            else if (assetName.StartsWith("Kira_") || assetName.StartsWith("Elara_") ||
                     assetName.StartsWith("Mordren_") || assetName.StartsWith("Thornwick_"))
            {
                // Character-specific cards
                string[] parts = assetName.Split('_');
                folder = parts[0]; // Character name

                // Handle Plus/Upgraded cards
                if (assetName.EndsWith("_Plus"))
                {
                    // First, try to find dedicated _plus images (e.g., kira_inferno_strike_plus)
                    string strippedName = assetName.Substring(0, assetName.Length - 5);
                    string plusImageName = strippedName.ToLower() + "_plus";

                    // Check if dedicated _plus images exist
                    string artFolderCheck = Path.Combine(CARD_ART_PATH, folder);
                    string testPath = Path.Combine(artFolderCheck, $"{plusImageName}_v01.png").Replace("\\", "/");

                    if (File.Exists(testPath))
                    {
                        // Use dedicated _plus images
                        imageBaseName = plusImageName;
                    }
                    else
                    {
                        // Fall back to base card images (strip _Plus)
                        imageBaseName = strippedName.ToLower();
                    }
                }
                else
                {
                    imageBaseName = assetName.ToLower();
                }
            }
            else
            {
                // Try to determine from parent folder
                string parentFolder = Path.GetDirectoryName(assetPath);
                string parentFolderName = Path.GetFileName(parentFolder);

                if (parentFolderName == "Kira" || parentFolderName == "Elara" ||
                    parentFolderName == "Mordren" || parentFolderName == "Thornwick")
                {
                    folder = parentFolderName;

                    // Handle Plus/Upgraded cards
                    if (assetName.EndsWith("_Plus"))
                    {
                        string strippedName2 = assetName.Substring(0, assetName.Length - 5);
                        string plusImageName2 = strippedName2.ToLower() + "_plus";

                        // Check if dedicated _plus images exist
                        string artFolderCheck2 = Path.Combine(CARD_ART_PATH, folder);
                        string testPath2 = Path.Combine(artFolderCheck2, $"{plusImageName2}_v01.png").Replace("\\", "/");

                        if (File.Exists(testPath2))
                        {
                            imageBaseName = plusImageName2;
                        }
                        else
                        {
                            imageBaseName = strippedName2.ToLower();
                        }
                    }
                    else
                    {
                        imageBaseName = assetName.ToLower();
                    }
                }
                else if (parentFolderName == "Shared")
                {
                    folder = "Shared";
                    imageBaseName = assetName.ToLower();
                }
                else
                {
                    // Fallback: try Base folder
                    folder = "Base";
                    imageBaseName = "base_" + assetName.ToLower();
                }
            }

            // Search for v01-v04 variants
            string artFolderPath = Path.Combine(CARD_ART_PATH, folder);

            for (int v = 1; v <= 4; v++)
            {
                string variantName = $"{imageBaseName}_v{v:D2}.png";
                string fullPath = Path.Combine(artFolderPath, variantName).Replace("\\", "/");

                if (File.Exists(fullPath))
                {
                    variants.Add(fullPath);
                }
            }

            // If no variants found with standard naming, try alternative patterns
            if (variants.Count == 0)
            {
                variants = TryAlternativePatterns(assetName, folder, imageBaseName);
            }

            return variants;
        }

        /// <summary>
        /// Checks if the card is a base card with special naming.
        /// </summary>
        private static bool TryGetBaseCardMapping(string assetName, out string baseName)
        {
            return BaseCardMappings.TryGetValue(assetName, out baseName);
        }

        /// <summary>
        /// Checks if the card is a Plus card with special naming (e.g., Kira_Strike_Plus -> kira_basic_strike).
        /// </summary>
        private static bool TryGetPlusCardMapping(string assetName, out string imageBaseName, out string folder)
        {
            if (PlusCardMappings.TryGetValue(assetName, out imageBaseName))
            {
                // Determine folder from the character prefix in the image base name
                if (imageBaseName.StartsWith("kira_")) folder = "Kira";
                else if (imageBaseName.StartsWith("elara_")) folder = "Elara";
                else if (imageBaseName.StartsWith("mordren_")) folder = "Mordren";
                else if (imageBaseName.StartsWith("thornwick_")) folder = "Thornwick";
                else folder = "Shared";

                return true;
            }

            imageBaseName = null;
            folder = null;
            return false;
        }

        /// <summary>
        /// Tries alternative naming patterns for edge cases.
        /// </summary>
        private static List<string> TryAlternativePatterns(string assetName, string folder, string imageBaseName)
        {
            var variants = new List<string>();

            // Try without character prefix for character cards
            string[] characters = { "kira_", "elara_", "mordren_", "thornwick_" };
            foreach (string charPrefix in characters)
            {
                if (imageBaseName.StartsWith(charPrefix))
                {
                    string withoutPrefix = imageBaseName.Substring(charPrefix.Length);
                    string artFolderPath = Path.Combine(CARD_ART_PATH, folder);

                    for (int v = 1; v <= 4; v++)
                    {
                        string variantName = $"{charPrefix}{withoutPrefix}_v{v:D2}.png";
                        string fullPath = Path.Combine(artFolderPath, variantName).Replace("\\", "/");

                        if (File.Exists(fullPath))
                        {
                            variants.Add(fullPath);
                        }
                    }

                    if (variants.Count > 0) break;
                }
            }

            return variants;
        }

        /// <summary>
        /// Preview which cards would be linked without making changes.
        /// </summary>
        public static void PreviewLinking()
        {
            var guidList = new List<string>();
            guidList.AddRange(AssetDatabase.FindAssets("t:CardDataSO", new[] { CARD_DATA_PATH }));
            guidList.AddRange(AssetDatabase.FindAssets("t:CardDataSO", new[] { CARD_RESOURCES_PATH }));
            string[] guids = guidList.ToArray();

            var found = new List<string>();
            var notFound = new List<string>();

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string assetName = Path.GetFileNameWithoutExtension(assetPath);

                var variants = FindArtVariants(assetName, assetPath);

                if (variants.Count > 0)
                {
                    found.Add($"{assetName} ({variants.Count} variants)");
                }
                else
                {
                    notFound.Add(assetName);
                }
            }

            Debug.Log($"[CardArtLinker] Preview - Found art for {found.Count} cards:\n• {string.Join("\n• ", found)}");

            if (notFound.Count > 0)
            {
                Debug.LogWarning($"[CardArtLinker] Preview - Missing art for {notFound.Count} cards:\n• {string.Join("\n• ", notFound)}");
            }

            EditorUtility.DisplayDialog("Card Art Linker Preview",
                $"Found art for {found.Count} cards\nMissing art for {notFound.Count} cards\n\nSee Console for details.",
                "OK");
        }

        /// <summary>
        /// Clears all card art assignments (for testing).
        /// </summary>
        public static void ClearAllCardArt()
        {
            if (!EditorUtility.DisplayDialog("Clear Card Art",
                "This will remove all card art assignments from CardDataSO assets.\n\nContinue?",
                "Clear All", "Cancel"))
            {
                return;
            }

            var guidList = new List<string>();
            guidList.AddRange(AssetDatabase.FindAssets("t:CardDataSO", new[] { CARD_DATA_PATH }));
            guidList.AddRange(AssetDatabase.FindAssets("t:CardDataSO", new[] { CARD_RESOURCES_PATH }));
            string[] guids = guidList.ToArray();
            int cleared = 0;

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var cardData = AssetDatabase.LoadAssetAtPath<CardDataSO>(assetPath);
                if (cardData == null) continue;

                var so = new SerializedObject(cardData);
                var artProperty = so.FindProperty("_cardArt");

                if (artProperty.objectReferenceValue != null)
                {
                    artProperty.objectReferenceValue = null;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(cardData);
                    cleared++;
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[CardArtLinker] Cleared art from {cleared} cards");
            EditorUtility.DisplayDialog("Card Art Cleared", $"Cleared art from {cleared} cards.", "OK");
        }
    }
}
