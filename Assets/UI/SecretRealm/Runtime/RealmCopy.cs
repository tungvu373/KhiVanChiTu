using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using KhiVanChiTu.UI.Foundation;
using KhiVanChiTu.UI.Equipment;
using KhiVanChiTu.UI.Navigation;

namespace KhiVanChiTu.UI.SecretRealm
{
    public static class RealmCopy
    {
        public static readonly string[] Names={"Vạn Ma Triều Dâng","Trảm Phá Cấm Chế","Đan Thần Kỳ Duyên","Vượt Lôi Trì","Kiếm Chủng Cổ Thần"};
        public static readonly string[] Modes={"TRẢM MA · 30 GIÂY","PHÁ ẤN · 20 GIÂY","TAM ĐỈNH TẦM BẢO","THU THẬP · 25 GIÂY","RÚT KIẾM · 3 LƯỢT"};
        public static readonly string[] Descriptions={"Yêu ma vây kín động thiên. Triệu vạn kiếm, quét sạch ma triều.","Thạch bi viễn cổ đang thức tỉnh. Dồn kiếm lực phá tan phù ấn.","Lắng nghe sấm ngôn. Chọn một trong ba đỉnh, tìm duyên lành.","Giữa lôi trận thượng cổ, nhanh tay thu thập những viên lôi tinh.","Cổ kiếm chờ người hữu duyên. Canh kim chỉ thị vào vùng hoàng kim."};
        public static readonly string[] Rewards={"Thảo dược · Linh thạch\nTừ 40 yêu ma: thêm Cơ Duyên","Quặng · Linh thạch\nPhá hết cấm chế: thêm quặng","Thảo dược · Linh thạch\nChọn đúng đỉnh: thêm Cơ Duyên","Tu vi · Linh thạch\nTừ 12 lôi tinh: thêm Cơ Duyên","Phi kiếm cấp 4 / 6 / 8\nLinh thạch theo cấp kiếm"};
        public static readonly string[] Objectives={"Trảm đủ 50 yêu ma trước khi hết 30 giây.","Phá thạch bi 1.000.000 HP trong 20 giây.","","Thu thập đủ 20 lôi tinh trong 25 giây.","Canh kim vào vùng vàng để rút kiếm cấp 8."};
        public static readonly string[] Hints={"Chạm yêu ma · Tuyệt kỹ quét tối đa 5 mục tiêu","Chạm Khai Thiên Trảm để gây thêm 50.000 sát thương","Đọc sấm ngôn rồi chạm đỉnh tương ứng","Chạm lôi tinh phát sáng · Chú ý vòng sấm báo trước","Vùng vàng: cấp 8 · Hai bên: cấp 6 · Ngoài rìa: cấp 4"};
        public static string Plain(string value)=>Regex.Replace(value??"",@"<[^>]*>","").Replace("🎯","").Replace("👉","").Replace("📜","").Replace("⏱️","").Replace("⚔️","").Replace("⭐","").Replace("💥","").Replace("⚡","").Replace("🗡️","").Trim();
    }
}
