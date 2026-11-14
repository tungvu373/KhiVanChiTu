using System;
using UnityEngine;

public readonly struct MobileUIActionSnapshot
{
    public readonly string Caption, Value, DisabledReason;
    public readonly bool CanExecute;
    public MobileUIActionSnapshot(string caption, string value, bool canExecute, string disabledReason = "")
    { Caption = caption; Value = value; CanExecute = canExecute; DisabledReason = disabledReason; }
}

/// <summary>The game/service adapter owns validation and commands; the view only displays this snapshot.</summary>
public interface IMobileUIActionSource
{
    event Action Changed;
    MobileUIActionSnapshot Read();
    void Execute();
}

public sealed class MobileUIActionView : MonoBehaviour
{
    public UnityEngine.UI.Button button;
    public UnityEngine.UI.Text caption, value, disabledReason;
    IMobileUIActionSource source;
    bool listening;
    // Unbound components on migrated buttons leave every existing listener/interactable value intact.
    public void Bind(IMobileUIActionSource next)
    {
        Detach(); source = next;
        if (isActiveAndEnabled) Attach();
    }
    public void Unbind() { Detach(); source = null; }
    void OnEnable() { Attach(); }
    void OnDisable() { Detach(); }
    void OnDestroy() { Detach(); }
    void Attach()
    {
        if (source == null || listening) return;
        listening = true; source.Changed += Refresh;
        if (button != null) button.onClick.AddListener(Execute);
        Refresh();
    }
    void Detach()
    {
        if (!listening) return;
        source.Changed -= Refresh;
        if (button != null) button.onClick.RemoveListener(Execute);
        listening = false;
    }
    public void Refresh()
    {
        if (source == null) return;
        var state = source.Read();
        if (caption != null) caption.text = state.Caption ?? "";
        if (value != null) value.text = state.Value ?? "";
        if (disabledReason != null) disabledReason.text = state.DisabledReason ?? "";
        if (button != null) button.interactable = state.CanExecute;
    }
    void Execute()
    {
        if (source != null && source.Read().CanExecute) source.Execute();
    }
}
