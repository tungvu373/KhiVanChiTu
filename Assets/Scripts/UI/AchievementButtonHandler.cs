using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gắn vào BtnHUDAchievement trên TopBar HUD để luôn bắt sự kiện click mở AchievementModalUI ở runtime.
/// </summary>
public class AchievementButtonHandler : MonoBehaviour
{
    private Button btn;

    private void Awake()
    {
        btn = GetComponent<Button>();
        WireButton();
    }

    private void Start()
    {
        WireButton();
    }

    private void WireButton()
    {
        if (btn == null) btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveListener(OnAchievementButtonClicked);
            btn.onClick.AddListener(OnAchievementButtonClicked);
        }
    }

    public void OnAchievementButtonClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX();
        AchievementModalUI.Open();
    }
}

