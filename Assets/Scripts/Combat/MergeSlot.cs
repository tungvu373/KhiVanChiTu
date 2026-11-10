using System.Collections;
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
        bool fromEquipSlot = droppedItem.originalParent != null && droppedItem.originalParent.name.StartsWith("Equip");
        if (isEquipSlot && SwordLoadoutService.Instance != null &&
            transform.GetSiblingIndex() >= SwordLoadoutService.Instance.GetCurrentCapacity())
        {
            droppedItem.transform.SetParent(droppedItem.originalParent, false);
            droppedItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            return;
        }

        if (currentItem == null) // Ô trống
        {
            droppedItem.transform.SetParent(transform, false);
            RectTransform rt = droppedItem.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;
            StartCoroutine(PunchBounce(droppedItem.transform));
        }
        else // Ô đang có kiếm
        {
            if (isEquipSlot)
            {
                // Hoán đổi hai thanh kiếm, tuyệt đối không ghép khi thả lên trang bị.
                Transform origin = droppedItem.originalParent;
                if (origin == null || origin.GetComponent<MergeSlot>() == null) return;
                currentItem.transform.SetParent(origin, false);
                currentItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                currentItem.transform.localScale = Vector3.one;
                droppedItem.transform.SetParent(transform, false);
                droppedItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                droppedItem.transform.localScale = Vector3.one;
                currentItem.UpdateUI();
                AchievementManager.Instance?.OnSwordMerged(currentItem.level);

                StartCoroutine(PunchBounce(currentItem.transform));
                StartCoroutine(PunchBounce(droppedItem.transform));
            }
            else if (!fromEquipSlot && currentItem.level == droppedItem.level && currentItem.level < 8)
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
                AchievementManager.Instance?.OnSwordMerged(currentItem.level);

                // Phát âm thanh ghép kiếm
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayMergeSwordSFX();
                }

                StartCoroutine(PunchBounce(currentItem.transform));

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

    private IEnumerator PunchBounce(Transform target)
    {
        if (target == null) yield break;
        Vector3 orig = Vector3.one;
        float elapsed = 0f;
        float duration = 0.16f;
        while (elapsed < duration)
        {
            if (target == null) yield break;
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float s = 1f + Mathf.Sin(t * Mathf.PI) * 0.20f;
            target.localScale = orig * s;
            yield return null;
        }
        if (target != null) target.localScale = orig;
    }
}
