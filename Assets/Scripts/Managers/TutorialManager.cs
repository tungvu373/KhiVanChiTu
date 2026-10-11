using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    private bool swordDone = false;
    private bool upgradeDone = false;
    private bool coDuyenDone = false;
    private bool shopDone = false;

    [Header("UI References")]
    public GameObject tutorialOverlay; 
    public Text tutorialText;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;

        if (PlayerPrefs.GetInt("TutorialCompleted", 0) == 1)
        {
            if (tutorialOverlay != null) tutorialOverlay.SetActive(false);
            Destroy(this);
            return;
        }
        
        if (tutorialOverlay != null) tutorialOverlay.SetActive(false);
    }

    private void Start()
    {
        if (tutorialOverlay == null) return;
        
        // Chuyển Overlay thành nút bấm toàn màn hình để Click-to-Dismiss
        Button overlayBtn = tutorialOverlay.GetComponent<Button>();
        if (overlayBtn == null) overlayBtn = tutorialOverlay.AddComponent<Button>();
        overlayBtn.onClick.AddListener(OnOverlayClicked);

        StartCoroutine(TutorialRoutine());
    }

    private IEnumerator TutorialRoutine()
    {
        while (!swordDone || !upgradeDone || !coDuyenDone || !shopDone)
        {
            // Nếu đang bật 1 bảng Hướng dẫn nào đó thì chờ người chơi tắt đi
            if (tutorialOverlay != null && tutorialOverlay.activeSelf) 
            {
                yield return null;
                continue;
            }

            // 1. Check Kiếm (Lần đầu tiên rớt ra túi đồ)
            if (!swordDone && GetInventorySwordCount() >= 1)
            {
                swordDone = true;
                ShowTutorial("Kéo thả thanh kiếm mới nhặt vào thanh kiếm đang trang bị\nđể Đột Phá sức mạnh!");
            }
            // 2. Check Cơ Duyên (Hộp quà bay qua)
            else if (!coDuyenDone && Object.FindObjectOfType<EncounterItem>() != null)
            {
                coDuyenDone = true;
                ShowTutorial("Kỳ ngộ (Cơ duyên) vừa bay ngang qua màn hình!\nClick nhanh vào nó trước khi bay mất để nhận thưởng!");
            }
            // 3. Check Linh Thạch 500
            else if (!upgradeDone && EconomyManager.Instance != null && EconomyManager.Instance.currentLinhThach >= 500)
            {
                upgradeDone = true;
                ShowTutorial("Linh thạch đã đủ!\nHãy mở tab Nâng cấp Linh Lực để gia gia tăng sát thương!");
            }
            // 4. Check Linh Thạch 2000
            else if (!shopDone && EconomyManager.Instance != null && EconomyManager.Instance.currentLinhThach >= 2000)
            {
                shopDone = true;
                ShowTutorial("Đã tích lũy nhiều Linh Thạch!\nHãy mở Tiệm Đan Dược mua đồ sinh tồn chuẩn bị cho Lôi Kiếp!");
            }

            yield return null;
        }

        PlayerPrefs.SetInt("TutorialCompleted", 1);
        PlayerPrefs.Save();
        Debug.Log("✅ Đã hoàn thành toàn bộ Hướng Dẫn Tân Thủ!");
    }

    private int GetInventorySwordCount()
    {
        if (MergeManager.Instance == null || MergeManager.Instance.inventoryPages == null) return 0;
        int count = 0;
        foreach (Transform page in MergeManager.Instance.inventoryPages)
        {
            foreach (Transform slot in page)
            {
                if (slot.GetComponentInChildren<MergeItem>() != null) count++;
            }
        }
        return count;
    }

    private void ShowTutorial(string text)
    {
        if (tutorialOverlay != null) tutorialOverlay.SetActive(true);
        if (tutorialText != null) tutorialText.text = text;
        Time.timeScale = 0f; 
        
        // Đảm bảo chặn click xuyên qua lớp mờ
        Image overlayImg = tutorialOverlay.GetComponent<Image>();
        if (overlayImg != null) overlayImg.raycastTarget = true;
    }

    private void OnOverlayClicked()
    {
        // Khi người chơi click vào màn đen mờ -> Tắt hướng dẫn và cho game chạy tiếp
        tutorialOverlay.SetActive(false);
        Time.timeScale = 1f; 
    }
}
