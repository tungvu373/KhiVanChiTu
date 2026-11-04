using System;
using UnityEngine;

namespace TuTien.UI.ViewModels
{
    /// <summary>
    /// ViewModel đại diện cho một ô kiếm trong Kiếm Lô / Kho Kiếm (Drawer 0).
    /// </summary>
    [Serializable]
    public class SwordSlotVM
    {
        public int slotId = 0;
        public string itemInstanceId = "";
        public Sprite icon = null;
        public int level = 1;
        public bool isLocked = false;
        public string lockReason = "";
        public bool isSelected = false;
        public bool canMerge = false;
        public bool isEquippedInBattle = false;
    }

    /// <summary>
    /// ViewModel cho các thẻ nâng cấp (Luyện Thể, Kỹ năng, Pháp Khí, Trận Pháp).
    /// </summary>
    [Serializable]
    public class UpgradeVM
    {
        public string upgradeId = "";
        public string label = "";
        public string currentEffect = "";
        public string nextEffect = "";
        public string costFormatted = "";
        public bool canExecute = true;
        public string disabledReason = "";
        public string pendingCommandId = "";
    }

    /// <summary>
    /// ViewModel thẻ Luyện Đan (Drawer 2).
    /// </summary>
    [Serializable]
    public class AlchemyVM
    {
        public int recipeIndex = 0;
        public string recipeName = "";
        public string herbsRequiredText = "";
        public float remainingTimeSeconds = 0f;
        public string qualityInfo = "";
        public bool canStart = false;
        public bool canClaim = false;
        public bool isBrewing = false;
    }
}

