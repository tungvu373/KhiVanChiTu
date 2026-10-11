using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Gắn vào CharacterFrame (Khung nhân vật góc dưới bên trái màn hình Combat).
/// Khi người chơi nhấn vào khung này, tự động mở Panel Thông Tin Chi Tiết Nhân Vật.
/// </summary>
public class CharacterFrameButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Image frameImage;
    private Color originalColor;
    private Color hoverColor;

    private void Awake()
    {
        frameImage = GetComponent<Image>();
        if (frameImage != null)
        {
            originalColor = frameImage.color;
            hoverColor = new Color(originalColor.r * 1.25f, originalColor.g * 1.25f, originalColor.b * 1.35f, originalColor.a);
        }

        // Đảm bảo có Button hoặc nhận diện được Raycast
        if (frameImage != null)
        {
            frameImage.raycastTarget = true;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        CharacterDetailModalUI.Open();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (frameImage != null)
        {
            frameImage.color = hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (frameImage != null)
        {
            frameImage.color = originalColor;
        }
    }
}
