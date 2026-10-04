using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    public int itemIndex;
    public Button buyButton;

    private void OnEnable()
    {
        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyClicked);
        }
    }

    private void OnBuyClicked()
    {
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.BuyItem(itemIndex);
        }
    }
}
