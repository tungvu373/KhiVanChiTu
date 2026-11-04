using UnityEngine;

public class OpeningBossGuard : MonoBehaviour
{
    public float Shield { get; private set; }
    public float MaximumShield { get; private set; }
    private TextMesh label;
    public void ResetGuard(float amount)
    {
        Shield = MaximumShield = Mathf.Max(0, amount);
        if (label == null)
        {
            var go = new GameObject("OpeningBossShieldLabel");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = Vector3.up * 3;
            label = go.AddComponent<TextMesh>();
            label.characterSize = 0.15f; label.fontSize = 36;
            label.anchor = TextAnchor.MiddleCenter; label.color = Color.cyan;
        }
        RefreshLabel();
        label.gameObject.SetActive(MaximumShield > 0);
    }
    public float Absorb(float amount)
    {
        float absorbed = Mathf.Min(Shield, Mathf.Max(0, amount));
        Shield -= absorbed;
        RefreshLabel();
        return Mathf.Max(0, amount - absorbed);
    }
    private void RefreshLabel() { if (label != null) label.text = Shield > 0 ? "HỘ THỂ " + Mathf.CeilToInt(Shield) : "HỘ THỂ ĐÃ VỠ"; }
    private void LateUpdate() { if (label != null && Camera.main != null) label.transform.rotation = Camera.main.transform.rotation; }
}
