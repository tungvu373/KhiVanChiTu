using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class CheckCanvasScalerTool
{
    [MenuItem("Tools/Check Canvas Scaler")]
    public static void Check()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) return;

        var cs = canvas.GetComponent<CanvasScaler>();
        if (cs != null)
        {
            Debug.Log($"CanvasScaler: mode={cs.uiScaleMode}, refRes={cs.referenceResolution}, matchWidthOrHeight={cs.matchWidthOrHeight}, screenMatchMode={cs.screenMatchMode}");
        }

        var rt = canvas.GetComponent<RectTransform>();
        Debug.Log($"Canvas RectTransform: size={rt.rect.size}, scale={rt.localScale}");

        var modal = canvas.transform.Find("DaoAnModal");
        if (modal != null)
        {
            var mrt = modal.GetComponent<RectTransform>();
            Debug.Log($"DaoAnModal RT: anchors=({mrt.anchorMin},{mrt.anchorMax}), offset=({mrt.offsetMin},{mrt.offsetMax}), size={mrt.rect.size}, scale={mrt.localScale}, pos={mrt.anchoredPosition}");

            var box = modal.Find("ModalBox");
            if (box != null)
            {
                var brt = box.GetComponent<RectTransform>();
                Debug.Log($"ModalBox RT: anchors=({brt.anchorMin},{brt.anchorMax}), offset=({brt.offsetMin},{brt.offsetMax}), size={brt.rect.size}, scale={brt.localScale}, pos={brt.anchoredPosition}");
            }
        }
    }
}

