using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bộ điều khiển thực thi toàn diện 5 Mini-Game Bí Cảnh Thượng Cổ.
/// Phiên bản nâng cấp đồ họa sắc nét, trực quan, đậm chất Tiên Hiệp Xianxia.
/// Tối ưu Mobile Portrait 9:16, 1 tay chạm cảm ứng, phản hồi xúc giác và hiệu ứng hạt sống động.
/// </summary>
public class SecretRealmPlayController : MonoBehaviour
{
    public static SecretRealmPlayController Instance { get; private set; }

    // UI Root Elements
    private GameObject rootModal;
    private RectTransform arenaContainer;
    private Image arenaBgImage;
    private Text titleText;
    private Text timerText;
    private Text scoreTitleText;
    private Text scoreValueText;
    private Text instructionText;
    private Image mainProgressBar;
    private Text progressBarText;
    private GameObject actionButtonGo;

    // Mini-game State
    private SecretRealmType activeType;
    private float timeRemaining;
    private bool isGameRunning;
    private int currentScore;

    // 1. Horde Survival State
    private int hordeKilled = 0;
    private const int HORDE_TARGET = 50;
    private float nextHordeSpawnTime = 0f;
    private readonly List<GameObject> activeDemonGoList = new List<GameObject>();

    // 2. Barrier Break State
    private float barrierMaxHp = 1000000f;
    private float barrierCurrentHp;
    private RectTransform steleTransform;
    private Image barrierRuneImage;
    private Text barrierStageText;

    // 3. Dan Cauldron State
    private int winningCauldronIndex = 1; // 0: Left, 1: Center, 2: Right
    private bool cauldronChosen = false;
    private readonly List<GameObject> cauldronObjects = new List<GameObject>();
    private readonly List<Text> cauldronStatusTexts = new List<Text>();

    // 4. Lightning Dodge State
    private int orbsCollected = 0;
    private const int ORBS_TARGET = 20;
    private float nextHazardSpawnTime = 0f;
    private readonly List<GameObject> activeThunderOrbs = new List<GameObject>();
    private readonly List<GameObject> activeHazards = new List<GameObject>();
    private RectTransform taijiPondTransform;

    // 5. Sword Tomb State
    private int swordTombAttemptsLeft = 3;
    private int bestSwordPulled = 4;
    private float needleAngle = 0f;
    private float needleSpeed = 3.8f;
    private RectTransform needleTransform;
    private RectTransform swordVisualTransform;
    private Image swordImage;
    private Image swordAuraImage;
    private Text swordFeedbackText;
    private bool needlePaused = false;

    // Result Modal
    private GameObject resultModal;
    private Coroutine resultReveal;

    public Transform ActiveNavigationLayer => resultModal != null && resultModal.activeInHierarchy
        ? resultModal.transform : rootModal != null && rootModal.activeInHierarchy ? rootModal.transform : null;

    // Cached Sprites
    private Sprite spBgHorde;
    private Sprite spBgBarrier;
    private Sprite spBgCauldron;
    private Sprite[] spSwordAtlas;
    private Sprite spHitSpark;
    private Sprite spRuneTelegraph;
    private Sprite spSoftGlow;
    private Sprite spTaiji;
    private Sprite spVanKiem;
    private Sprite spSamSet;
    private Sprite spChuongAn;
    private Sprite spPhapTran;

