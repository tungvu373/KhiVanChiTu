using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gắn vào BtnHUDBiCanh trên TopBar HUD để luôn bắt sự kiện click mở SecretRealmModalUI ở runtime.
/// Khắc phục triệt để việc mất listener trong Editor khi bấm Play mode.
/// </summary>
public class BiCanhButtonHandler : MonoBehaviour
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
            btn.onClick.RemoveListener(OnBiCanhButtonClicked);
            btn.onClick.AddListener(OnBiCanhButtonClicked);
        }
    }

    public void OnBiCanhButtonClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX();
        if (SecretRealmModalUI.Instance != null)
        {
            SecretRealmModalUI.Instance.Open();
        }
        else
        {
            var modal = Object.FindAnyObjectByType<SecretRealmModalUI>(FindObjectsInactive.Include);
            if (modal != null)
            {
                modal.Open();
            }
        }
    }
}

