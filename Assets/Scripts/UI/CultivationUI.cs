using UnityEngine;
using UnityEngine.UI;

public class CultivationUI : MonoBehaviour
{
    public Text txtStageName;
    public Text txtTuVi;
    public Image imgProgressBar;

    private void Start()
    {
        if (CultivationManager.Instance != null)
        {
            CultivationManager.Instance.OnTuViChanged += UpdateTuViUI;
            CultivationManager.Instance.OnStageChanged += UpdateStageUI;
            var stage = CultivationManager.Instance.GetCurrentStageData();
            if (stage != null)
            {
                UpdateStageUI(stage);
                UpdateTuViUI(CultivationManager.Instance.currentTuVi, stage.requiredTuVi);
            }
        }
    }

    private void OnDestroy()
    {
        if (CultivationManager.Instance != null)
        {
            CultivationManager.Instance.OnTuViChanged -= UpdateTuViUI;
            CultivationManager.Instance.OnStageChanged -= UpdateStageUI;
        }
    }

    private void UpdateTuViUI(float current, float max)
    {
        if (txtTuVi != null) txtTuVi.text = $"Tu Vi: {EconomyManager.FormatNumber(current)} / {EconomyManager.FormatNumber(max)}";
        if (imgProgressBar != null && CultivationManager.Instance != null)
            imgProgressBar.fillAmount = CultivationDisplay.PhaseProgress(
                CultivationManager.Instance.currentStageIndex, max > 0 ? current / max : 0f);
    }

    private void UpdateStageUI(CultivationStageData stageData)
    {
        if (txtStageName != null && CultivationManager.Instance != null)
            txtStageName.text = $"Cảnh giới: {CultivationManager.Instance.GetDisplayRealmName()}";
    }

    private void Update()
    {
#if UNITY_EDITOR
        // CHEAT TEST
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            // Bấm T để tăng 50 Tu Vi
            if (UnityEngine.InputSystem.Keyboard.current.tKey.wasPressedThisFrame)
            {
                if (CultivationManager.Instance != null) CultivationManager.Instance.AddTuVi(50);
            }
            
            // Bấm B để thắng Boss Lôi Kiếp và đột phá
            if (UnityEngine.InputSystem.Keyboard.current.bKey.wasPressedThisFrame)
            {
                if (GameManager.Instance != null && GameManager.Instance.currentState == GameManager.GameState.BossTribulation)
                {
                    if (CultivationManager.Instance != null) CultivationManager.Instance.Breakthrough();
                }
            }
        }
#endif
    }
}
