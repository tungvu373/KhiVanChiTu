using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    public int itemIndex;
    public Button buyButton;

    private Text priceText;

    private void OnEnable()
    {
        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyClicked);
            priceText = buyButton.GetComponentInChildren<Text>();
        }
    }

    private void Update()
    {
        if (priceText != null && CultivationManager.Instance != null)
        {
            int stage = CultivationManager.Instance.currentStageIndex;
            float baseCost = (itemIndex == 4) ? 1000f : 500f;
            float cost = baseCost * Mathf.Pow(1.2f, stage);
            
            priceText.text = $"{Mathf.CeilToInt(cost)} LT";
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
