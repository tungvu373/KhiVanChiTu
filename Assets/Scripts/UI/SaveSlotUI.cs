using UnityEngine;
using UnityEngine.UI;
using System;

public class SaveSlotUI : MonoBehaviour
{
    public int slotIndex; 
    
    [Header("UI Components")]
    public GameObject dataPanel;   // Bật lên khi ô này có nhân vật
    public GameObject emptyPanel;  // Bật lên khi ô này trống
    
    [Header("Data Display")]
    public Text nameText;
    public Text cultivationText;
    public Text playTimeText;
    public Image modelImage;       // Hiển thị Model nhân vật
    public Sprite maleSprite;
    public Sprite femaleSprite;

    [Header("Buttons")]
    public Button slotButton;      // Bao trùm cả ô để bắt sự kiện click
    public Button deleteButton;    // Nút [Xóa]

    private MainMenuManager menuManager;
    private CharacterSaveData currentData;

    public void Initialize(MainMenuManager manager, CharacterSaveData data, int index)
    {
        menuManager = manager;
        slotIndex = index;
        currentData = data;

        RefreshUI();

        // Xóa listener cũ
        if(slotButton != null) slotButton.onClick.RemoveAllListeners();
        if(deleteButton != null) deleteButton.onClick.RemoveAllListeners();

        if (currentData.isSlotEmpty)
        {
            // Trống -> Click vào thì tạo nhân vật
            if(slotButton != null) slotButton.onClick.AddListener(() => menuManager.OpenCharacterCreation(slotIndex));
        }
        else
        {
            // Có dữ liệu -> Click vào thì hỏi vào game
            if(slotButton != null) slotButton.onClick.AddListener(() => menuManager.AskConfirmEnterGame(slotIndex, currentData));
            
            // Xóa nhân vật
            if(deleteButton != null) deleteButton.onClick.AddListener(() => menuManager.AskConfirmDeleteSlot(slotIndex));
        }
    }

    private void RefreshUI()
    {
        if (currentData.isSlotEmpty)
        {
            if (dataPanel) dataPanel.SetActive(false);
            if (emptyPanel) emptyPanel.SetActive(true); 
            if (deleteButton) deleteButton.gameObject.SetActive(false);
            
            // Ẩn ảnh Model khi trống
            if (modelImage != null)
            {
                modelImage.color = new Color(1, 1, 1, 0); 
            }
        }
        else
        {
            if (dataPanel) dataPanel.SetActive(true);
            if (emptyPanel) emptyPanel.SetActive(false);
            if (deleteButton) deleteButton.gameObject.SetActive(true);

            // Gán thông tin hiển thị
            if (nameText) nameText.text = currentData.characterName;
            if (cultivationText) cultivationText.text = $"Tu vi: {currentData.stageName}";
            
            if (playTimeText)
            {
                TimeSpan time = TimeSpan.FromSeconds(currentData.playTime);
                playTimeText.text = string.Format("Giờ chơi: {0:D2}h:{1:D2}m\nNgày: {2}", time.Hours, time.Minutes, currentData.lastPlayedDate);
            }

            // Gán ảnh nhân vật
            if (modelImage != null)
            {
                modelImage.color = new Color(1, 1, 1, 1);
                modelImage.sprite = (currentData.gender == 0) ? maleSprite : femaleSprite;
            }
        }
    }
}
