using System.Collections.Generic;
using UnityEngine;

namespace TuTien.UI.ViewModels
{
    /// <summary>
    /// Mock Online Service Provider cung cấp Fixture Data hoàn chỉnh cho ViewModels.
    /// Giúp toàn bộ UI kiểm thử mượt mà và sẵn sàng 100% để chuyển sang UGS Backend sau này.
    /// </summary>
    public class MockOnlineServiceProvider : MonoBehaviour
    {
        public static MockOnlineServiceProvider Instance { get; private set; }

        public PlayerHeaderVM CurrentPlayerHeader { get; private set; }
        public WalletVM CurrentWallet { get; private set; }
        public LeaderboardVM PowerLeaderboard { get; private set; }
        public LeaderboardVM CultivationLeaderboard { get; private set; }
        public AuthVM CurrentAuth { get; private set; }
        public AppStatusVM CurrentAppStatus { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            InitializeMockData();
        }

        private void InitializeMockData()
        {
            // 1. Mock Player Header
            CurrentPlayerHeader = new PlayerHeaderVM
            {
                displayName = "Hàn Lập",
                realmDisplayName = "Trúc Cơ",
                subLevel = 2,
                cultivationCurrent = 42500f,
                cultivationRequired = 100000f,
                formattedPower = "88,640",
                syncStatus = "Đã đồng bộ máy chủ",
                isSyncing = false
            };

            // 2. Mock Wallet
            CurrentWallet = new WalletVM
            {
                linhThachFormatted = "258,400",
                coDuyenFormatted = "350",
                oreFormatted = "85",
                isPending = false
            };

            // 3. Mock Leaderboard: Chiến Lực
            PowerLeaderboard = new LeaderboardVM
            {
                boardType = "Power",
                seasonLabel = "Tu Tiên Tiên Lộ - Tuần 1",
                loadState = "Ready",
                updatedAt = "Vừa cập nhật",
                ownEntry = new LeaderboardEntryVM { rank = 18, characterName = "Hàn Lập", realmText = "Trúc Cơ Trung Kỳ", scoreFormatted = "88,640", isOwn = true },
                entries = new List<LeaderboardEntryVM>
                {
                    new LeaderboardEntryVM { rank = 1, characterName = "Lệ Phi Vũ", realmText = "Kết Đan Hậu Kỳ", scoreFormatted = "450,200" },
                    new LeaderboardEntryVM { rank = 2, characterName = "Nam Cung Uyển", realmText = "Kết Đan Sơ Kỳ", scoreFormatted = "385,150" },
                    new LeaderboardEntryVM { rank = 3, characterName = "Cực Âm Tổ Sư", realmText = "Trúc Cơ Đỉnh Phong", scoreFormatted = "290,000" },
                    new LeaderboardEntryVM { rank = 4, characterName = "Vương Lục", realmText = "Trúc Cơ Hậu Kỳ", scoreFormatted = "215,800" },
                    new LeaderboardEntryVM { rank = 5, characterName = "Bạch Tiểu Thuần", realmText = "Trúc Cơ Trung Kỳ", scoreFormatted = "180,450" }
                }
            };

            // 4. Mock Leaderboard: Tu Luyện
            CultivationLeaderboard = new LeaderboardVM
            {
                boardType = "Cultivation",
                seasonLabel = "Cảnh Giới Tiên Giới",
                loadState = "Ready",
                updatedAt = "Vừa cập nhật",
                ownEntry = new LeaderboardEntryVM { rank = 22, characterName = "Hàn Lập", realmText = "Trúc Cơ Trung Kỳ (42%)", scoreFormatted = "Cấp 12", isOwn = true },
                entries = new List<LeaderboardEntryVM>
                {
                    new LeaderboardEntryVM { rank = 1, characterName = "Lệ Phi Vũ", realmText = "Kết Đan Hậu Kỳ", scoreFormatted = "Cấp 23" },
                    new LeaderboardEntryVM { rank = 2, characterName = "Nam Cung Uyển", realmText = "Kết Đan Sơ Kỳ", scoreFormatted = "Cấp 21" },
                    new LeaderboardEntryVM { rank = 3, characterName = "Cực Âm Tổ Sư", realmText = "Trúc Cơ Đỉnh Phong", scoreFormatted = "Cấp 20" }
                }
            };

            // 5. Mock Auth
            CurrentAuth = new AuthVM
            {
                isLoggedIn = true,
                isGuest = true,
                accountId = "guest_847192",
                username = "Hàn Lập (Khách)"
            };

            // 6. Mock App Status
            CurrentAppStatus = new AppStatusVM
            {
                onlineState = "Online",
                requiredVersion = "1.0.0"
            };
        }
    }
}

