using UnityEngine;
using UnityEngine.UI;

public class ShopToggleUI : MonoBehaviour
{
    public GameObject shopPanel;
    public bool isCloseButton = false;

    private void OnEnable()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                if (shopPanel != null)
                {
                    shopPanel.SetActive(!isCloseButton);
                }
                else
                {
                    GameObject p = GameObject.Find("Canvas")?.transform.Find("ShopPanel")?.gameObject;
                    if (p != null) p.SetActive(!isCloseButton);
                }
            });
        }
    }
}
