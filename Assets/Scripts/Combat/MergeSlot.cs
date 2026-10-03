using UnityEngine;
using UnityEngine.EventSystems;

public class MergeSlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObj = eventData.pointerDrag;
        if (droppedObj == null) return;
        
        MergeItem droppedItem = droppedObj.GetComponent<MergeItem>();
        if (droppedItem == null) return;

        MergeItem currentItem = GetComponentInChildren<MergeItem>();
        bool isEquipSlot = gameObject.name.StartsWith("Equip");

        if (currentItem == null) // Ô trống
        {
            droppedItem.transform.SetParent(transform, false);
            RectTransform rt = droppedItem.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;
        }
        else // Ô đang có kiếm
        {
            if (isEquipSlot)
            {
                Debug.LogWarning("[Merge] KHÔNG cho phép ghép kiếm trực tiếp trên Ô Trang Bị! Vui lòng gỡ xuống Kho đồ.");
                droppedItem.transform.SetParent(droppedItem.originalParent, false);
                RectTransform rt = droppedItem.GetComponent<RectTransform>();
                rt.anchoredPosition = Vector2.zero;
                rt.localScale = Vector3.one;
            }
            else if (currentItem.level == droppedItem.level && currentItem.level < 8)
            {
                // Hợp nhất (Merge) cộng dồn phôi
                currentItem.currentPieces += droppedItem.currentPieces;
                Destroy(droppedItem.gameObject);
                
                // Kiểm tra nếu đủ phôi thì lên cấp (có thể lên nhiều cấp nếu đập 1 cục to vào)
                while (currentItem.currentPieces >= currentItem.GetRequiredPieces() && currentItem.level < 8)
                {
                    currentItem.currentPieces -= currentItem.GetRequiredPieces();
                    currentItem.level++;
                    if (currentItem.currentPieces <= 0) currentItem.currentPieces = 1;
                }
                
                currentItem.UpdateUI();
                Debug.Log($"[Merge] Tiến trình ghép kiếm: Lv.{currentItem.level} ({currentItem.currentPieces}/{currentItem.GetRequiredPieces()})!");
            }
            else
            {
                // Trả về chỗ cũ nếu không cùng cấp hoặc đang là max cấp
                droppedItem.transform.SetParent(droppedItem.originalParent, false);
                RectTransform rt = droppedItem.GetComponent<RectTransform>();
                rt.anchoredPosition = Vector2.zero;
                rt.localScale = Vector3.one;
            }
        }
        
        if (MergeManager.Instance != null) MergeManager.Instance.OnEquipChanged();
    }
}