    public static Font DefaultFont
    {
        get
        {
            var f = UiTextStyle.BodyFont;
            if (f != null) return f;
            f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (f != null) return f;
            return Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        LoadResourceSprites();
    }

    private void LoadResourceSprites()
    {
        spBgHorde = Resources.Load<Sprite>("SecretRealm/bicanh_horde_survival");
        spBgBarrier = Resources.Load<Sprite>("SecretRealm/bicanh_barrier_break");
        spBgCauldron = Resources.Load<Sprite>("SecretRealm/bicanh_dan_cauldron");
        spSwordAtlas = Resources.LoadAll<Sprite>("SecretRealm/kiem");
        spHitSpark = Resources.Load<Sprite>("SecretRealm/Tex_VFX_HitSpark");
        spRuneTelegraph = Resources.Load<Sprite>("SecretRealm/Tex_VFX_RuneTelegraph");
        spSoftGlow = Resources.Load<Sprite>("SecretRealm/Tex_VFX_SoftGlow");
        spTaiji = Resources.Load<Sprite>("SecretRealm/Tex_VFX_Taiji");
        spVanKiem = Resources.Load<Sprite>("SecretRealm/VanKiemQuyTong");
        spSamSet = Resources.Load<Sprite>("SecretRealm/SamSet");
        spChuongAn = Resources.Load<Sprite>("SecretRealm/ChuongAn");
        spPhapTran = Resources.Load<Sprite>("SecretRealm/PhapTran");
    }

    public void LaunchMiniGame(SecretRealmType type)
    {
        StopAllCoroutines();
        LoadResourceSprites();
        activeType = type;
        currentScore = 0;
        isGameRunning = true;

        EnsurePlayUI();
        rootModal.SetActive(true);
        rootModal.transform.SetAsLastSibling();
        if (resultModal != null) resultModal.SetActive(false);

        ClearDynamicArena();

        switch (type)
        {
            case SecretRealmType.HordeSurvival:
                SetupHordeSurvival();
                break;
            case SecretRealmType.BarrierBreak:
                SetupBarrierBreak();
                break;
            case SecretRealmType.DanCauldron:
                SetupDanCauldron();
                break;
            case SecretRealmType.LightningDodge:
                SetupLightningDodge();
                break;
            case SecretRealmType.SwordTomb:
                SetupSwordTomb();
                break;
        }
        var motionOwner=MobileUIAnimationDirector.For(this);
        var presentation=motionOwner!=null?motionOwner.GetComponent<MobileUIReconstruction>():null;
        if(presentation!=null)MobileUIResponsiveRepairs.ApplyTree(presentation,rootModal.transform);
    }

    private void Update()
    {
        if (!isGameRunning) return;

        if (activeType != SecretRealmType.DanCauldron && activeType != SecretRealmType.SwordTomb)
        {
            timeRemaining -= Time.deltaTime;
            if (timerText != null)
            {
                timerText.text = $"⏱️ <color=#FFE666>{Mathf.Max(0f, timeRemaining):0.0}s</color>";
            }
        }

        switch (activeType)
        {
            case SecretRealmType.HordeSurvival:
                UpdateHordeSurvival();
                break;
            case SecretRealmType.BarrierBreak:
                UpdateBarrierBreak();
                break;
            case SecretRealmType.DanCauldron:
                // Tự động kết thúc khi người chơi bấm chọn
                break;
            case SecretRealmType.LightningDodge:
                UpdateLightningDodge();
                break;
            case SecretRealmType.SwordTomb:
                UpdateSwordTomb();
                break;
        }

        if (timeRemaining <= 0f && activeType != SecretRealmType.DanCauldron && activeType != SecretRealmType.SwordTomb)
        {
            EndMiniGame();
        }
    }

    #region 1. VẠN MA TRIỀU DÂNG (HORDE SURVIVAL)
    private void SetupHordeSurvival()
    {
        timeRemaining = 30f;
        hordeKilled = 0;
        nextHordeSpawnTime = 0f;
        ClearGameObjects(activeDemonGoList);

        SetHeader(
            "👹 VẠN MA TRIỀU DÂNG (SINH TỒN 30S)",
            new Color(0.95f, 0.25f, 0.3f),
            "🎯 MỤC TIÊU: Trảm diệt 50 Yêu Ma trước khi cạn kiệt thời gian!",
            "👉 CÁCH CHƠI: Chạm trực tiếp vào Yêu Ma hoặc bấm nút [Vạn Kiếm Quy Tông] bên dưới để quét sạch!"
        );

        if (arenaBgImage != null && spBgHorde != null)
        {
            arenaBgImage.sprite = spBgHorde;
            arenaBgImage.color = new Color(0.9f, 0.85f, 0.85f, 0.95f);
        }

        scoreTitleText.text = "⚔️ YÊU MA ĐÃ TRẢM:";
        UpdateHordeScoreUI();

        CreateBottomActionButton("⚔️ VẠN KIẾM QUY TÔNG (QUÉT SẠCH 5 QUÁI)", spVanKiem, new Color(0.75f, 0.15f, 0.25f), () =>
        {
            TriggerHordeSuperSlash();
        });

        for (int i = 0; i < 6; i++) SpawnHordeDemon();
    }

    private void UpdateHordeSurvival()
    {
        if (Time.time >= nextHordeSpawnTime && activeDemonGoList.Count < 12 && (hordeKilled + activeDemonGoList.Count) < HORDE_TARGET)
        {
            nextHordeSpawnTime = Time.time + 0.45f;
            SpawnHordeDemon();
        }

        if (activeDemonGoList.Count > 0 && UnityEngine.Random.value < 0.12f)
        {
            KillOneHordeDemon(true);
        }
    }

    private void SpawnHordeDemon()
    {
        if (arenaContainer == null) return;

        var go = new GameObject("DemonBadge_" + activeDemonGoList.Count, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        go.transform.SetParent(arenaContainer, false);
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(64f, 64f);

        float rx = UnityEngine.Random.Range(-155f, 155f);
        float ry = UnityEngine.Random.Range(-175f, 155f);
        rt.anchoredPosition = new Vector2(rx, ry);

        var img = go.GetComponent<Image>();
        img.sprite = spSoftGlow;
        img.color = new Color(0.95f, 0.15f, 0.2f, 0.95f);

        var ol = go.GetComponent<Outline>();
        ol.effectColor = new Color(1f, 0.9f, 0.3f, 0.95f);
        ol.effectDistance = new Vector2(2f, -2f);

        // Biểu tượng mặt quỷ ở giữa
        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Text));
        iconGo.transform.SetParent(go.transform, false);
        var irt = iconGo.GetComponent<RectTransform>();
        irt.anchorMin = Vector2.zero;
        irt.anchorMax = Vector2.one;
        irt.offsetMin = irt.offsetMax = Vector2.zero;
        var itxt = iconGo.GetComponent<Text>();
        itxt.font = DefaultFont;
        itxt.fontSize = 28;
        itxt.alignment = TextAnchor.MiddleCenter;
        itxt.color = Color.white;
        string[] demonIcons = { "👹", "👺", "💀", "👿" };
        itxt.text = demonIcons[UnityEngine.Random.Range(0, demonIcons.Length)];

        // Tên quái
        var nameGo = new GameObject("Name", typeof(RectTransform), typeof(Text));
        nameGo.transform.SetParent(go.transform, false);
        var nrt = nameGo.GetComponent<RectTransform>();
        nrt.anchoredPosition = new Vector2(0f, -40f);
        nrt.sizeDelta = new Vector2(76f, 18f);
        var ntxt = nameGo.GetComponent<Text>();
        ntxt.font = DefaultFont;
        ntxt.fontSize = 11;
        ntxt.fontStyle = FontStyle.Bold;
        ntxt.alignment = TextAnchor.MiddleCenter;
        ntxt.color = new Color(1f, 0.85f, 0.7f);
        ntxt.text = "Huyết Ma";

        var btn = go.GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            if (activeDemonGoList.Contains(go))
            {
                SpawnFloatingText("-150.000 (Trảm!)", rt.anchoredPosition, new Color(1f, 0.4f, 0.2f), 1.2f);
                SpawnHitSpark(rt.anchoredPosition, Color.yellow, 64f);
                activeDemonGoList.Remove(go);
                Destroy(go);
                hordeKilled++;
                currentScore = hordeKilled;
                UpdateHordeScoreUI();
                ShakeArena(0.12f, 4f);
                if (hordeKilled >= HORDE_TARGET) EndMiniGame();
            }
        });

        activeDemonGoList.Add(go);
    }

    private void TriggerHordeSuperSlash()
    {
        int kills = Mathf.Min(activeDemonGoList.Count, 5);
        if (kills == 0) return;

        ShakeArena(0.25f, 8f);
        SpawnHitSpark(Vector2.zero, new Color(1f, 0.9f, 0.2f), 160f);

        for (int i = 0; i < kills; i++)
        {
            KillOneHordeDemon(false);
        }
    }

    private void KillOneHordeDemon(bool isIdleDps)
    {
        if (activeDemonGoList.Count == 0) return;
        var go = activeDemonGoList[0];
        activeDemonGoList.RemoveAt(0);
        if (go != null)
        {
            var rt = go.GetComponent<RectTransform>();
            Vector2 p = rt != null ? rt.anchoredPosition : Vector2.zero;
            string txt = isIdleDps ? "-50.000" : "-999.999 (Vạn Kiếm!)";
            Color col = isIdleDps ? Color.white : new Color(1f, 0.85f, 0.2f);
            SpawnFloatingText(txt, p, col, isIdleDps ? 1f : 1.3f);
            SpawnHitSpark(p, Color.red, 52f);
            Destroy(go);
        }

        hordeKilled++;
        currentScore = hordeKilled;
        UpdateHordeScoreUI();
        if (hordeKilled >= HORDE_TARGET) EndMiniGame();
    }

    private void UpdateHordeScoreUI()
    {
        if (scoreValueText != null)
            scoreValueText.text = $"{hordeKilled} / {HORDE_TARGET}";

        float progress = Mathf.Clamp01((float)hordeKilled / HORDE_TARGET);
        if (mainProgressBar != null) mainProgressBar.fillAmount = progress;
        if (progressBarText != null) progressBarText.text = $"TIẾN ĐỘ TRẢM MA: {Mathf.RoundToInt(progress * 100f)}%";
    }
    #endregion

    #region 2. TRẢM PHÁ CẤM CHẾ (BARRIER BREAK)
    private void SetupBarrierBreak()
    {
        timeRemaining = 20f;
        barrierCurrentHp = barrierMaxHp;
        currentScore = 0;

        SetHeader(
            "🗿 TRẢM PHÁ CẤM CHẾ (DPS CHECK 20S)",
            new Color(0.2f, 0.75f, 1f),
            "🎯 MỤC TIÊU: Dồn sát thương phá nát Thạch Bi Thượng Cổ trước khi cấm chế kích hoạt!",
            "👉 CÁCH CHƠI: Chạm liên tục vào nút [Khai Thiên Trảm] bên dưới để xuất kiếm phá tan cấm chế!"
        );

        if (arenaBgImage != null && spBgBarrier != null)
        {
            arenaBgImage.sprite = spBgBarrier;
            arenaBgImage.color = new Color(0.85f, 0.9f, 1f, 0.95f);
        }

        scoreTitleText.text = "🗿 TIẾN ĐỘ PHÁ CẤM:";
        scoreValueText.text = "0%";

        CreateAncientMonolithStele();

        Sprite attackIcon = spChuongAn != null ? spChuongAn : (spSwordAtlas != null && spSwordAtlas.Length > 7 ? spSwordAtlas[7] : null);
        CreateBottomActionButton("⚡ KHAI THIÊN TRẢM! (+50.000 DMG)", attackIcon, new Color(0.15f, 0.45f, 0.85f), () =>
        {
            HitBarrier(50000f, true);
        });

        UpdateBarrierScoreUI();
    }

    private void CreateAncientMonolithStele()
    {
        if (arenaContainer == null) return;

        var steleGo = new GameObject("AncientStele", typeof(RectTransform), typeof(Image), typeof(Outline));
        steleGo.transform.SetParent(arenaContainer, false);
        steleTransform = steleGo.GetComponent<RectTransform>();
        steleTransform.anchoredPosition = new Vector2(0f, 20f);
        steleTransform.sizeDelta = new Vector2(240f, 320f);

        var img = steleGo.GetComponent<Image>();
        img.color = new Color(0.14f, 0.17f, 0.25f, 0.98f);

        var ol = steleGo.GetComponent<Outline>();
        ol.effectColor = new Color(0.3f, 0.8f, 1f, 0.9f);
        ol.effectDistance = new Vector2(3f, -3f);

        var runeGo = new GameObject("RuneSeal", typeof(RectTransform), typeof(Image));
        runeGo.transform.SetParent(steleGo.transform, false);
        var rrt = runeGo.GetComponent<RectTransform>();
        rrt.anchoredPosition = new Vector2(0f, 30f);
        rrt.sizeDelta = new Vector2(170f, 170f);
        barrierRuneImage = runeGo.GetComponent<Image>();
        barrierRuneImage.sprite = spRuneTelegraph != null ? spRuneTelegraph : spTaiji;
        barrierRuneImage.color = new Color(0.2f, 0.85f, 1f, 0.85f);

        var titleGo = new GameObject("SteleTitle", typeof(RectTransform), typeof(Text));
        titleGo.transform.SetParent(steleGo.transform, false);
        var trt = titleGo.GetComponent<RectTransform>();
        trt.anchoredPosition = new Vector2(0f, 125f);
        trt.sizeDelta = new Vector2(220f, 30f);
        var ttxt = titleGo.GetComponent<Text>();
        ttxt.font = DefaultFont;
        ttxt.fontSize = 15;
        ttxt.fontStyle = FontStyle.Bold;
        ttxt.alignment = TextAnchor.MiddleCenter;
        ttxt.color = new Color(1f, 0.85f, 0.3f);
        ttxt.text = "THƯỢNG CỔ CẤM CHẾ";

        var stageGo = new GameObject("StageText", typeof(RectTransform), typeof(Text));
        stageGo.transform.SetParent(steleGo.transform, false);
        var srt = stageGo.GetComponent<RectTransform>();
        srt.anchoredPosition = new Vector2(0f, -100f);
        srt.sizeDelta = new Vector2(220f, 32f);
        barrierStageText = stageGo.GetComponent<Text>();
        barrierStageText.font = DefaultFont;
        barrierStageText.fontSize = 13;
        barrierStageText.fontStyle = FontStyle.Bold;
        barrierStageText.alignment = TextAnchor.MiddleCenter;
        barrierStageText.color = Color.white;
        barrierStageText.text = "Giai Đoạn 1: Phù Ấn Kiên Cố";
    }

    private void UpdateBarrierBreak()
    {
        if (barrierRuneImage != null)
        {
            barrierRuneImage.rectTransform.Rotate(0f, 0f, -25f * Time.deltaTime);
        }

        float idleDps = 15000f * Time.deltaTime;
        HitBarrier(idleDps, false);
    }

    private void HitBarrier(float damage, bool isTap)
    {
        barrierCurrentHp = Mathf.Max(0f, barrierCurrentHp - damage);
        float brokenPercent = (1f - (barrierCurrentHp / barrierMaxHp)) * 100f;
        currentScore = Mathf.RoundToInt(brokenPercent);

        if (isTap)
        {
            ShakeArena(0.08f, 5f);
            Vector2 sparkPos = new Vector2(UnityEngine.Random.Range(-50f, 50f), UnityEngine.Random.Range(-20f, 60f));
            SpawnHitSpark(sparkPos, new Color(0.3f, 0.85f, 1f), 60f);
            SpawnFloatingText("+50.000 BẠO KÍCH!", sparkPos + new Vector2(0f, 20f), new Color(1f, 0.85f, 0.2f), 1.25f);
        }

        UpdateBarrierScoreUI();

        if (barrierCurrentHp <= 0f)
        {
            EndMiniGame();
        }
    }

    private void UpdateBarrierScoreUI()
    {
        float ratio = barrierCurrentHp / barrierMaxHp;
        if (scoreValueText != null) scoreValueText.text = $"{currentScore}%";

        if (mainProgressBar != null) mainProgressBar.fillAmount = (1f - ratio);
        if (progressBarText != null) progressBarText.text = $"TIẾN ĐỘ PHÁ CẤM: {currentScore}%";

        if (barrierRuneImage != null && barrierStageText != null)
        {
            if (ratio > 0.66f)
            {
                barrierRuneImage.color = new Color(0.2f, 0.85f, 1f, 0.85f);
                barrierStageText.text = "Giai Đoạn 1: Phù Ấn Kiên Cố (100%)";
                barrierStageText.color = Color.cyan;
            }
            else if (ratio > 0.33f)
            {
                barrierRuneImage.color = new Color(1f, 0.7f, 0.1f, 0.95f);
                barrierStageText.text = "Giai Đoạn 2: Cấm Chế Rạn Nứt! (50%)";
                barrierStageText.color = new Color(1f, 0.8f, 0.2f);
            }
            else
            {
                barrierRuneImage.color = new Color(1f, 0.25f, 0.2f, 1f);
                barrierStageText.text = "Giai Đoạn 3: CẤM CHẾ SẮP SỤP ĐỔ!";
                barrierStageText.color = new Color(1f, 0.3f, 0.3f);
            }
        }
    }
    #endregion

    #region 3. ĐAN THẦN KỲ DUYÊN (TAM ĐỈNH TẦM BẢO)
    private void SetupDanCauldron()
    {
        timeRemaining = 60f;
        cauldronChosen = false;
        winningCauldronIndex = UnityEngine.Random.Range(0, 3);
        cauldronObjects.Clear();
        cauldronStatusTexts.Clear();

        if (actionButtonGo != null) actionButtonGo.SetActive(false);

        string[] riddles = {
            "📜 Sấm Ngôn Thần Nông: 'Tử khí đông lai tụ phương TẢ, Đỉnh Tử Kim đang sôi trào đan dược quý!'",
            "📜 Sấm Ngôn Lão Quân: 'Âm Dương giao thái tại TRUNG TÂM, Lò Bát Quái ẩn chứa Cực Phẩm Kim Đan!'",
            "📜 Sấm Ngôn Dược Vương: 'Mộc linh hưng thịnh phương HỮU, Đỉnh Thanh Ngọc tỏa hương nghịch thiên!'"
        };

        SetHeader(
            "🔮 ĐAN THẦN KỲ DUYÊN (TAM ĐỈNH TẦM BẢO)",
            new Color(0.9f, 0.75f, 0.15f),
            riddles[winningCauldronIndex],
            "👉 CÁCH CHƠI: Đọc kỹ Sấm Ngôn chỉ dẫn ở trên và chạm vào Đỉnh tương ứng để mở nắp nổ hũ!"
        );

        if (arenaBgImage != null && spBgCauldron != null)
        {
            arenaBgImage.sprite = spBgCauldron;
            arenaBgImage.color = new Color(0.95f, 0.9f, 0.85f, 0.95f);
        }

        scoreTitleText.text = "🔮 TRẠNG THÁI:";
        scoreValueText.text = "Đang Chờ Khai Đỉnh";

        if (mainProgressBar != null) mainProgressBar.fillAmount = 1f;
        if (progressBarText != null) progressBarText.text = "LỰA CHỌN ĐÚNG ĐỈNH ĐỂ NHẬN CỰC PHẨM ĐAN";

        string[] cNames = { "TỬ KIM ĐỈNH\n(Phương Tả)", "BÁT QUÁI ĐỈNH\n(Trung Tâm)", "THANH NGỌC ĐỈNH\n(Phương Hữu)" };
        Color[] themeColors = {
            new Color(0.75f, 0.25f, 0.95f),
            new Color(0.95f, 0.7f, 0.15f),
            new Color(0.2f, 0.85f, 0.5f)
        };

        for (int i = 0; i < 3; i++)
        {
            int idx = i;
            float posX = -118f + (i * 118f);
            CreateCauldronPedestal(idx, cNames[i], new Vector2(posX, -25f), themeColors[i]);
        }
    }

    private void CreateCauldronPedestal(int index, string title, Vector2 pos, Color themeColor)
    {
        if (arenaContainer == null) return;

        var cGo = new GameObject("Cauldron_" + index, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        cGo.transform.SetParent(arenaContainer, false);
        var rt = cGo.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(110f, 170f);

        var img = cGo.GetComponent<Image>();
        img.color = new Color(themeColor.r * 0.15f, themeColor.g * 0.15f, themeColor.b * 0.15f, 0.75f);

        var ol = cGo.GetComponent<Outline>();
        ol.effectColor = themeColor;
        ol.effectDistance = new Vector2(2f, -2f);

        // Biểu tượng Đỉnh Đan
        var iconGo = new GameObject("CauldronIcon", typeof(RectTransform), typeof(Image));
        iconGo.transform.SetParent(cGo.transform, false);
        var irt = iconGo.GetComponent<RectTransform>();
        irt.anchoredPosition = new Vector2(0f, 35f);
        irt.sizeDelta = new Vector2(68f, 68f);
        var iconImg = iconGo.GetComponent<Image>();
        iconImg.sprite = (index == 1 && spTaiji != null) ? spTaiji : (spPhapTran != null ? spPhapTran : spSoftGlow);
        iconImg.color = themeColor;

        // Vòng phát sáng đan hương
        var glowGo = new GameObject("GlowAura", typeof(RectTransform), typeof(Image));
        glowGo.transform.SetParent(cGo.transform, false);
        var grt = glowGo.GetComponent<RectTransform>();
        grt.anchoredPosition = new Vector2(0f, 35f);
        grt.sizeDelta = new Vector2(95f, 95f);
        var gImg = glowGo.GetComponent<Image>();
        gImg.sprite = spSoftGlow;
        gImg.color = new Color(themeColor.r, themeColor.g, themeColor.b, 0.35f);

        // Tên Đỉnh
        var txtGo = new GameObject("TitleText", typeof(RectTransform), typeof(Text));
        txtGo.transform.SetParent(cGo.transform, false);
        var trt = txtGo.GetComponent<RectTransform>();
        trt.anchoredPosition = new Vector2(0f, -25f);
        trt.sizeDelta = new Vector2(106f, 36f);
        var txt = txtGo.GetComponent<Text>();
        txt.font = DefaultFont;
        txt.fontSize = 11;
        txt.fontStyle = FontStyle.Bold;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        txt.text = title;
        cauldronStatusTexts.Add(txt);

        // Nút Khai Đỉnh riêng biệt bên dưới
        var btnOpenGo = new GameObject("BtnOpen", typeof(RectTransform), typeof(Image), typeof(Outline));
        btnOpenGo.transform.SetParent(cGo.transform, false);
        var brt = btnOpenGo.GetComponent<RectTransform>();
        brt.anchoredPosition = new Vector2(0f, -60f);
        brt.sizeDelta = new Vector2(94f, 30f);
        btnOpenGo.GetComponent<Image>().color = themeColor;
        var bol = btnOpenGo.GetComponent<Outline>();
        bol.effectColor = Color.black;
        bol.effectDistance = new Vector2(1f, -1f);
        var btxt = CreateText(btnOpenGo.transform, "Txt", "KHAI ĐỈNH", 11, FontStyle.Bold, Vector2.zero, Color.white);
        btxt.rectTransform.sizeDelta = brt.sizeDelta;

        var btn = cGo.GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            OnSelectCauldron(index);
        });

        cauldronObjects.Add(cGo);
    }

    private void OnSelectCauldron(int selectedIdx)
    {
        if (cauldronChosen) return;
        cauldronChosen = true;

        bool isWin = (selectedIdx == winningCauldronIndex);
        currentScore = isWin ? 1 : 0;

        for (int i = 0; i < cauldronObjects.Count; i++)
        {
            var btn = cauldronObjects[i].GetComponent<Button>();
            if (btn != null) btn.interactable = false;

            if (i == winningCauldronIndex)
            {
                cauldronStatusTexts[i].text = "⭐ NỔ HŨ ⭐\nCỰC PHẨM ĐAN!";
                cauldronStatusTexts[i].color = Color.yellow;
                SpawnHitSpark(cauldronObjects[i].GetComponent<RectTransform>().anchoredPosition, Color.yellow, 120f);
            }
            else
            {
                cauldronStatusTexts[i].text = "Thảo Dược\nThượng Phẩm";
                cauldronStatusTexts[i].color = Color.cyan;
            }
        }

        if (scoreValueText != null)
            scoreValueText.text = isWin ? "<color=#FFE666>NỔ HŨ CỰC PHẨM!</color>" : "<color=#80DFFF>THƯỢNG PHẨM THẢO DƯỢC</color>";

        ShakeArena(0.2f, 6f);
        StartCoroutine(DelayEndMiniGame(2.2f));
    }

    private IEnumerator DelayEndMiniGame(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        EndMiniGame();
    }
    #endregion

    #region 4. VƯỢT LÔI TRÌ (LIGHTNING DODGE)
    private void SetupLightningDodge()
    {
        timeRemaining = 25f;
        orbsCollected = 0;
        nextHazardSpawnTime = 0f;
        ClearGameObjects(activeThunderOrbs);
        ClearGameObjects(activeHazards);

        if (actionButtonGo != null) actionButtonGo.SetActive(false);

        SetHeader(
            "⚡ VƯỢT LÔI TRÌ (THU THẬP & NÉ TRẬN 25S)",
            new Color(0.25f, 0.9f, 1f),
            "🎯 MỤC TIÊU: Thu thập đủ 20 Lôi Tinh Cầu trong trận địa Lôi Trì Thượng Cổ!",
            "👉 CÁCH CHƠI: Chạm thật nhanh vào các Lôi Tinh Cầu phát sáng và chú ý né các vòng sấm đỏ!"
        );

        if (arenaBgImage != null)
        {
            arenaBgImage.sprite = spBgBarrier != null ? spBgBarrier : spBgHorde;
            arenaBgImage.color = new Color(0.15f, 0.2f, 0.35f, 0.98f);
        }

        CreateTaijiLightningPond();

        scoreTitleText.text = "⚡ LÔI TINH ĐÃ THU:";
        UpdateLightningScoreUI();

        for (int i = 0; i < 5; i++) SpawnThunderOrb();
    }

    private void CreateTaijiLightningPond()
    {
        if (arenaContainer == null) return;

        var pondGo = new GameObject("TaijiPond", typeof(RectTransform), typeof(Image));
        pondGo.transform.SetParent(arenaContainer, false);
        taijiPondTransform = pondGo.GetComponent<RectTransform>();
        taijiPondTransform.anchoredPosition = new Vector2(0f, 10f);
        taijiPondTransform.sizeDelta = new Vector2(330f, 330f);

        var img = pondGo.GetComponent<Image>();
        img.sprite = spTaiji != null ? spTaiji : spRuneTelegraph;
        img.color = new Color(0.25f, 0.65f, 1f, 0.65f);
    }

    private void UpdateLightningDodge()
    {
        if (taijiPondTransform != null)
        {
            taijiPondTransform.Rotate(0f, 0f, 18f * Time.deltaTime);
        }

        if (activeThunderOrbs.Count < 5 && (orbsCollected + activeThunderOrbs.Count) < ORBS_TARGET)
        {
            SpawnThunderOrb();
        }

        if (Time.time >= nextHazardSpawnTime)
        {
            nextHazardSpawnTime = Time.time + UnityEngine.Random.Range(2.0f, 3.5f);
            StartCoroutine(SpawnHazardTelegraph());
        }
    }

    private void SpawnThunderOrb()
    {
        if (arenaContainer == null) return;

        var go = new GameObject("ThunderOrb_" + activeThunderOrbs.Count, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        go.transform.SetParent(arenaContainer, false);
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(58f, 58f);

        float rx = UnityEngine.Random.Range(-140f, 140f);
        float ry = UnityEngine.Random.Range(-140f, 140f);
        rt.anchoredPosition = new Vector2(rx, ry);

        var img = go.GetComponent<Image>();
        img.sprite = spSoftGlow;
        img.color = new Color(0.2f, 0.88f, 1f, 0.95f);

        var ol = go.GetComponent<Outline>();
        ol.effectColor = Color.white;
        ol.effectDistance = new Vector2(2f, -2f);

        var iconGo = new GameObject("BoltIcon", typeof(RectTransform), typeof(Image));
        iconGo.transform.SetParent(go.transform, false);
        var irt = iconGo.GetComponent<RectTransform>();
        irt.anchorMin = Vector2.zero;
        irt.anchorMax = Vector2.one;
        irt.offsetMin = new Vector2(10f, 10f);
        irt.offsetMax = new Vector2(-10f, -10f);
        var iconImg = iconGo.GetComponent<Image>();
        iconImg.sprite = spSamSet != null ? spSamSet : spHitSpark;
        iconImg.color = Color.white;

        var btn = go.GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            if (activeThunderOrbs.Contains(go))
            {
                activeThunderOrbs.Remove(go);
                SpawnHitSpark(rt.anchoredPosition, Color.cyan, 60f);
                SpawnFloatingText("+1 LÔI TINH!", rt.anchoredPosition, Color.cyan, 1.2f);
                Destroy(go);
                orbsCollected++;
                currentScore = orbsCollected;
                UpdateLightningScoreUI();
                ShakeArena(0.08f, 3f);
                if (orbsCollected >= ORBS_TARGET) EndMiniGame();
            }
        });

        activeThunderOrbs.Add(go);
    }

    private IEnumerator SpawnHazardTelegraph()
    {
        if (arenaContainer == null) yield break;

        var hGo = new GameObject("HazardTelegraph", typeof(RectTransform), typeof(Image));
        hGo.transform.SetParent(arenaContainer, false);
        var hrt = hGo.GetComponent<RectTransform>();
        hrt.sizeDelta = new Vector2(95f, 95f);
        float rx = UnityEngine.Random.Range(-130f, 130f);
        float ry = UnityEngine.Random.Range(-130f, 130f);
        hrt.anchoredPosition = new Vector2(rx, ry);

        var img = hGo.GetComponent<Image>();
        img.sprite = spRuneTelegraph != null ? spRuneTelegraph : spSoftGlow;
        img.color = new Color(1f, 0.2f, 0.2f, 0.75f);

        activeHazards.Add(hGo);

        yield return new WaitForSeconds(1.2f);

        if (hGo != null)
        {
            SpawnHitSpark(hrt.anchoredPosition, Color.yellow, 100f);
            ShakeArena(0.15f, 6f);
            activeHazards.Remove(hGo);
            Destroy(hGo);
        }
    }

    private void UpdateLightningScoreUI()
    {
        if (scoreValueText != null)
            scoreValueText.text = $"{orbsCollected} / {ORBS_TARGET}";

        float progress = Mathf.Clamp01((float)orbsCollected / ORBS_TARGET);
        if (mainProgressBar != null) mainProgressBar.fillAmount = progress;
        if (progressBarText != null) progressBarText.text = $"THU THẬP LÔI TINH: {Mathf.RoundToInt(progress * 100f)}%";
    }
    #endregion

    #region 5. KIẾM CHỦNG CỔ THẦN (SWORD TOMB)
    private void SetupSwordTomb()
    {
        timeRemaining = 45f;
        swordTombAttemptsLeft = 3;
        bestSwordPulled = 4;
        needleAngle = 0f;
        needlePaused = false;

        SetHeader(
            "🗡️ KIẾM CHỦNG CỔ THẦN (RÚT BẢO KIẾM 3 LƯỢT)",
            new Color(1f, 0.85f, 0.2f),
            "🎯 MỤC TIÊU: Canh đúng vạch Hoàng Kim ở giữa để rút được Cổ Thần Kiếm Bậc 8!",
            "👉 CÁCH CHƠI: Quan sát kim chỉ thị dao động, chạm nút [Rút Kiếm Ngay] khi kim vào vùng Hoàng Kim!"
        );

        if (arenaBgImage != null && spBgBarrier != null)
        {
            arenaBgImage.sprite = spBgBarrier;
            arenaBgImage.color = new Color(0.8f, 0.85f, 0.95f, 0.95f);
        }

        scoreTitleText.text = "🗡️ SỐ LƯỢT RÚT CÒN:";
        UpdateSwordTombUI();

        CreateSwordTombVisuals();

        Sprite pullIcon = (spSwordAtlas != null && spSwordAtlas.Length > 7) ? spSwordAtlas[7] : null;
        CreateBottomActionButton("⚔️ RÚT KIẾM NGAY! (CANH VÙNG HOÀNG KIM)", pullIcon, new Color(0.85f, 0.6f, 0.1f), () =>
        {
            PullSwordFromTomb();
        });
    }

    private void CreateSwordTombVisuals()
    {
        if (arenaContainer == null) return;

        // Vòng hào quang kiếm cổ
        var auraGo = new GameObject("SwordAura", typeof(RectTransform), typeof(Image));
        auraGo.transform.SetParent(arenaContainer, false);
        var art = auraGo.GetComponent<RectTransform>();
        art.anchoredPosition = new Vector2(0f, 65f);
        art.sizeDelta = new Vector2(230f, 230f);
        swordAuraImage = auraGo.GetComponent<Image>();
        swordAuraImage.sprite = spSoftGlow;
        swordAuraImage.color = new Color(1f, 0.85f, 0.2f, 0.45f);

        // Hình kiếm lơ lửng trung tâm (Lấy đúng sprite đơn trong Sprite Atlas)
        var swordGo = new GameObject("FloatingSword", typeof(RectTransform), typeof(Image));
        swordGo.transform.SetParent(arenaContainer, false);
        swordVisualTransform = swordGo.GetComponent<RectTransform>();
        swordVisualTransform.anchoredPosition = new Vector2(0f, 65f);
        swordVisualTransform.sizeDelta = new Vector2(90f, 150f);
        swordVisualTransform.localRotation = Quaternion.Euler(0f, 0f, -45f);

        swordImage = swordGo.GetComponent<Image>();
        if (spSwordAtlas != null && spSwordAtlas.Length > 7)
        {
            swordImage.sprite = spSwordAtlas[7]; // Hiển thị Cổ Thần Kiếm Bậc 8
        }
        swordImage.preserveAspect = true;

        // Text thông báo kiếm vừa rút
        var feedGo = new GameObject("SwordFeedback", typeof(RectTransform), typeof(Text));
        feedGo.transform.SetParent(arenaContainer, false);
        var frt = feedGo.GetComponent<RectTransform>();
        frt.anchoredPosition = new Vector2(0f, -42f);
        frt.sizeDelta = new Vector2(340f, 32f);
        swordFeedbackText = feedGo.GetComponent<Text>();
        swordFeedbackText.font = DefaultFont;
        swordFeedbackText.fontSize = 15;
        swordFeedbackText.fontStyle = FontStyle.Bold;
        swordFeedbackText.alignment = TextAnchor.MiddleCenter;
        swordFeedbackText.color = Color.yellow;
        swordFeedbackText.text = "Chuẩn bị... Hãy canh chuẩn vạch Hoàng Kim!";

        // Thanh đo lực phân vùng (Precision Gauge)
        CreatePrecisionGauge();
    }

    private void CreatePrecisionGauge()
    {
        var barBg = new GameObject("GaugeBg", typeof(RectTransform), typeof(Image), typeof(Outline));
        barBg.transform.SetParent(arenaContainer, false);
        var rt = barBg.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0f, -92f);
        rt.sizeDelta = new Vector2(320f, 42f);

        barBg.GetComponent<Image>().color = new Color(0.12f, 0.15f, 0.22f, 0.98f);
        var ol = barBg.GetComponent<Outline>();
        ol.effectColor = new Color(1f, 0.85f, 0.3f, 0.8f);
        ol.effectDistance = new Vector2(2f, -2f);

        // Vùng Hoàng Kim ở trung tâm (Center Sweet Spot)
        var goldZone = new GameObject("GoldSweetSpot", typeof(RectTransform), typeof(Image));
        goldZone.transform.SetParent(barBg.transform, false);
        var gzRt = goldZone.GetComponent<RectTransform>();
        gzRt.anchoredPosition = Vector2.zero;
        gzRt.sizeDelta = new Vector2(48f, 42f);
        goldZone.GetComponent<Image>().color = new Color(1f, 0.82f, 0.15f, 0.95f);

        var gzTxt = CreateText(goldZone.transform, "TxtGZ", "CỰC\nPHẨM", 10, FontStyle.Bold, Vector2.zero, Color.black);
        gzTxt.rectTransform.sizeDelta = gzRt.sizeDelta;

        // Vùng Linh Kiếm Bậc 6 (Hai bên)
        CreateGaugeZone(barBg.transform, "PurpleLeft", new Vector2(-54f, 0f), new Vector2(58f, 42f), new Color(0.65f, 0.25f, 0.85f, 0.85f), "LINH\nKIẾM");
        CreateGaugeZone(barBg.transform, "PurpleRight", new Vector2(54f, 0f), new Vector2(58f, 42f), new Color(0.65f, 0.25f, 0.85f, 0.85f), "LINH\nKIẾM");

        // Vùng Phàm Kiếm (Hai đầu ngoài)
        CreateGaugeZone(barBg.transform, "BlueLeft", new Vector2(-118f, 0f), new Vector2(70f, 42f), new Color(0.2f, 0.5f, 0.75f, 0.7f), "PHÀM\nBẢO");
        CreateGaugeZone(barBg.transform, "BlueRight", new Vector2(118f, 0f), new Vector2(70f, 42f), new Color(0.2f, 0.5f, 0.75f, 0.7f), "PHÀM\nBẢO");

        // Kim đo dao động
        var needle = new GameObject("Needle", typeof(RectTransform), typeof(Image), typeof(Outline));
        needle.transform.SetParent(barBg.transform, false);
        needleTransform = needle.GetComponent<RectTransform>();
        needleTransform.sizeDelta = new Vector2(8f, 54f);
        needle.GetComponent<Image>().color = Color.white;

        var nol = needle.GetComponent<Outline>();
        nol.effectColor = Color.red;
        nol.effectDistance = new Vector2(2f, -2f);
    }

    private GameObject CreateGaugeZone(Transform parent, string name, Vector2 pos, Vector2 size, Color col, string label)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        go.GetComponent<Image>().color = col;

        var txt = CreateText(go.transform, "Txt", label, 10, FontStyle.Bold, Vector2.zero, Color.white);
        txt.rectTransform.sizeDelta = size;
        return go;
    }

    private void UpdateSwordTomb()
    {
        if (needlePaused) return;

        if (swordVisualTransform != null)
        {
            float yOffset = Mathf.Sin(Time.time * 3f) * 10f;
            swordVisualTransform.anchoredPosition = new Vector2(0f, 65f + yOffset);
        }

        if (needleTransform != null)
        {
            needleAngle += Time.deltaTime * needleSpeed;
            float xOffset = Mathf.Sin(needleAngle) * 148f;
            needleTransform.anchoredPosition = new Vector2(xOffset, 0f);
        }
    }

    private void PullSwordFromTomb()
    {
        if (needlePaused || swordTombAttemptsLeft <= 0) return;

        needlePaused = true;
        float currentX = Mathf.Abs(needleTransform != null ? needleTransform.anchoredPosition.x : 0f);
        int pulledLevel = 4;
        string resultMsg = "";
        Color bannerColor = Color.cyan;

        if (currentX <= 24f)
        {
            pulledLevel = 8;
            resultMsg = "💥 HOÀNG KIM HOÀN HẢO! RÚT ĐƯỢC CỔ THẦN KIẾM BẬC 8!";
            bannerColor = Color.yellow;
            SpawnHitSpark(Vector2.zero, Color.yellow, 150f);
            ShakeArena(0.3f, 10f);
        }
        else if (currentX <= 82f)
        {
            pulledLevel = 6;
            resultMsg = "⚡ XUẤT SẮC! RÚT ĐƯỢC LINH KIẾM BẬC 6!";
            bannerColor = new Color(0.85f, 0.45f, 1f);
            SpawnHitSpark(Vector2.zero, bannerColor, 90f);
            ShakeArena(0.18f, 6f);
        }
        else
        {
            pulledLevel = 4;
            resultMsg = "🗡️ BẢO KIẾM BẬC 4.";
            bannerColor = Color.cyan;
            SpawnHitSpark(Vector2.zero, Color.white, 60f);
            ShakeArena(0.1f, 3f);
        }

        bestSwordPulled = Mathf.Max(bestSwordPulled, pulledLevel);
        currentScore = bestSwordPulled;
        swordTombAttemptsLeft--;

        if (swordImage != null && spSwordAtlas != null && spSwordAtlas.Length >= pulledLevel)
        {
            swordImage.sprite = spSwordAtlas[pulledLevel - 1];
        }

        if (swordFeedbackText != null)
        {
            swordFeedbackText.text = resultMsg;
            swordFeedbackText.color = bannerColor;
        }

        UpdateSwordTombUI();

        StartCoroutine(ResumeNeedleOrEnd(pulledLevel == 8 || swordTombAttemptsLeft <= 0));
    }

    private IEnumerator ResumeNeedleOrEnd(bool isOver)
    {
        yield return new WaitForSecondsRealtime(1.4f);
        if (isOver)
        {
            EndMiniGame();
        }
        else
        {
            needlePaused = false;
        }
    }

    private void UpdateSwordTombUI()
    {
        if (scoreValueText != null)
            scoreValueText.text = $"{swordTombAttemptsLeft} / 3";

        if (mainProgressBar != null) mainProgressBar.fillAmount = (float)swordTombAttemptsLeft / 3f;
        if (progressBarText != null) progressBarText.text = $"KIẾM CAO NHẤT ĐÃ ĐẠT: BẬC {bestSwordPulled}";
    }
    #endregion

    #region END GAME & RESULT MODAL
    private void EndMiniGame()
    {
        if (!isGameRunning) return;
        isGameRunning = false;

        SecretRealmService.RewardResult reward = new SecretRealmService.RewardResult();
        if (SecretRealmService.Instance != null)
        {
            reward = SecretRealmService.Instance.ProcessAndGrantRewards(activeType, currentScore, false);
        }

        ShowResultModal(reward);
    }

    public void ShowResultModal(SecretRealmService.RewardResult reward = default)
    {
        if (resultModal == null)
        {
            var canvas = GameObject.Find("Canvas");
            var existingResult = canvas != null ? canvas.transform.Find("SecretRealmResultModal") : null;
            if (existingResult != null)
            {
                resultModal = existingResult.gameObject;
                var btnConfirm = resultModal.transform.Find("Card/BtnConfirm")?.GetComponent<Button>();
                if (btnConfirm != null)
                {
                    btnConfirm.onClick.RemoveAllListeners();
                    btnConfirm.onClick.AddListener(CloseAll);
                }
            }
            else
            {
                resultModal = CreateResultModal();
            }
        }

        resultModal.SetActive(true);
        resultModal.transform.SetAsLastSibling();

        var txtTitle = resultModal.transform.Find("Card/TitleText")?.GetComponent<Text>();
        if (txtTitle != null) txtTitle.text = "🏆 CHIẾN BÁO BÍ CẢNH THẮNG LỢI!";

        var txtScore = resultModal.transform.Find("Card/ScoreText")?.GetComponent<Text>();
        if (txtScore != null)
        {
            string scoreDesc = $"Loại Bí Cảnh: {GetMiniGameName(activeType)}\nĐiểm Thành Tích: {currentScore}";
            txtScore.text = scoreDesc;
        }

        var txtRewards = resultModal.transform.Find("Card/RewardBox/RewardDetailsText")?.GetComponent<Text>();
        if (txtRewards != null)
        {
            string r = "<color=#FFE666><b>CHIẾN LỢI PHẨM THU ĐƯỢC:</b></color>\n\n";
            if (reward.linhThach > 0) r += $"• Linh Thạch: <color=#FFE666>+{reward.linhThach:N0}</color>\n";
            if (reward.tuVi > 0) r += $"• Tu Vi: <color=#80DFFF>+{reward.tuVi:N0}</color>\n";
            int totalHerbs = reward.herbs0 + reward.herbs1 + reward.herbs2;
            if (totalHerbs > 0) r += $"• Thảo Dược Thần Nông: <color=#55FF88>+{totalHerbs} bó</color>\n";
            if (reward.ore > 0) r += $"• Thiên Tinh Quặng: <color=#FFB84D>+{reward.ore} khối</color>\n";
            if (reward.bonusCoDuyen > 0) r += $"• Cơ Duyên Ban Phúc: <color=#E699FF>+{reward.bonusCoDuyen} CD</color>\n";
            if (reward.swordLevel > 0) r += $"• Cổ Thần Phi Kiếm: <color=#FFE666><b>Bậc {reward.swordLevel}</b></color>\n";
            if (!string.IsNullOrEmpty(reward.specialNote)) r += $"\n<color=#80DFFF><i>{reward.specialNote}</i></color>";

            txtRewards.text = r;
        }
        if(MobileUIAnimationDirector.For(this)!=null)
        {
            var presentation=MobileUIAnimationDirector.For(this).GetComponent<MobileUIReconstruction>();
            if(presentation!=null)MobileUIResponsiveRepairs.ApplyTree(presentation,resultModal.transform);
            var card=resultModal.transform.Find("Card") as RectTransform;
            if(card!=null)
            {
                var group=card.GetComponent<CanvasGroup>();if(group==null)group=card.gameObject.AddComponent<CanvasGroup>();
                if(resultReveal!=null)StopCoroutine(resultReveal);
                resultReveal=StartCoroutine(MobileUIModalMotion.Animate(this,group,card,true,manageInput:false,hideOnClose:false,reward:true));
            }
        }
    }

    private string GetMiniGameName(SecretRealmType type)
    {
        switch (type)
        {
            case SecretRealmType.HordeSurvival: return "Vạn Ma Triều Dâng";
            case SecretRealmType.BarrierBreak: return "Trảm Phá Cấm Chế";
            case SecretRealmType.DanCauldron: return "Đan Thần Kỳ Duyên";
            case SecretRealmType.LightningDodge: return "Vượt Lôi Trì";
            case SecretRealmType.SwordTomb: return "Kiếm Chủng Cổ Thần";
            default: return "Bí Cảnh Thượng Cổ";
        }
    }

    public void CloseAll()
    {
        isGameRunning = false;
        StopAllCoroutines();
        resultReveal=null;
        ClearDynamicArena();
        if (rootModal != null) rootModal.SetActive(false);
        if (resultModal != null) resultModal.SetActive(false);
        if (SecretRealmModalUI.Instance != null) SecretRealmModalUI.Instance.Open();
    }
    #endregion

    #region CORE UI BUILDER
    private void EnsurePlayUI()
    {
        if (rootModal != null) return;

        var canvas = GameObject.Find("Canvas");
        if (canvas == null) return;

        var existing = canvas.transform.Find("SecretRealmPlayRoot");
        if (existing != null)
        {
            rootModal = existing.gameObject;
            WireExistingPlayUI(existing);
            return;
        }

        rootModal = new GameObject("SecretRealmPlayRoot", typeof(RectTransform), typeof(Image));
        rootModal.transform.SetParent(canvas.transform, false);
        var rt = rootModal.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        rootModal.GetComponent<Image>().color = new Color(0.04f, 0.05f, 0.08f, 0.98f);

        // Header Panel (Tọa độ y: 0.84 -> 1.0)
        var headerGo = new GameObject("HeaderPanel", typeof(RectTransform), typeof(Image), typeof(Outline));
        headerGo.transform.SetParent(rootModal.transform, false);
        var hrt = headerGo.GetComponent<RectTransform>();
        hrt.anchorMin = new Vector2(0f, 0.83f);
        hrt.anchorMax = new Vector2(1f, 1f);
        hrt.offsetMin = hrt.offsetMax = Vector2.zero;
        headerGo.GetComponent<Image>().color = new Color(0.1f, 0.12f, 0.18f, 0.98f);

        var hol = headerGo.GetComponent<Outline>();
        hol.effectColor = new Color(0.85f, 0.65f, 0.2f, 0.8f);
        hol.effectDistance = new Vector2(0f, -2f);

        // Nút Rời Bí Cảnh
        var btnQuit = new GameObject("BtnQuit", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        btnQuit.transform.SetParent(headerGo.transform, false);
        var qrt = btnQuit.GetComponent<RectTransform>();
        qrt.anchorMin = qrt.anchorMax = new Vector2(0f, 1f);
        qrt.anchoredPosition = new Vector2(50f, -22f);
        qrt.sizeDelta = new Vector2(76f, 32f);
        btnQuit.GetComponent<Image>().color = new Color(0.6f, 0.15f, 0.2f, 0.95f);
        var qtxt = CreateText(btnQuit.transform, "Txt", "❌ RỜI", 12, FontStyle.Bold, Vector2.zero, Color.white);
        qtxt.rectTransform.sizeDelta = qrt.sizeDelta;
        btnQuit.GetComponent<Button>().onClick.AddListener(CloseAll);

        // Tiêu đề Bí Cảnh (Trọng tâm trên đỉnh)
        titleText = CreateText(headerGo.transform, "TitleText", "BÍ CẢNH THƯỢNG CỔ", 15, FontStyle.Bold, new Vector2(-10f, -22f), Color.yellow);
        titleText.rectTransform.sizeDelta = new Vector2(210f, 28f);

        // Đồng hồ thời gian (Góc phải trên)
        timerText = CreateText(headerGo.transform, "TimerText", "⏱️ 30.0s", 15, FontStyle.Bold, new Vector2(135f, -22f), Color.cyan);
        timerText.rectTransform.sizeDelta = new Vector2(90f, 28f);

        // Bảng quy tắc & hướng dẫn (Instruction Card)
        var ruleBox = new GameObject("RuleCard", typeof(RectTransform), typeof(Image), typeof(Outline));
        ruleBox.transform.SetParent(headerGo.transform, false);
        var rrt = ruleBox.GetComponent<RectTransform>();
        rrt.anchoredPosition = new Vector2(0f, -62f);
        rrt.sizeDelta = new Vector2(350f, 44f);
        ruleBox.GetComponent<Image>().color = new Color(0.06f, 0.08f, 0.12f, 0.9f);
        var rol = ruleBox.GetComponent<Outline>();
        rol.effectColor = new Color(0.4f, 0.6f, 0.85f, 0.4f);
        rol.effectDistance = new Vector2(1f, -1f);

        instructionText = CreateText(ruleBox.transform, "TxtRule", "Hướng dẫn chơi...", 11, FontStyle.Normal, Vector2.zero, new Color(0.9f, 0.95f, 1f));
        instructionText.rectTransform.sizeDelta = new Vector2(340f, 40f);
        instructionText.alignment = TextAnchor.MiddleLeft;

        // Điểm số / Tiến độ (Hàng thứ 3)
        scoreTitleText = CreateText(headerGo.transform, "ScoreTitle", "TIẾN ĐỘ:", 12, FontStyle.Bold, new Vector2(-60f, -95f), Color.white);
        scoreTitleText.rectTransform.sizeDelta = new Vector2(140f, 20f);
        scoreValueText = CreateText(headerGo.transform, "ScoreValue", "0 / 50", 14, FontStyle.Bold, new Vector2(80f, -95f), Color.yellow);
        scoreValueText.rectTransform.sizeDelta = new Vector2(140f, 20f);

        // Vùng chiến đấu / Arena trung tâm
        var areaGo = new GameObject("ArenaContainer", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
        areaGo.transform.SetParent(rootModal.transform, false);
        arenaContainer = areaGo.GetComponent<RectTransform>();
        arenaContainer.anchorMin = new Vector2(0f, 0.14f);
        arenaContainer.anchorMax = new Vector2(1f, 0.83f);
        arenaContainer.offsetMin = arenaContainer.offsetMax = Vector2.zero;
        arenaBgImage = areaGo.GetComponent<Image>();
        arenaBgImage.color = new Color(0.08f, 0.1f, 0.14f, 1f);

        // Footer / Action Panel
        var footerGo = new GameObject("FooterPanel", typeof(RectTransform), typeof(Image));
        footerGo.transform.SetParent(rootModal.transform, false);
        var frt = footerGo.GetComponent<RectTransform>();
        frt.anchorMin = new Vector2(0f, 0f);
        frt.anchorMax = new Vector2(1f, 0.14f);
        frt.offsetMin = frt.offsetMax = Vector2.zero;
        footerGo.GetComponent<Image>().color = new Color(0.08f, 0.1f, 0.15f, 0.98f);

        // Thanh tiến trình chính (Main Progress Bar)
        var pBarBg = new GameObject("MainProgressBarBg", typeof(RectTransform), typeof(Image));
        pBarBg.transform.SetParent(footerGo.transform, false);
        var prt = pBarBg.GetComponent<RectTransform>();
        prt.anchoredPosition = new Vector2(0f, 32f);
        prt.sizeDelta = new Vector2(340f, 18f);
        pBarBg.GetComponent<Image>().color = new Color(0.05f, 0.06f, 0.09f, 1f);

        var pFill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        pFill.transform.SetParent(pBarBg.transform, false);
        var firt = pFill.GetComponent<RectTransform>();
        firt.anchorMin = Vector2.zero;
        firt.anchorMax = Vector2.one;
        firt.offsetMin = firt.offsetMax = Vector2.zero;
        mainProgressBar = pFill.GetComponent<Image>();
        mainProgressBar.color = new Color(0.2f, 0.85f, 0.45f, 1f);
        mainProgressBar.type = Image.Type.Filled;
        mainProgressBar.fillMethod = Image.FillMethod.Horizontal;
        mainProgressBar.fillAmount = 0f;

        var pTxtGo = new GameObject("TxtProgress", typeof(RectTransform), typeof(Text));
        pTxtGo.transform.SetParent(pBarBg.transform, false);
        var ptrt = pTxtGo.GetComponent<RectTransform>();
        ptrt.anchorMin = Vector2.zero;
        ptrt.anchorMax = Vector2.one;
        ptrt.offsetMin = ptrt.offsetMax = Vector2.zero;
        progressBarText = pTxtGo.GetComponent<Text>();
        progressBarText.font = DefaultFont;
        progressBarText.fontSize = 11;
        progressBarText.fontStyle = FontStyle.Bold;
        progressBarText.alignment = TextAnchor.MiddleCenter;
        progressBarText.color = Color.white;
        progressBarText.text = "TIẾN ĐỘ: 0%";
    }

    private void WireExistingPlayUI(Transform root)
    {
        rootModal = root.gameObject;
        titleText = root.Find("HeaderPanel/TitleText")?.GetComponent<Text>();
        timerText = root.Find("HeaderPanel/TimerText")?.GetComponent<Text>();
        instructionText = root.Find("HeaderPanel/RuleCard/TxtRule")?.GetComponent<Text>();
        scoreTitleText = root.Find("HeaderPanel/ScoreTitle")?.GetComponent<Text>();
        scoreValueText = root.Find("HeaderPanel/ScoreValue")?.GetComponent<Text>();

        var btnQuit = root.Find("HeaderPanel/BtnQuit")?.GetComponent<Button>();
        if (btnQuit != null)
        {
            btnQuit.onClick.RemoveAllListeners();
            btnQuit.onClick.AddListener(CloseAll);
        }

        var arenaTr = root.Find("ArenaContainer");
        if (arenaTr != null)
        {
            arenaContainer = arenaTr as RectTransform;
            arenaBgImage = arenaTr.GetComponent<Image>();
        }

        mainProgressBar = root.Find("FooterPanel/MainProgressBarBg/Fill")?.GetComponent<Image>();
        progressBarText = root.Find("FooterPanel/MainProgressBarBg/TxtProgress")?.GetComponent<Text>();
    }

    private void SetHeader(string title, Color titleColor, string objective, string howToPlay)
    {
        if (titleText != null)
        {
            titleText.text = title;
            titleText.color = titleColor;
        }

        if (instructionText != null)
        {
            instructionText.text = $"{objective}\n{howToPlay}";
        }
    }

    private void CreateBottomActionButton(string title, Sprite icon, Color btnColor, UnityEngine.Events.UnityAction action)
    {
        if (actionButtonGo != null) Destroy(actionButtonGo);

        var footer = rootModal != null ? rootModal.transform.Find("FooterPanel") : null;
        if (footer == null) return;

        actionButtonGo = new GameObject("MainActionBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        actionButtonGo.transform.SetParent(footer, false);
        var rt = actionButtonGo.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0f, -12f);
        rt.sizeDelta = new Vector2(330f, 48f);

        actionButtonGo.GetComponent<Image>().color = btnColor;
        var ol = actionButtonGo.GetComponent<Outline>();
        ol.effectColor = new Color(1f, 0.9f, 0.35f, 0.95f);
        ol.effectDistance = new Vector2(1.5f, -1.5f);

        if (icon != null)
        {
            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(actionButtonGo.transform, false);
            var irt = iconGo.GetComponent<RectTransform>();
            irt.anchorMin = irt.anchorMax = irt.pivot = new Vector2(0f, 0.5f);
            irt.anchoredPosition = new Vector2(14f, 0f);
            irt.sizeDelta = new Vector2(34f, 34f);
            var iImg = iconGo.GetComponent<Image>();
            iImg.sprite = icon;
            iImg.preserveAspect = true;
        }

        var txtGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
        txtGo.transform.SetParent(actionButtonGo.transform, false);
        var trt = txtGo.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(icon != null ? 48f : 0f, 0f);
        trt.offsetMax = Vector2.zero;
        var txt = txtGo.GetComponent<Text>();
        txt.font = DefaultFont;
        txt.fontSize = 13;
        txt.fontStyle = FontStyle.Bold;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        txt.text = title;

        actionButtonGo.GetComponent<Button>().onClick.AddListener(action);
    }

    private GameObject CreateResultModal()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) return null;

        var go = new GameObject("SecretRealmResultModal", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(canvas.transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.88f);

        var card = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(Outline));
        card.transform.SetParent(go.transform, false);
        var crt = card.GetComponent<RectTransform>();
        crt.sizeDelta = new Vector2(340f, 440f);
        crt.anchoredPosition = Vector2.zero;
        card.GetComponent<Image>().color = new Color(0.12f, 0.15f, 0.22f, 0.98f);

        var ol = card.GetComponent<Outline>();
        ol.effectColor = new Color(1f, 0.82f, 0.2f, 0.95f);
        ol.effectDistance = new Vector2(2f, -2f);

        CreateText(card.transform, "TitleText", "🏆 CHIẾN BÁO BÍ CẢNH THẮNG LỢI!", 16, FontStyle.Bold, new Vector2(0f, 185f), Color.yellow);
        CreateText(card.transform, "ScoreText", "Điểm số hoàn thành: 100", 13, FontStyle.Bold, new Vector2(0f, 145f), Color.cyan);

        var rBox = new GameObject("RewardBox", typeof(RectTransform), typeof(Image), typeof(Outline));
        rBox.transform.SetParent(card.transform, false);
        var rrt = rBox.GetComponent<RectTransform>();
        rrt.anchoredPosition = new Vector2(0f, 15f);
        rrt.sizeDelta = new Vector2(310f, 210f);
        rBox.GetComponent<Image>().color = new Color(0.06f, 0.08f, 0.12f, 0.95f);
        var rol = rBox.GetComponent<Outline>();
        rol.effectColor = new Color(0.85f, 0.65f, 0.2f, 0.5f);
        rol.effectDistance = new Vector2(1f, -1f);

        var rtxt = CreateText(rBox.transform, "RewardDetailsText", "Phần thưởng...", 12, FontStyle.Normal, Vector2.zero, Color.white);
        rtxt.rectTransform.sizeDelta = new Vector2(290f, 195f);
        rtxt.alignment = TextAnchor.UpperLeft;

        var btnConfirm = new GameObject("BtnConfirm", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        btnConfirm.transform.SetParent(card.transform, false);
        var brt = btnConfirm.GetComponent<RectTransform>();
        brt.anchoredPosition = new Vector2(0f, -170f);
        brt.sizeDelta = new Vector2(270f, 50f);
        btnConfirm.GetComponent<Image>().color = new Color(0.18f, 0.65f, 0.35f, 0.98f);

        var bol = btnConfirm.GetComponent<Outline>();
        bol.effectColor = new Color(1f, 0.9f, 0.4f, 0.9f);
        bol.effectDistance = new Vector2(1.5f, -1.5f);

        var btxt = CreateText(btnConfirm.transform, "Txt", "🏆 XÁC NHẬN THU NHẬN", 14, FontStyle.Bold, Vector2.zero, Color.white);
        btxt.rectTransform.sizeDelta = brt.sizeDelta;

        btnConfirm.GetComponent<Button>().onClick.AddListener(CloseAll);

        return go;
    }

    private void ClearDynamicArena()
    {
        if (arenaContainer == null) return;
        for (int i = arenaContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(arenaContainer.GetChild(i).gameObject);
        }
        activeDemonGoList.Clear();
        activeThunderOrbs.Clear();
        activeHazards.Clear();
        cauldronObjects.Clear();
        cauldronStatusTexts.Clear();
    }

    private void ClearGameObjects(List<GameObject> list)
    {
        foreach (var go in list) if (go != null) Destroy(go);
        list.Clear();
    }

    private Text CreateText(Transform parent, string name, string val, int size, FontStyle style, Vector2 pos, Color col)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(300f, 30f);
        var txt = go.GetComponent<Text>();
        txt.font = DefaultFont;
        txt.fontSize = size;
        txt.fontStyle = style;
        txt.color = col;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.text = val;
        return txt;
    }

    private void SpawnFloatingText(string content, Vector2 pos, Color col, float scale = 1f)
    {
        if (arenaContainer == null) return;

        var go = new GameObject("FText", typeof(RectTransform), typeof(Text), typeof(Outline));
        go.transform.SetParent(arenaContainer, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(240f, 32f);
        rt.localScale = Vector3.one * scale;

        var txt = go.GetComponent<Text>();
        txt.font = DefaultFont;
        txt.fontSize = 15;
        txt.fontStyle = FontStyle.Bold;
        txt.color = col;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.text = content;
        txt.raycastTarget = false;

        var ol = go.GetComponent<Outline>();
        ol.effectColor = Color.black;
        ol.effectDistance = new Vector2(1.5f, -1.5f);

        StartCoroutine(AnimateFloatingText(go, rt, txt));
    }

    private IEnumerator AnimateFloatingText(GameObject go, RectTransform rt, Text txt)
    {
        float elapsed = 0f;
        float duration = 0.85f;
        Vector2 startPos = rt.anchoredPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            if (rt != null) rt.anchoredPosition = startPos + new Vector2(0f, t * 50f);
            if (txt != null)
            {
                Color c = txt.color;
                c.a = 1f - t;
                txt.color = c;
            }
            yield return null;
        }

        if (go != null) Destroy(go);
    }

    private void SpawnHitSpark(Vector2 pos, Color col, float size)
    {
        if (arenaContainer == null) return;

        var go = new GameObject("HitSpark", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(arenaContainer, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(size, size);

        var img = go.GetComponent<Image>();
        img.sprite = spHitSpark != null ? spHitSpark : spSoftGlow;
        img.color = col;
        img.raycastTarget = false;

        StartCoroutine(AnimateHitSpark(go, rt, img));
    }

    private IEnumerator AnimateHitSpark(GameObject go, RectTransform rt, Image img)
    {
        float elapsed = 0f;
        float duration = 0.25f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            if (rt != null) rt.localScale = Vector3.one * (1f + t * 0.8f);
            if (img != null)
            {
                Color c = img.color;
                c.a = 1f - t;
                img.color = c;
            }
            yield return null;
        }

        if (go != null) Destroy(go);
    }

    private void ShakeArena(float duration, float magnitude)
    {
        StartCoroutine(DoShakeArena(duration, magnitude));
    }

    private IEnumerator DoShakeArena(float duration, float magnitude)
    {
        if (arenaContainer == null) yield break;

        Vector2 originalPos = arenaContainer.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float x = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            float y = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            if (arenaContainer != null) arenaContainer.anchoredPosition = originalPos + new Vector2(x, y);
            yield return null;
        }

        if (arenaContainer != null) arenaContainer.anchoredPosition = originalPos;
    }
    #endregion
}
