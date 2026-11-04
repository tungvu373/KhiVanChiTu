using UnityEngine;
using UnityEngine.UI;

/// <summary>Hiển thị tiền tệ gọn, có biểu tượng và số rút gọn trên HUD mobile.</summary>
public class MobileCurrencyText : MonoBehaviour
{
    public enum Currency { LinhThach, CoDuyen }
    public Currency currency;
    private Text label;

    private void Awake() => label = GetComponent<Text>();

    private void OnEnable()
    {
        if (EconomyManager.Instance == null) return;
        EconomyManager.Instance.OnLinhThachChanged += OnLinhThachChanged;
        EconomyManager.Instance.OnCoDuyenChanged += OnCoDuyenChanged;
        Refresh();
    }

    private void OnDisable()
    {
        if (EconomyManager.Instance == null) return;
        EconomyManager.Instance.OnLinhThachChanged -= OnLinhThachChanged;
        EconomyManager.Instance.OnCoDuyenChanged -= OnCoDuyenChanged;
    }

    private void OnLinhThachChanged(float _) { if (currency == Currency.LinhThach) Refresh(); }
    private void OnCoDuyenChanged(int _) { if (currency == Currency.CoDuyen) Refresh(); }

    private void Refresh()
    {
        if (label == null || EconomyManager.Instance == null) return;
        if (currency == Currency.LinhThach)
        {
            label.text = "◆ " + EconomyManager.FormatNumber(EconomyManager.Instance.currentLinhThach);
            label.color = new Color(0.55f, 0.88f, 1f);
        }
        else
        {
            label.text = "✦ " + EconomyManager.Instance.currentCoDuyen.ToString("N0");
            label.color = new Color(1f, 0.79f, 0.28f);
        }
    }
}
