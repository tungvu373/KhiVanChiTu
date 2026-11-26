using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace KhiVanChiTu.Remake.Editor
{
    public sealed class CodexArtImportPostprocessor : AssetPostprocessor
    {
        public const string CanonicalDropRoot = "Assets/_Game/Resources/CodexDrop/";
        public const string LegacyDropRoot = "Assets/_Game/Art/CodexDrop/";
        public const string RegistryAssetPath = "Assets/_Game/Resources/RemakeArtRegistry.asset";

        private static readonly HashSet<string> PendingRelocate = new HashSet<string>();

        private void OnPreprocessTexture()
        {
            string p = assetPath.Replace("\\", "/");
            if (p.StartsWith(LegacyDropRoot, StringComparison.OrdinalIgnoreCase))
            {
                PendingRelocate.Add(p);
                return;
            }

            if (!p.StartsWith(CanonicalDropRoot, StringComparison.OrdinalIgnoreCase))
                return;

            var importer = (TextureImporter)assetImporter;
            string rel = p.Substring(CanonicalDropRoot.Length);
            bool isMapGround = rel.StartsWith("Map/tex_", StringComparison.OrdinalIgnoreCase) ||
                               rel.StartsWith("DaoVan/daovan_bg_canvas", StringComparison.OrdinalIgnoreCase);
            bool isSkyOrBg = rel.StartsWith("Backgrounds/", StringComparison.OrdinalIgnoreCase) ||
                             rel.IndexOf("panorama", StringComparison.OrdinalIgnoreCase) >= 0;
            bool is9SliceOrBigDecal = rel.IndexOf("9slice", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                      rel.IndexOf("dirt_patch", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                      rel.IndexOf("ground_ring", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                      rel.StartsWith("Map/prop_", StringComparison.OrdinalIgnoreCase) ||
                                      (rel.StartsWith("Stages/", StringComparison.OrdinalIgnoreCase) &&
                                       rel.IndexOf("event_fortuitous", StringComparison.OrdinalIgnoreCase) < 0);

            importer.textureType = (isMapGround || isSkyOrBg) ? TextureImporterType.Default : TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            if (rel.Equals("HUD/hud_panel_light_9slice.png", StringComparison.OrdinalIgnoreCase))
                importer.spriteBorder = new Vector4(146, 118, 146, 118);
            else if (rel.Equals("HUD/hud_card_light_9slice.png", StringComparison.OrdinalIgnoreCase))
                importer.spriteBorder = new Vector4(110, 84, 110, 142);
            else if (rel.Equals("HUD/hud_btn_primary_9slice.png", StringComparison.OrdinalIgnoreCase))
                importer.spriteBorder = new Vector4(128, 56, 128, 52);
            else if (rel.Equals("HUD/hud_btn_secondary_9slice.png", StringComparison.OrdinalIgnoreCase))
                importer.spriteBorder = new Vector4(116, 42, 116, 42);
            importer.mipmapEnabled = isMapGround;
            importer.alphaIsTransparency = !p.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase);
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = (rel.StartsWith("Map/tex_grass", StringComparison.OrdinalIgnoreCase) ||
                                 rel.StartsWith("Map/tex_act", StringComparison.OrdinalIgnoreCase) ||
                                 rel.StartsWith("DaoVan/daovan_bg_canvas", StringComparison.OrdinalIgnoreCase))
                ? TextureWrapMode.Repeat
                : TextureWrapMode.Clamp;

            int maxSize = isSkyOrBg ? 2048 : isMapGround ? 1024 : is9SliceOrBigDecal ? 512 : 256;
            importer.maxTextureSize = maxSize;
            importer.textureCompression = TextureImporterCompression.Compressed;
        }

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (PendingRelocate.Count > 0)
            {
                string[] toMove = new string[PendingRelocate.Count];
                PendingRelocate.CopyTo(toMove);
                PendingRelocate.Clear();
                EditorApplication.delayCall += () => MoveLegacyDropsToResources(toMove);
            }

            bool registryDirty = false;
            foreach (string path in importedAssets)
            {
                if (path.Contains("CodexDrop") || path.Contains("GameIcons"))
                {
                    registryDirty = true;
                    break;
                }
            }
            if (!registryDirty)
            {
                foreach (string path in deletedAssets)
                {
                    if (path.Contains("CodexDrop")) { registryDirty = true; break; }
                }
            }

            if (registryDirty || AssetDatabase.LoadAssetAtPath<RemakeArtRegistrySO>(RegistryAssetPath) == null)
            {
                EditorApplication.delayCall += RebuildArtRegistry;
            }
        }

        private static void MoveLegacyDropsToResources(string[] paths)
        {
            foreach (string oldPath in paths)
            {
                if (!File.Exists(oldPath)) continue;
                string rel = oldPath.Substring(LegacyDropRoot.Length);
                string newPath = CanonicalDropRoot + rel;
                string targetDir = Path.GetDirectoryName(newPath)?.Replace("\\", "/");
                if (!string.IsNullOrEmpty(targetDir)) EnsureFolderRecursive(targetDir);
                string error = AssetDatabase.MoveAsset(oldPath, newPath);
                if (string.IsNullOrEmpty(error))
                    Debug.Log($"[CodexArt] Auto-migrated Codex asset to WebGL-safe Resources folder: {newPath}");
            }
            RebuildArtRegistry();
        }

        [MenuItem("KhiVan/Remake/Rebuild Art Registry & Codex Manifest")]
        public static void RebuildArtRegistry()
        {
            EnsureFolderRecursive("Assets/_Game/Resources/CodexDrop");
            var registry = AssetDatabase.LoadAssetAtPath<RemakeArtRegistrySO>(RegistryAssetPath);
            if (registry == null)
            {
                registry = ScriptableObject.CreateInstance<RemakeArtRegistrySO>();
                AssetDatabase.CreateAsset(registry, RegistryAssetPath);
            }

            var reused = new List<RemakeArtRegistrySO.NamedSpriteEntry>();
            CollectSpritesFromFolder("Assets/UI/GameIcons/Art", reused);
            CollectSpritesFromFolder("Assets/UI/GameIcons/RouteArt", reused);
            Sprite portrait = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Menu/Art/CharacterPortrait.png");
            if (portrait != null)
                reused.Add(new RemakeArtRegistrySO.NamedSpriteEntry { key = "CharacterPortrait", sprite = portrait });
            Sprite flyingSwordSheet = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Import/Asset/kiem.png");
            if (flyingSwordSheet != null)
                reused.Add(new RemakeArtRegistrySO.NamedSpriteEntry { key = "FlyingSwordSheet", sprite = flyingSwordSheet });

            registry.SetReusedSprites(reused);
            registry.SetCodexCatalog(BuildCanonicalCodexCatalog());
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();
            CodexArtLoader.InvalidateCaches();
        }

        private static void CollectSpritesFromFolder(string folder, List<RemakeArtRegistrySO.NamedSpriteEntry> list)
        {
            if (!AssetDatabase.IsValidFolder(folder)) return;
            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { folder });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sp != null)
                {
                    string key = Path.GetFileNameWithoutExtension(path);
                    list.Add(new RemakeArtRegistrySO.NamedSpriteEntry { key = key, sprite = sp });
                }
            }
        }

        private static List<RemakeArtRegistrySO.CodexSlotMeta> BuildCanonicalCodexCatalog()
        {
            var items = new List<RemakeArtRegistrySO.CodexSlotMeta>();
            void Add(string code, string key, string name, bool m1) =>
                items.Add(new RemakeArtRegistrySO.CodexSlotMeta { code = code, key = key, displayName = name, isMilestone1Priority = m1 });

            // Nhom A (12)
            Add("A1", "HUD/hud_circle_btn_idle", "Khung ô tròn sáng (Idle)", true);
            Add("A2", "HUD/hud_circle_btn_active", "Khung ô tròn sáng (Active)", true);
            Add("A3", "HUD/hud_circle_btn_locked", "Khung ô tròn khóa", true);
            Add("A4", "HUD/hud_toggle_bagua", "Nút tròn Bát Quái đóng/mở", true);
            Add("A5", "HUD/hud_avatar_frame", "Khung tròn Avatar sáng", true);
            Add("A6", "HUD/hud_panel_light_9slice", "Khung bảng chính 9-slice sáng", true);
            Add("A7", "HUD/hud_card_light_9slice", "Khung thẻ con 9-slice sáng", true);
            Add("A8", "HUD/hud_btn_primary_9slice", "Nút chính 9-slice vàng kim", true);
            Add("A9", "HUD/hud_btn_secondary_9slice", "Nút phụ 9-slice thanh ngọc", true);
            Add("A10a", "HUD/rarity_1_pham", "Khung phẩm chất Phàm", false);
            Add("A10b", "HUD/rarity_2_linh", "Khung phẩm chất Linh", false);
            Add("A10c", "HUD/rarity_3_huyen", "Khung phẩm chất Huyền", false);
            Add("A10d", "HUD/rarity_4_dia", "Khung phẩm chất Địa", false);
            Add("A10e", "HUD/rarity_5_thien", "Khung phẩm chất Thiên", false);
            Add("A11", "Backgrounds/bg_main_menu_daylight", "Nền Main Menu ban ngày", true);
            Add("A12", "Backgrounds/bg_loading_screen", "Nền Loading ban ngày", true);

            // Nhom B (24)
            Add("B1", "Pills/pill_00_waste", "Phế Đan", true);
            Add("B2", "Pills/pill_01_truc_co", "Trúc Cơ Đan", true);
            Add("B3", "Pills/pill_02_ket_kim", "Kết Kim Đan", true);
            Add("B4", "Pills/pill_03_nguyen_anh", "Ngưng Anh Đan", false);
            Add("B5", "Pills/pill_04_hoa_than", "Hóa Thần Đan", false);
            Add("B6", "Pills/pill_05_luyen_hu", "Luyện Hư Đan", false);
            Add("B7", "Pills/pill_06_hop_the", "Hợp Thể Đan", false);
            Add("B8", "Pills/pill_07_dai_thua", "Đại Thừa Đan", false);
            Add("B9", "Pills/pill_08_do_kiep", "Độ Kiếp Tiên Đan", false);
            Add("B10", "Pills/herb_04_long_huyet", "Long Huyết Thảo", false);
            Add("B11", "Pills/herb_05_phuong_hoang_chi", "Phượng Hoàng Chi", false);
            Add("B12", "Pills/herb_06_tien_lo_lien", "Tiên Lộ Thanh Liên", false);
            for (int r = 1; r <= 8; r++)
                Add($"B{12 + r}", $"Pills/alchemist_rank_{r}", $"Huy hiệu Đan Sư {r} Phẩm", r == 1);
            Add("B21", "Pills/flame_1_mortal", "Phàm Hỏa Lò Đan", true);
            Add("B22", "Pills/flame_2_azure_lotus", "Thanh Liên Hỏa", false);
            Add("B23", "Pills/flame_3_violet_core", "Tử Cực Hỏa", false);
            Add("B24", "Pills/flame_4_immortal_gold", "Tiên Thiên Hỏa", false);

            // Nhom C (3)
            Add("C1", "Formations/formation_thunder", "Cửu Thiên Lôi Trận", true);
            Add("C2", "Formations/formation_sword_rain", "Vạn Kiếm Quy Tông Trận", true);
            Add("C3", "Formations/formation_ground_ring_light", "Vòng trận đồ sáng dưới chân", true);

            // Nhom D (10)
            Add("D1", "Equipment/equip_bracer", "Hộ Uyển (Ô #5)", false);
            Add("D2", "Equipment/equip_ring", "Trữ Vật Giới (Ô #6)", false);
            Add("D3", "Equipment/set_thanh_van", "Huy hiệu Bộ Thanh Vân", false);
            Add("D4", "Equipment/set_huyen_vu", "Huy hiệu Bộ Huyền Vũ", false);
            Add("D5", "Equipment/set_dan_tong", "Huy hiệu Bộ Đan Tông", false);
            Add("D6", "Equipment/mat_reforge_stone", "Đá Tẩy Luyện", false);
            Add("D7", "Equipment/mat_forge_talisman", "Phù Rèn Kiếm", false);
            Add("D8", "Equipment/mat_skill_scroll", "Tàn Quyển Công Pháp", false);
            Add("D9", "Equipment/mat_dao_point", "Điểm Đạo Vận", false);
            Add("D10", "Equipment/mat_calm_incense", "Tĩnh Tâm Hương", false);

            // Nhom E (9)
            Add("E1", "DaoVan/daovan_bg_canvas", "Nền bản đồ Cây Đạo Vận", true);
            Add("E2", "DaoVan/daovan_core_node", "Node gốc Đạo Tâm", true);
            Add("E3", "DaoVan/branch_sword", "Nhánh Kiếm Đạo", false);
            Add("E4", "DaoVan/branch_body", "Nhánh Thể Phách", false);
            Add("E5", "DaoVan/branch_cultivation", "Nhánh Tu Luyện", false);
            Add("E6", "DaoVan/branch_wealth", "Nhánh Tài Lộc", false);
            Add("E7", "DaoVan/branch_arts", "Nhánh Bách Nghệ", false);
            Add("E8", "DaoVan/node_frame_unlocked", "Khung node Đạo Vận đã mở", true);
            Add("E9", "DaoVan/node_frame_maxed", "Khung node Đạo Vận tối đa", true);

            // Nhom F (15)
            Add("F1", "Map/tex_grass_meadow_albedo", "Texture cỏ đồng bằng Vực 1", true);
            Add("F2", "Map/tex_dirt_patch_albedo", "Mảng đất vàng cát Vực 1", true);
            Add("F3", "Map/tex_sky_daylight_panorama", "Bầu trời xanh ban ngày 360", true);
            Add("F4", "Map/tex_act2_sand_bright", "Texture Vực 2 Hoang Mạc Sáng", false);
            Add("F5", "Map/tex_act3_jade_cliff", "Texture Vực 3 Tiên Sơn Bạch Ngọc", false);
            Add("F6", "Map/prop_grass_tuft", "Bụi cỏ stylized ban ngày", false);
            Add("F7", "Map/prop_bush_tree_stylized", "Cây / bụi cây stylized ban ngày", false);
            Add("F8", "Monsters/act1_wolf", "Chân dung Thanh Phong Lang", true);
            Add("F9", "Monsters/act1_bird", "Chân dung Linh Vũ Điểu", true);
            Add("F10", "Monsters/act1_turtle", "Chân dung Huyền Giáp Quy", true);
            Add("F11", "Monsters/act1_boss_guardian", "Chân dung Boss Cổ Mộc Yêu Vương", true);
            Add("F12", "Monsters/act2_scorpion", "Chân dung Quái Vực 2", false);
            Add("F13", "Monsters/act2_boss_overlord", "Chân dung Boss Vực 2", false);
            Add("F14", "Monsters/act3_thunder_spirit", "Chân dung Quái Vực 3", false);
            Add("F15", "Monsters/act3_boss_archon", "Chân dung Boss Vực 3", false);

            // Nhom G (6)
            Add("G1", "HUD/nav_leaderboard", "Nút Bảng Xếp Hạng", false);
            Add("G2", "HUD/nav_world_chat", "Nút Chat Thế Giới", false);
            Add("G3", "HUD/nav_friends", "Nút Đạo Hữu / Kết Bạn", false);
            Add("G4", "HUD/nav_skills", "Nút Kỹ Năng Chiến Đấu", true);
            Add("G5", "HUD/nav_map_stage", "Nút Bản Đồ Ải", true);
            Add("G6", "HUD/nav_breakthrough", "Nút Đột Phá Cảnh Giới", true);

            // Nhom H (8 — Sân khấu UI V2, Tiên Môn & Kỳ Ngộ)
            Add("H1", "HUD/nav_immortal_gate", "Nút Tiên Môn (Tháp & Bí Cảnh)", false);
            Add("H2", "Stages/stage_sword_forge", "Sân khấu Đài Rèn Kiếm Lộ V2", false);
            Add("H3", "Stages/stage_alchemy_cauldron", "Sân khấu Đan Lô Cửu Long V2", false);
            Add("H4", "Stages/stage_tribulation_altar", "Sân khấu Độ Kiếp Đài V2", false);
            Add("H5", "Stages/mode_demon_tower", "Minh họa Vạn Yêu Tháp (100 Tầng)", false);
            Add("H6", "Stages/mode_secret_realm", "Minh họa Thái Hư Bí Cảnh", false);
            Add("H7", "Stages/mode_pvp_arena", "Minh họa Luận Kiếm Đài (Coming Soon)", false);
            Add("H8", "Stages/event_fortuitous_orb", "Nút Kỳ Ngộ Linh Cơ", false);

            return items;
        }

        private static void EnsureFolderRecursive(string folderPath)
        {
            string[] parts = folderPath.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
