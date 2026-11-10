using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Giao diện HUD Kỹ Năng Mobile (48px+ touch targets, Portrait 9:16).
/// Quản lý 4 kỹ năng kinh điển: Vạn Kiếm Quy Tông (Tuyệt kỹ), Chưởng Ấn, Lôi Phạt, Âm Dương Trận.
/// Hiển thị Cooldown, Tiêu hao Linh Lực (MP), Trạng thái Mở Khóa, Hiệu ứng Phát Sáng khi sẵn sàng.
/// </summary>
public class MobileSkillHUD : MonoBehaviour
{
    public static MobileSkillHUD Instance { get; private set; }

    [Header("Ultimate: Vạn Kiếm Quy Tông")]
    public Button btnUltimate;
    public Image imgUltiFill;
    public Image imgUltiGlow;
    public Text txtUltiTitle;
    public Text txtUltiMana;

    [Header("Normal Skills: Chưởng Ấn [0], Lôi Phạt [1], Âm Dương Trận [2]")]
    public Button[] btnNormalSkills = new Button[3];
    public Image[] imgNormalCooldownOverlays = new Image[3];
    public Text[] txtNormalCooldownTimers = new Text[3];
    public Text[] txtNormalTitles = new Text[3];
    public Text[] txtNormalCosts = new Text[3];
    public GameObject[] lockOverlays = new GameObject[3];

