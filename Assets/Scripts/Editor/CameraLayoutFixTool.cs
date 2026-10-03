#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class CameraLayoutFixTool
{
    [MenuItem("TuTien/Giai đoạn 8/2. Chỉnh Camera chia đôi màn hình (Combat Trái - UI Phải)")]
    public static void FixCameraLayout()
    {
        // 1. Cắt vùng Render của Camera 3D chỉ hiển thị đúng 50% bên Trái màn hình
        if (Camera.main != null)
        {
            Camera.main.rect = new Rect(0, 0, 0.5f, 1f);
            
            // Đặt Camera nhìn thẳng xuống tâm một cách cân đối nhất
            Camera.main.transform.position = new Vector3(0f, 15f, -12f);
            Camera.main.transform.rotation = Quaternion.Euler(55f, 0, 0);
        }

        // 2. Kéo Player về chính giữa tâm (0,0,0) của vùng 3D
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            player.transform.position = Vector3.zero;
        }

        // 3. Căn chỉnh lại Đài Tu Luyện (Mặt đất) thu gọn vào chính giữa
        GameObject ground = GameObject.Find("TuTienGround");
        if (ground != null)
        {
            ground.transform.position = new Vector3(0, -1.5f, 0);
            ground.transform.localScale = new Vector3(22f, 0.5f, 22f); // Thu gọn lại cho vừa vặn góc trái
        }

        // 4. Kéo Sương mù (Linh Khí) về chính giữa
        GameObject mist = GameObject.Find("LingQiVFX");
        if (mist != null)
        {
            mist.transform.position = new Vector3(0, -0.5f, 0);
        }

        // 5. Đổ màu đặc (Opaque) cho Bảng UI bên Phải để làm vách ngăn hoàn hảo
        GameObject rightPanel = GameObject.Find("RightPanel");
        if (rightPanel != null)
        {
            Image img = rightPanel.GetComponent<Image>();
            if (img == null) img = rightPanel.AddComponent<Image>();
            img.color = new Color(0.12f, 0.12f, 0.15f, 1f); // Màu xám đen, đậm đặc, không trong suốt
        }

        Debug.Log("✅ Đã chia đôi màn hình cực chuẩn: Combat 3D bên Trái, Bảng điều khiển UI bên Phải!");
    }
}
#endif
