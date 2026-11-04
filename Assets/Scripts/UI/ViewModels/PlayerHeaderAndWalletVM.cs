using System;

namespace TuTien.UI.ViewModels
{
    /// <summary>
    /// ViewModel thông tin người chơi trên Header/HUD theo mục 6 Brief UI.
    /// </summary>
    [Serializable]
    public class PlayerHeaderVM
    {
        public string displayName = "Đạo Hữu";
        public string realmDisplayName = "Luyện Khí";
        public int subLevel = 1; // 1: Sơ Kỳ, 2: Trung Kỳ, 3: Hậu Kỳ, 4: Đỉnh Phong
        public float cultivationCurrent = 0f;
        public float cultivationRequired = 100f;
        public float CultivationRatio => cultivationRequired > 0 ? Math.Clamp(cultivationCurrent / cultivationRequired, 0f, 1f) : 0f;
        public string formattedPower = "1,250";
        public string syncStatus = "Đã đồng bộ";
        public bool isSyncing = false;
    }

    /// <summary>
    /// ViewModel số dư tài nguyên người chơi (Linh Thạch, Cơ Duyên, Quặng).
    /// Tuyệt đối không cho phép View tự gán (setter) số dư để đảm bảo an toàn.
    /// </summary>
    [Serializable]
    public class WalletVM
    {
        public string linhThachFormatted = "0";
        public string coDuyenFormatted = "0";
        public string oreFormatted = "0";
        public bool isPending = false;
        public string pendingMessage = "";
    }
}