    private static readonly string[] SkillNames = { "Ấn Chưởng", "Lôi Phạt", "Âm Dương" };
    private static readonly int[] ManaCosts = { 40, 25, 30 };
    private static readonly string[] LockRequirements = { "Đại Thừa", "Nguyên Anh", "4 Trận" };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        WireButtons();
    }

    public void WireButtons()
    {
        if (btnUltimate != null)
        {
            btnUltimate.onClick.RemoveAllListeners();
            btnUltimate.onClick.AddListener(() =>
            {
                if (SkillManager.Instance != null)
                    SkillManager.Instance.CastActiveSkill();
            });
        }

        for (int i = 0; i < btnNormalSkills.Length; i++)
        {
            int index = i;
            if (btnNormalSkills[i] != null)
            {
                btnNormalSkills[i].onClick.RemoveAllListeners();
                btnNormalSkills[i].onClick.AddListener(() =>
                {
                    if (SkillManager.Instance != null)
                        SkillManager.Instance.CastNormalSkillManual(index);
                });
            }
        }
    }

    private void Update()
    {
        var skillMgr = SkillManager.Instance;
        if (skillMgr == null) return;

        UpdateUltimateUI(skillMgr);
        UpdateNormalSkillsUI(skillMgr);
    }

    private void UpdateUltimateUI(SkillManager sm)
    {
        if (btnUltimate == null) return;

        bool isUnlocked = sm.activeSkill != null && sm.activeSkill.isUnlocked;
        bool isActive = sm.isSkillActive;
        float cd = sm.GetActiveSkillCooldown();
        float mana = sm.currentLinhLuc;
        float maxMana = sm.maxLinhLuc;
        float fillRatio = Mathf.Clamp01(mana / Mathf.Max(1f, maxMana));

        if (!isUnlocked)
        {
            btnUltimate.interactable = false;
            if (txtUltiTitle != null) txtUltiTitle.text = "VẠN KIẾM";
            if (txtUltiMana != null) txtUltiMana.text = "Kiếm Lv7";
            if (imgUltiFill != null) imgUltiFill.fillAmount = 0f;
            if (imgUltiGlow != null) imgUltiGlow.gameObject.SetActive(false);
            return;
        }

        if (imgUltiFill != null)
        {
            imgUltiFill.fillAmount = fillRatio;
        }

        if (isActive)
        {
            btnUltimate.interactable = false;
            if (txtUltiTitle != null) txtUltiTitle.text = "XUẤT TRẬN";
            if (txtUltiMana != null) txtUltiMana.text = $"{Mathf.CeilToInt(mana)} MP";
            if (imgUltiGlow != null)
            {
                imgUltiGlow.gameObject.SetActive(true);
                imgUltiGlow.color = Color.Lerp(Color.yellow, new Color(1f, 0.5f, 0f), Mathf.PingPong(Time.unscaledTime * 3f, 1f));
            }
        }
        else if (cd > 0f)
        {
            btnUltimate.interactable = false;
            if (txtUltiTitle != null) txtUltiTitle.text = "HỒI";
            if (txtUltiMana != null) txtUltiMana.text = $"{cd:F1}s";
            if (imgUltiGlow != null) imgUltiGlow.gameObject.SetActive(false);
        }
        else
        {
            bool ready = (mana >= maxMana);
            btnUltimate.interactable = ready;
            if (txtUltiTitle != null) txtUltiTitle.text = ready ? "XUẤT CHIÊU" : "VẠN KIẾM";
            if (txtUltiMana != null) txtUltiMana.text = $"{Mathf.FloorToInt(mana)}/{Mathf.FloorToInt(maxMana)}";

            if (imgUltiGlow != null)
            {
                imgUltiGlow.gameObject.SetActive(ready);
                if (ready)
                {
                    imgUltiGlow.color = new Color(1f, 0.85f, 0.2f, 0.6f + Mathf.PingPong(Time.unscaledTime * 2f, 0.4f));
                }
            }
        }
    }

    private void UpdateNormalSkillsUI(SkillManager sm)
    {
        for (int i = 0; i < 3; i++)
        {
            if (i >= sm.normalSkills.Length || btnNormalSkills[i] == null) continue;

            var skill = sm.normalSkills[i];
            bool isUnlocked = skill != null && skill.isUnlocked;
            float cd = sm.GetNormalSkillCooldown(i);
            float baseCd = skill != null ? Mathf.Max(1f, skill.baseCooldown) : 10f;
            float cost = ManaCosts[i];

            if (lockOverlays[i] != null)
            {
                lockOverlays[i].SetActive(!isUnlocked);
            }

            if (!isUnlocked)
            {
                btnNormalSkills[i].interactable = false;
                if (imgNormalCooldownOverlays[i] != null) imgNormalCooldownOverlays[i].fillAmount = 1f;
                if (txtNormalCooldownTimers[i] != null) txtNormalCooldownTimers[i].text = LockRequirements[i];
                if (txtNormalTitles[i] != null) txtNormalTitles[i].text = SkillNames[i];
                if (txtNormalCosts[i] != null) txtNormalCosts[i].text = $"{cost} MP";
                continue;
            }

            if (txtNormalTitles[i] != null) txtNormalTitles[i].text = SkillNames[i];
            if (txtNormalCosts[i] != null) txtNormalCosts[i].text = $"{cost} MP";

            if (cd > 0f)
            {
                btnNormalSkills[i].interactable = false;
                if (imgNormalCooldownOverlays[i] != null)
                {
                    imgNormalCooldownOverlays[i].gameObject.SetActive(true);
                    imgNormalCooldownOverlays[i].fillAmount = cd / baseCd;
                }
                if (txtNormalCooldownTimers[i] != null)
                {
                    txtNormalCooldownTimers[i].gameObject.SetActive(true);
                    txtNormalCooldownTimers[i].text = $"{cd:F1}s";
                }
            }
            else
            {
                bool hasMana = (sm.currentLinhLuc >= cost);
                bool canCast = hasMana && !sm.isSkillActive;
                btnNormalSkills[i].interactable = canCast;

                if (imgNormalCooldownOverlays[i] != null)
                {
                    imgNormalCooldownOverlays[i].gameObject.SetActive(!hasMana);
                    imgNormalCooldownOverlays[i].fillAmount = hasMana ? 0f : 0.6f;
                }
                if (txtNormalCooldownTimers[i] != null)
                {
                    txtNormalCooldownTimers[i].gameObject.SetActive(!hasMana);
                    txtNormalCooldownTimers[i].text = hasMana ? "" : "Hết MP";
                }
            }
        }
    }
}

