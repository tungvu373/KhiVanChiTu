using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panel hiển thị chi tiết toàn bộ thông tin nhân vật:
/// Tên, Cảnh giới, Tu vi, HP, MP, Khí tức tiên thiên, Phi kiếm xuất trận,
/// và các chỉ số công pháp nâng cấp cốt lõi: Kiếm Ý, Linh Lực, Tụ Linh, Thần Thức.
/// </summary>
public class CharacterDetailModalUI : MonoBehaviour
{
    private static CharacterDetailModalUI _instance;
    public static CharacterDetailModalUI Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<CharacterDetailModalUI>(true);
            }
            return _instance;
        }
    }

    [Header("Cấu trúc Panel")]
    public CanvasGroup canvasGroup;
    public RectTransform modalBox;
    public ScrollRect scrollRect;
    public Button closeButton;
    public Button closeOverlayButton;
    public Button bottomCloseButton;

    [Header("1. Thông Tin Bản Thân")]
    public Text txtName;
    public Text txtRealm;
    public Text txtTuVi;
    public Text txtCoDuyen;
    public Text txtLinhThach;

    [Header("2. Chỉ Số Sinh Tồn & Chiến Đấu")]
    public Text txtHp;
    public Text txtMp;
    public Text txtMultiplier;
    public Text txtEquipSwords;

    [Header("3. Chỉ Số Công Pháp Nâng Cấp (Kiếm Ý, Linh Lực, Tụ Linh, Thần Thức)")]
    public Text txtUpgradeKiemY;
    public Text txtUpgradeLinhLuc;
    public Text txtUpgradeTuLinh;
    public Text txtUpgradeThanThuc;

    private Coroutine animRoutine;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            if (Application.isPlaying)
            {
                Destroy(gameObject);
                return;
            }
        }
        _instance = this;

        if (scrollRect == null)
        {
            scrollRect = GetComponentInChildren<ScrollRect>(true);
        }

        if (closeButton != null) closeButton.onClick.AddListener(ClosePanel);
        if (closeOverlayButton != null) closeOverlayButton.onClick.AddListener(ClosePanel);
        if (bottomCloseButton != null) bottomCloseButton.onClick.AddListener(ClosePanel);
    }

    private void Start()
    {
        // Panel được người dùng tắt sẵn trong Scene, chỉ hiển thị khi click mở
    }

    [ContextMenu("Test Open Panel")]
    public void TestOpen() => OpenPanel();

    [ContextMenu("Test Close Panel")]
    public void TestClose() => ClosePanel();

    public static void Open()
    {
        if (Instance != null)
        {
            Instance.OpenPanel();
        }
    }

    public static void Close()
    {
        if (Instance != null)
        {
            Instance.ClosePanel();
        }
    }

    public void OpenPanel()
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();

        RefreshData();

        Canvas.ForceUpdateCanvases();
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }

        if (animRoutine != null) StopCoroutine(animRoutine);
        animRoutine = StartCoroutine(AnimateOpen());
    }

    public void ClosePanel()
    {
        if (!gameObject.activeSelf) return;

        if (animRoutine != null) StopCoroutine(animRoutine);
        animRoutine = StartCoroutine(AnimateClose());
    }

    public void RefreshData()
    {
        // 1. THÔNG TIN BẢN THÂN
        string charName = "Đạo Hữu";
        if (CultivationManager.Instance != null && !string.IsNullOrEmpty(CultivationManager.Instance.CharacterName))
        {
            charName = CultivationManager.Instance.CharacterName;
        }
        if (txtName != null) txtName.text = $"🏷️ Đạo Hiệu: <color=#FFE666>{charName}</color>";

        if (CultivationManager.Instance != null && CultivationManager.Instance.allStages != null && CultivationManager.Instance.currentStageIndex < CultivationManager.Instance.allStages.Length)
        {
            var stage = CultivationManager.Instance.allStages[CultivationManager.Instance.currentStageIndex];
            float currentTuVi = CultivationManager.Instance.currentTuVi;
            float reqTuVi = stage.requiredTuVi;
            float progress = reqTuVi > 0 ? (currentTuVi / reqTuVi) * 100f : 100f;

            if (txtRealm != null) txtRealm.text = $"🌟 Cảnh Giới: <color=#66FFB2>{stage.stageName}</color>";
            if (txtTuVi != null) txtTuVi.text = $"🔮 Tu Vi: <color=#66CCFF>{Mathf.FloorToInt(currentTuVi):N0} / {reqTuVi:N0}</color> (Tiến độ: <color=#FFE666>{progress:F1}%</color>)";
        }

        if (EconomyManager.Instance != null)
        {
            if (txtCoDuyen != null) txtCoDuyen.text = $"✨ Cơ Duyên: <color=#FFAA33>{EconomyManager.Instance.currentCoDuyen} Điểm</color>";
            if (txtLinhThach != null) txtLinhThach.text = $"💎 Linh Thạch: <color=#66CCFF>{EconomyManager.Instance.currentLinhThach:N0}</color>";
        }

        // 2. CHỈ SỐ SINH TỒN & CHIẾN ĐẤU
        if (CombatManager.Instance != null)
        {
            float hp = CombatManager.Instance.playerCurrentHp;
            float maxHp = CombatManager.Instance.playerMaxHp;
            if (txtHp != null) txtHp.text = $"❤️ Sinh Lực: <color=#55FF55>{Mathf.Ceil(hp):N0} / {maxHp:N0}</color>";
        }

        if (SkillManager.Instance != null)
        {
            float mp = SkillManager.Instance.currentLinhLuc;
            float maxMp = SkillManager.Instance.maxLinhLuc;
            float regen = SkillManager.Instance.linhLucRegenRate;
            if (txtMp != null) txtMp.text = $"🔷 Linh Lực: <color=#66B2FF>{mp:F0} / {maxMp:F0}</color> (+{regen:F1}/s hồi phục)";
        }

        if (CultivationManager.Instance != null)
        {
            float mult = CultivationManager.Instance.permanentStatMultiplier;
            float bonus = (mult - 1f) * 100f;
            if (txtMultiplier != null) txtMultiplier.text = $"⚡ Khí Tức Tiên Thiên: <color=#FFDD44>+{bonus:F1}%</color> (Hệ số x{mult:F2})";
        }

        // 3. PHI KIẾM ĐANG XUẤT TRẬN
        if (MergeManager.Instance != null && MergeManager.Instance.equipPanel != null)
        {
            int equippedCount = 0;
            float totalDamage = 0f;
            for (int i = 0; i < 4; i++)
            {
                if (i < MergeManager.Instance.equipPanel.childCount)
                {
                    Transform slot = MergeManager.Instance.equipPanel.GetChild(i);
                    MergeItem item = slot.GetComponentInChildren<MergeItem>();
                    if (item != null && item.level > 0)
                    {
                        equippedCount++;
                        totalDamage += FlyingSword.CalculateBaseDamage(item.level);
                    }
                }
            }

            if (txtEquipSwords != null)
            {
                txtEquipSwords.text = $"🗡️ Phi Kiếm Đang Xuất Trận: <color=#FFE666>{equippedCount}/4 Thanh</color> (Tổng Căn Cơ: {totalDamage:N0} Dame)";
            }
        }

        // 4. CHỈ SỐ CÔNG PHÁP NÂNG CẤP (Thứ tự: Kiếm Ý, Linh Lực, Tụ Linh, Thần Thức)
        int lvlKY = 1;
        int lvlLL = 1;
        int lvlTL = 1;
        int lvlTT = 1;

        float valKY = 15f;
        float valTL = 15f;
        float valTT = 12f;
        float maxMpVal = 100f;
        float regenMpVal = 10f;

        if (UpgradeManager.Instance != null)
        {
            lvlKY = UpgradeManager.Instance.kiemYLevel;
            lvlLL = UpgradeManager.Instance.linhLucLevel;
            lvlTL = UpgradeManager.Instance.tuLinhLevel;
            lvlTT = UpgradeManager.Instance.thanThucLevel;

            valKY = UpgradeManager.Instance.GetKiemYValue();
            if (valKY <= 0) valKY = 10f + (lvlKY * 5f);

            valTL = UpgradeManager.Instance.GetTuLinhValue();
            if (valTL <= 0) valTL = 10f + (lvlTL * 5f);

            valTT = UpgradeManager.Instance.GetThanThucValue();
            if (valTT <= 0) valTT = 10f + (lvlTT * 2f);
        }

        if (SkillManager.Instance != null)
        {
            maxMpVal = SkillManager.Instance.maxLinhLuc;
            regenMpVal = SkillManager.Instance.linhLucRegenRate;
        }

        // Hiển thị đầy đủ Tên, Cấp và Chỉ số thực tế
        if (txtUpgradeKiemY != null)
            txtUpgradeKiemY.text = $"• 🗡️ <color=#FF8866>Kiếm Ý</color>: Cấp {lvlKY}  ➔  Chỉ số: <color=#FFE666>+{valKY:F1} Sát thương Kiếm</color>";

        if (txtUpgradeLinhLuc != null)
            txtUpgradeLinhLuc.text = $"• 🔷 <color=#66CCFF>Linh Lực</color>: Cấp {lvlLL}  ➔  Chỉ số: <color=#66CCFF>{maxMpVal:F0} MP</color> (+{regenMpVal:F1}/s hồi phục)";

        if (txtUpgradeTuLinh != null)
            txtUpgradeTuLinh.text = $"• ✨ <color=#88FF88>Tụ Linh</color>: Cấp {lvlTL}  ➔  Chỉ số: <color=#88FF88>+{valTL:F1}% Thu nạp Linh Thạch</color>";

        if (txtUpgradeThanThuc != null)
            txtUpgradeThanThuc.text = $"• 👁️ <color=#CC88FF>Thần Thức</color>: Cấp {lvlTT}  ➔  Chỉ số: <color=#CC88FF>+{valTT:F1}% Tốc độ xuất chiêu</color>";
    }

    private IEnumerator AnimateOpen()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (modalBox == null) modalBox = transform.Find("ModalBox") as RectTransform;

        float duration = 0.2f;
        float elapsed = 0f;
        Vector3 baseScale = Vector3.one;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Pop bounce nhẹ nhàng
            float scale = Mathf.Lerp(0.85f, 1.0f, Mathf.Sin(t * Mathf.PI * 0.5f));
            if (modalBox != null) modalBox.localScale = baseScale * scale;
            if (canvasGroup != null) canvasGroup.alpha = t;
            yield return null;
        }

        if (modalBox != null) modalBox.localScale = baseScale;
        if (canvasGroup != null) canvasGroup.alpha = 1f;
        animRoutine = null;
    }

    private IEnumerator AnimateClose()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (modalBox == null) modalBox = transform.Find("ModalBox") as RectTransform;

        float duration = 0.15f;
        float elapsed = 0f;
        Vector3 baseScale = Vector3.one;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float scale = Mathf.Lerp(1.0f, 0.9f, t);
            if (modalBox != null) modalBox.localScale = baseScale * scale;
            if (canvasGroup != null) canvasGroup.alpha = 1f - t;
            yield return null;
        }

        if (canvasGroup != null) canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
        animRoutine = null;
    }
}
