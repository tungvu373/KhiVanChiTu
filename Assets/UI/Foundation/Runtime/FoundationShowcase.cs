using System;
using TMPro;
using UnityEngine;

namespace KhiVanChiTu.UI.Foundation
{
    /// <summary>Disposable local demo state. No managers, PlayerPrefs or save APIs.</summary>
    public sealed class FoundationShowcase : MonoBehaviour
    {
        public FoundationRoot root;
        public FoundationAction primary, secondary, header, openScreen, openSheet, openReward, openConfirm, showToast, help;
        public FoundationValue resource, stat;
        public FoundationItem slot, card;
        public FoundationProgress progress;
        public FoundationTabs tabs;
        public FoundationPanel fullScreen, sheet, reward, confirm;
        public FoundationNotice toast, tooltip;
        public TMP_Text status;
        public Sprite sampleSword;
        public int DemoValue { get; private set; } = 72;
        void Start() { Wire(); }
        public void Wire()
        {
            primary.Bind("T\u0103ng ti\u1ebfn \u0111\u1ed9", () => { DemoValue = (DemoValue + 14) % 101; Refresh(); });
            secondary.Bind("Kh\u00f4i ph\u1ee5c m\u1eabu", () => { DemoValue = 72; Refresh(); });
            header.Bind("\u2039   T\u00e0ng Kinh C\u00e1c", () => toast.Show("\u0110\u00e2y l\u00e0 m\u00e0n h\u00ecnh th\u1eed nghi\u1ec7m UI."));
            openScreen.Bind("FullScreenPanel", () => fullScreen.Show("T\u00e0ng Kinh C\u00e1c", LongCopy, () => toast.Show("\u0110\u00e3 \u0111\u1ecdc")));
            openSheet.Bind("BottomSheet", () => sheet.Show("Chi ti\u1ebft ph\u00e1p kh\u00ed", LongCopy, () => toast.Show("\u0110\u00e3 ch\u1ecdn")));
            openReward.Bind("RewardModal", () => reward.Show("C\u01a1 duy\u00ean h\u1ed9i t\u1ee5", "Linh th\u1ea1ch +120\n\u0110\u00e2y l\u00e0 d\u1eef li\u1ec7u m\u1eabu, kh\u00f4ng ghi v\u00e0o nh\u00e2n v\u1eadt.", () => { DemoValue = 100; Refresh(); toast.Show("\u0110\u00e3 nh\u1eadn ph\u1ea7n th\u01b0\u1edfng m\u1eabu"); }));
            openConfirm.Bind("ConfirmDialog", () => confirm.Show("X\u00e1c nh\u1eadn thao t\u00e1c", "Kh\u00f4i ph\u1ee5c ti\u1ebfn \u0111\u1ed9 m\u1eabu v\u1ec1 0%?", () => { DemoValue = 0; Refresh(); }));
            showToast.Bind("Hi\u1ec7n Toast", () => toast.Show("Linh kh\u00ed \u0111\u00e3 h\u1ed9i t\u1ee5 \u00b7 +120"));
            help.Bind("Gi\u1eef \u0111\u1ec3 xem Tooltip", () => { });
            var trigger = help.GetComponent<FoundationTooltipTrigger>() ?? help.gameObject.AddComponent<FoundationTooltipTrigger>();
            trigger.tooltip = tooltip; trigger.message = "Ch\u1ea1m v\u00e0 gi\u1eef \u0111\u1ec3 \u0111\u1ecdc. Th\u1ea3 tay \u0111\u1ec3 \u0111\u00f3ng.";
            slot.Bind("Thanh V\u00e2n", "", sampleSword, "12", () => toast.Show("\u0110\u00e3 ch\u1ecdn \u00f4 ki\u1ebfm"));
            card.Bind("Thanh V\u00e2n Ki\u1ebfm", "Ph\u00e1p kh\u00ed \u00b7 C\u1ea5p 12", sampleSword, "12", () => toast.Show("\u0110\u00e3 ch\u1ecdn ph\u00e1p kh\u00ed"));
            tabs.selected.RemoveListener(OnTab); tabs.selected.AddListener(OnTab); tabs.Initialize(); Refresh();
        }
        void OnTab(int index) { if (status != null) status.text = "Tab " + (index + 1) + " / 3  \u00b7  UI Foundation"; }
        void Refresh()
        {
            progress.Set(DemoValue, 100, "Tu vi  " + DemoValue + " / 100");
            resource.Set("Linh th\u1ea1ch", (1234567 + DemoValue).ToString("N0"));
            stat.Set("C\u01b0\u1eddng \u0111\u1ed9", DemoValue + "  \u2192  " + (DemoValue + 14));
        }
        public const string LongCopy = "Kh\u00ed v\u1eadn h\u1ed9i t\u1ee5, v\u1ea1n ki\u1ebfm quy t\u00f4ng.\n\nN\u1ed9i dung ti\u1ebfng Vi\u1ec7t c\u00f3 \u0111\u1ea7y \u0111\u1ee7 d\u1ea5u: \u0103 \u00e2 \u0111 \u00ea \u00f4 \u01a1 \u01b0.\n\nM\u1ed7i ph\u00e1p kh\u00ed \u0111\u1ec1u mang m\u1ed9t c\u00e2u chuy\u1ec7n. H\u00e3y cu\u1ed9n \u0111\u1ec3 \u0111\u1ecdc n\u1ed9i dung d\u00e0i v\u00e0 ki\u1ec3m tra footer c\u1ed1 \u0111\u1ecbnh.\n\nT\u00ean nh\u00e2n v\u1eadt: V\u0169 Kh\u00e1nh T\u00f9ng.\n\nT\u00e0i nguy\u00ean: 999.999.999.999 linh th\u1ea1ch.\n\nKh\u00f4ng c\u00f3 thay \u0111\u1ed5i n\u00e0o v\u1edbi gameplay ho\u1eb7c ti\u1ebfn tr\u00ecnh nh\u00e2n v\u1eadt trong m\u00e0n h\u00ecnh n\u00e0y.";
    }
}
