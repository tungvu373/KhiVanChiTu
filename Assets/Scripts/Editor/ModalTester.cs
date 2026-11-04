using UnityEditor;
using UnityEngine;

public static class ModalTester
{
    [MenuItem("TuTien/Debug/Test Modal Open Directly")]
    public static void TestOpenDirectly()
    {
        var modal = Object.FindAnyObjectByType<MilestoneModalUI>(FindObjectsInactive.Include);
        if (modal == null) { Debug.LogError("No MilestoneModalUI found!"); return; }

        Debug.Log($"Before Open: gameObject.activeSelf={modal.gameObject.activeSelf}, activeInHierarchy={modal.gameObject.activeInHierarchy}, parent={(modal.transform.parent != null ? modal.transform.parent.name : "NULL")}, parentActive={modal.transform.parent?.gameObject.activeInHierarchy}");

        modal.Open();

        Debug.Log($"After Open: gameObject.activeSelf={modal.gameObject.activeSelf}, activeInHierarchy={modal.gameObject.activeInHierarchy}, IsModalOpen={modal.IsModalOpen}, modalPanel={modal.modalPanel?.name}");
    }
}

