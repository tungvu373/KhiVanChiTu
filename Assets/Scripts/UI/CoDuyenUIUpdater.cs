using UnityEngine;
using UnityEngine.UI;

public class CoDuyenUIUpdater : MonoBehaviour
{
    public Text txt;

    private void Start()
    {
        if (txt == null) txt = GetComponent<Text>();
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnCoDuyenChanged += UpdateUI;
            UpdateUI(EconomyManager.Instance.currentCoDuyen);
        }
    }

    private void OnDestroy()
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnCoDuyenChanged -= UpdateUI;
        }
    }

    private void UpdateUI(int coDuyen)
    {
        if (txt != null)
        {
            txt.text = $"Cơ Duyên: {coDuyen}";
        }
    }
}
