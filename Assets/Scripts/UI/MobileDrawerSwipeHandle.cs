using UnityEngine;
using UnityEngine.EventSystems;

public sealed class MobileDrawerSwipeHandle : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private MobileDrawerUIController controller;
    [SerializeField] private float minimumSwipePixels = 60f;
    private Vector2 startPosition;

    public void OnPointerDown(PointerEventData eventData)
    {
        startPosition = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (startPosition.y - eventData.position.y >= minimumSwipePixels)
        {
            if (controller != null) controller.CloseActiveDrawer();
            else MobileDrawerUIController.Instance?.CloseActiveDrawer();
        }
    }
}
