using UnityEngine;

/// <summary>Styles new inventory/reward rows on hierarchy change, without polling UI data.</summary>
[DisallowMultipleComponent]
public sealed class JadeUIStyleScope : MonoBehaviour
{
    [System.NonSerialized] public JadeUITheme theme;
    private void OnTransformChildrenChanged() { if (theme != null) theme.Queue(transform); }
    private void OnEnable() { if (theme != null) theme.Queue(transform); }
}
