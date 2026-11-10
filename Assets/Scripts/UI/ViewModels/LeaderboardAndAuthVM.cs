using System;
using System.Collections.Generic;

namespace TuTien.UI.ViewModels
{
    /// <summary>
    /// ViewModel dòng xếp hạng người chơi trong bảng xếp hạng.
    /// </summary>
    [Serializable]
    public class LeaderboardEntryVM
    {
        public int rank = 1;
        public string characterName = "Đạo Hữu";
        public string realmText = "Trúc Cơ Đỉnh Phong";
        public string scoreFormatted = "125,000";
        public bool isOwn = false;
    }

    /// <summary>
    /// ViewModel Bảng Xếp Hạng (Chiến Lực và Tu Luyện).
    /// </summary>
    [Serializable]
    public class LeaderboardVM
    {
        public string boardType = "Power"; // "Power" hoặc "Cultivation"
        public string seasonLabel = "Mùa 1: Tiên Khởi";
        public List<LeaderboardEntryVM> entries = new List<LeaderboardEntryVM>();
        public LeaderboardEntryVM ownEntry = new LeaderboardEntryVM();
        public string eligibleReason = "";
        public string updatedAt = "Vừa xong";
        public string loadState = "Ready"; // "Loading", "Ready", "Error", "Offline"
    }

    /// <summary>
    /// ViewModel xác thực tài khoản (Register / Login / Recovery).
    /// </summary>
    [Serializable]
    public class AuthVM
    {
        public bool isLoggedIn = false;
        public bool isGuest = true;
        public string accountId = "";
        public string username = "";
        public string validationError = "";
        public string requestState = "Idle"; // "Idle", "Submitting", "Success", "Error"
        public string recoveryResult = "";
    }

    /// <summary>
    /// ViewModel trạng thái kết nối và ứng dụng toàn cục.
    /// </summary>
    [Serializable]
    public class AppStatusVM
    {
        public string onlineState = "Online"; // "Online", "Reconnecting", "Offline"
        public string maintenanceMessage = "";
        public string requiredVersion = "1.0.0";
        public bool hasSessionConflict = false;
    }
}

