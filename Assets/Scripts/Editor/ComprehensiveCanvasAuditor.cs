#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class ComprehensiveCanvasAuditor
{
    private static readonly Regex EmojiRegex = new Regex(
        @"(?:[\uD800-\uDBFF][\uDC00-\uDFFF]|[\u2600-\u27BF]|[\u25A0-\u25FF]|[\u2300-\u23FF]|[\u2B50-\u2B55]|[\u200D\uFE0F\u25C6\u2713\u2715\u2716\u274C\u2705])");

    public class AuditReport
    {
        public List<string> errors = new List<string>();
        public List<string> warnings = new List<string>();
        public List<string> notices = new List<string>();
        public List<string> redundantItems = new List<string>();
        public int totalButtons = 0;
        public int smallTouchTargets = 0;
        public int totalTexts = 0;
        public int textsWithEmoji = 0;
        public int totalImages = 0;
        public int unneededRaycasts = 0;
    }

    [MenuItem("TuTien/Audit/Run Full Mobile UI & Redundancy Audit")]
    public static void RunFullAudit()
    {
        var report = new AuditReport();
        var canvasGo = GameObject.Find("Canvas");
        if (canvasGo == null)
        {
            Debug.LogError("[AUDIT] Canvas not found in active scene!");
            return;
        }

        Debug.Log("====================================================================");
        Debug.Log("🚀 STARTING COMPREHENSIVE MOBILE UI & REDUNDANCY AUDIT (unity-mobile-ui)");
        Debug.Log("====================================================================");

        // 1. Kiểm tra cấu hình Canvas & CanvasScaler
        AuditCanvasSettings(canvasGo, report);

        // 2. Rà soát từng GameObject trong Canvas
        AuditCanvasHierarchy(canvasGo.transform, report);

        // 3. Rà soát danh sách Modals và khả năng chặn raycast
        AuditModals(canvasGo.transform, report);

        // 4. Rà soát các thành phần thừa trong Scene và Canvas (Task 5)
        AuditRedundantComponents(canvasGo, report);

        // 5. Tổng kết báo cáo
        PrintSummary(report);
    }

    private static void AuditCanvasSettings(GameObject canvasGo, AuditReport report)
    {
        var canvas = canvasGo.GetComponent<Canvas>();
        var scaler = canvasGo.GetComponent<CanvasScaler>();

        if (canvas == null)
        {
            report.errors.Add("Canvas GameObject thiếu component Canvas!");
            return;
        }

        if (scaler == null)
        {
            report.errors.Add("Canvas thiếu component CanvasScaler!");
            return;
        }

        if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
        {
            report.warnings.Add($"CanvasScaler ScaleMode là {scaler.uiScaleMode}, khuyến nghị: ScaleWithScreenSize");
        }

        if (scaler.referenceResolution.x != 1080 || scaler.referenceResolution.y != 1920)
        {
            report.notices.Add($"ReferenceResolution là {scaler.referenceResolution}, chuẩn dọc của dự án là 1080x1920.");
        }

        report.notices.Add($"Canvas RenderMode: {canvas.renderMode}, MatchWidthOrHeight: {scaler.matchWidthOrHeight:F2}");
    }

    private static void AuditCanvasHierarchy(Transform root, AuditReport report)
    {
        var allTransforms = root.GetComponentsInChildren<Transform>(true);
        foreach (var t in allTransforms)
        {
            // Bỏ qua các object thuộc panel prototype cũ đã biết
            if (IsKnownLegacyObject(t)) continue;

            // Kiểm tra Buttons & Touch Targets
            var btn = t.GetComponent<Button>();
            if (btn != null)
            {
                report.totalButtons++;
                var rt = t as RectTransform;
                if (rt != null)
                {
                    Vector2 size = rt.rect.size;
                    // Chuẩn 44-48 logical px trên 1080x1920 tương đương ~90-96 reference px
                    if (size.x > 0 && size.y > 0 && (size.x < 48f || size.y < 48f))
                    {
                        report.smallTouchTargets++;
                        report.warnings.Add($"[Touch Target Nhỏ <48px] '{GetPath(t)}' kích thước {size.x:F0}x{size.y:F0}px. Dễ bấm hụt trên mobile!");
                    }
                }
            }

            // Kiểm tra Text & Font & Emojis
            var txt = t.GetComponent<Text>();
            if (txt != null)
            {
                report.totalTexts++;
                if (txt.font == null)
                {
                    report.errors.Add($"[Mất Font] Text '{GetPath(t)}' không có Font được gán!");
                }

                if (!string.IsNullOrEmpty(txt.text) && EmojiRegex.IsMatch(txt.text))
                {
                    report.textsWithEmoji++;
                    report.warnings.Add($"[Ký Tự Lạ / Emoji] Text '{GetPath(t)}' chứa ký tự đặc biệt: \"{txt.text}\"");
                }
            }

            // Kiểm tra Image RaycastTarget lãng phí
            var img = t.GetComponent<Image>();
            if (img != null)
            {
                report.totalImages++;
                // Nếu là Image nhưng không có Button, Toggle, ScrollRect, EventTrigger mà vẫn bật raycastTarget
                bool isInteractive = t.GetComponent<Button>() != null || t.GetComponent<Toggle>() != null ||
                                     t.GetComponent<ScrollRect>() != null || t.GetComponent<InputField>() != null;
                if (!isInteractive && img.raycastTarget)
                {
                    // Chỉ cảnh báo nếu tên có vẻ là background/border/icon/decor
                    string lName = t.name.ToLower();
                    if (lName.Contains("bg") || lName.Contains("background") || lName.Contains("border") ||
                        lName.Contains("icon") || lName.Contains("frame") || lName.Contains("art"))
                    {
                        report.unneededRaycasts++;
                        report.notices.Add($"[Raycast Target Thừa] '{GetPath(t)}' là hình trang trí nhưng raycastTarget=true (lãng phí raycast cost).");
                    }
                }
            }
        }
    }

    private static void AuditModals(Transform canvasTr, AuditReport report)
    {
        string[] modalNames = {
            "DaoAnModal", "AchievementModalUI", "MilestoneModalUI", "StorageModalUI",
            "TribulationModalUI", "CharacterDetailModal", "PauseMenuModal", "DismantleConfirmModalUI"
        };

        foreach (var mName in modalNames)
        {
            var mTr = canvasTr.Find(mName);
            if (mTr == null)
            {
                report.warnings.Add($"[Modal Thiếu] Không tìm thấy '{mName}' trực tiếp dưới Canvas!");
                continue;
            }

            // Kiểm tra xem Modal có blocker raycast full màn hình không
            var img = mTr.GetComponent<Image>();
            if (img == null || !img.raycastTarget)
            {
                report.warnings.Add($"[Modal Lọt Click] '{mName}' thiếu Image raycastTarget=true ở root overlay để chặn click xuyên xuống đấu trường!");
            }

            // Kiểm tra nút Đóng
            bool hasClose = false;
            foreach (var b in mTr.GetComponentsInChildren<Button>(true))
            {
                string bName = b.name.ToLower();
                if (bName.Contains("close") || bName.Contains("btnx") || bName.Contains("btnclose") || bName.Contains("back"))
                {
                    hasClose = true;
                    break;
                }
            }
            if (!hasClose)
            {
                report.warnings.Add($"[Modal Thiếu Nút Đóng] '{mName}' không tìm thấy Button nào có tên Close/Back/X!");
            }
        }
    }

    private static void AuditRedundantComponents(GameObject canvasGo, AuditReport report)
    {
        // 1. Trong Canvas
        string[] legacyCanvasObjects = { "UIManager", "RightPanel", "ShopPanel", "FullBackground" };
        foreach (var lName in legacyCanvasObjects)
        {
            var tr = canvasGo.transform.Find(lName);
            if (tr != null)
            {
                report.redundantItems.Add($"[Canvas Thừa] Canvas/{lName} (activeSelf: {tr.gameObject.activeSelf}, components: {string.Join(", ", GetCompNames(tr))})");
            }
        }

        // 2. Trong Scene Root
        var scene = canvasGo.scene;
        foreach (var rootGo in scene.GetRootGameObjects())
        {
            if (rootGo.name == "GameManagers")
            {
                report.redundantItems.Add($"[Scene Root Trùng Lặp] '{rootGo.name}' chứa StageManager/VFXHelper, trùng lặp với '/Managers'.");
            }
            if (rootGo.name == "DaoAnModalUI")
            {
                report.redundantItems.Add($"[Scene Root Thừa] '{rootGo.name}' chứa DaoAnModalUI script rải rác ngoài Canvas.");
            }
        }
    }

    private static string[] GetCompNames(Transform tr)
    {
        var list = new List<string>();
        foreach (var c in tr.GetComponents<Component>())
        {
            if (c != null) list.Add(c.GetType().Name);
        }
        return list.ToArray();
    }

    private static bool IsKnownLegacyObject(Transform t)
    {
        string path = GetPath(t);
        return path.Contains("RightPanel") || path.Contains("ShopPanel") || path.Contains("FullBackground");
    }

    private static string GetPath(Transform t)
    {
        string p = t.name;
        var cur = t.parent;
        while (cur != null && cur.name != "Canvas")
        {
            p = cur.name + "/" + p;
            cur = cur.parent;
        }
        return p;
    }

    private static void PrintSummary(AuditReport report)
    {
        Debug.Log("--------------------------------------------------------------------");
        Debug.Log($"📊 BÁO CÁO TỔNG QUAN AUDIT UI:");
        Debug.Log($"   - Tổng Buttons: {report.totalButtons} | Nút nhỏ (<48px): {report.smallTouchTargets}");
        Debug.Log($"   - Tổng Texts: {report.totalTexts} | Chứa Emoji/Ký tự lạ: {report.textsWithEmoji}");
        Debug.Log($"   - Tổng Images: {report.totalImages} | RaycastTarget thừa: {report.unneededRaycasts}");
        Debug.Log($"   - LỖI ĐỎ (Errors): {report.errors.Count}");
        Debug.Log($"   - CẢNH BÁO (Warnings): {report.warnings.Count}");
        Debug.Log($"   - THÀNH PHẦN THỪA (Redundant): {report.redundantItems.Count}");
        Debug.Log("--------------------------------------------------------------------");

        if (report.errors.Count > 0)
        {
            Debug.LogError($"🔴 CÁC LỖI ĐỎ CẦN FIX NGAY ({report.errors.Count}):\n" + string.Join("\n", report.errors));
        }

        if (report.warnings.Count > 0)
        {
            Debug.LogWarning($"🟡 CÁC CẢNH BÁO CẦN LƯU Ý ({report.warnings.Count}):\n" + string.Join("\n", report.warnings));
        }

        if (report.redundantItems.Count > 0)
        {
            Debug.Log($"📦 DANH SÁCH THÀNH PHẦN THỪA CẦN DUYỆT XÓA ({report.redundantItems.Count}):\n" + string.Join("\n", report.redundantItems));
        }
        Debug.Log("====================================================================");
    }
}
#endif

