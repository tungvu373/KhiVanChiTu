using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gắn vào BtnHUDDaoAn trên TopBar HUD để bắt sự kiện click mở DaoAnModalUI ở runtime.
/// Đảm bảo luôn hoạt động ổn định và tin cậy tương tự StorageButtonHandler.
/// </summary>
public class DaoAnButtonHandler : MonoBehaviour
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
            btn.onClick.RemoveListener(OnDaoAnButtonClicked);
            btn.onClick.AddListener(OnDaoAnButtonClicked);
        }
    }

    public void OnDaoAnButtonClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX();
        if (DaoAnModalUI.Instance != null)
        {
            DaoAnModalUI.Instance.Open();
        }
        else
        {
            var modal = Object.FindAnyObjectByType<DaoAnModalUI>(FindObjectsInactive.Include);
            if (modal != null)
            {
                modal.Open();
            }
            else
            {
                var go = new GameObject("DaoAnModalUI", typeof(DaoAnModalUI));
                go.GetComponent<DaoAnModalUI>().Open();
            }
        }
    }
}

