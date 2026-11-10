using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gắn vào BtnHUDStorage (Nút TÚI trên TopBar HUD) để luôn bắt sự kiện click mở StorageModalUI ở runtime.
/// Khắc phục triệt để việc mất listener trong Editor khi bấm Play mode.
/// </summary>
public class StorageButtonHandler : MonoBehaviour
{
    [SerializeField] private StorageCategory initialCategory = StorageCategory.All;
    private Button btn;

    private void Awake()
    {
        btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnStorageButtonClicked);
        }
    }

    private void Start()
    {
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnStorageButtonClicked);
        }
    }

    public void OnStorageButtonClicked()
    {
        StorageModalUI.Open(initialCategory);
    }
}

