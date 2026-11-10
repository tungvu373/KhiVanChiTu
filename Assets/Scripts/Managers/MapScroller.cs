using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Quản lý hiệu ứng cuốn chiếu bản đồ vô tận (Infinite Seamless Treadmill).
/// Khắc phục triệt để lỗi hẫng bản đồ / khoảng trống vô định khi chạy lâu:
/// 1. Tự động đảm bảo tối thiểu 2 chunk địa hình nối đuôi nhau liên tục (Dual-Chunk Loop).
/// 2. Cơ chế cuốn chiếu động (Seamless Snapping): Khi một chunk trôi hoàn toàn ra sau lưng Camera,
///    nó sẽ lập tức bắt nối tiếp vào chunk xa nhất phía trước (maxZ + chunkLength), triệt tiêu hoàn toàn khe hở (Zero Seam).
/// </summary>
public class MapScroller : MonoBehaviour
{
    [Header("Cài đặt Cuốn Chiếu (Treadmill)")]
    public float scrollSpeed = 5f; // Tốc độ trôi của map (tốc độ đi bộ của nhân vật)
    public bool isMoving = true;   // Khi nào đánh quái thì set false để dừng lại

    [Header("Map Chunks (Các block map)")]
    public List<Transform> mapChunks; // Tối thiểu 2 chunk để đảm bảo cuộn vô tận không gián đoạn
    public float chunkLength = 1000f; // Chiều dài của mỗi chunk

    public static MapScroller Instance { get; private set; }

    private Camera cachedCamera;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        cachedCamera = Camera.main;

        if (mapChunks == null)
        {
            mapChunks = new List<Transform>();
        }
        mapChunks.RemoveAll(c => c == null);

        // 1. Tự động nhận diện kích thước Terrain nếu có
        if (mapChunks.Count > 0 && mapChunks[0] != null)
        {
            Terrain terrain = mapChunks[0].GetComponent<Terrain>();
            if (terrain != null && terrain.terrainData != null)
            {
                chunkLength = terrain.terrainData.size.z;
                Debug.Log($"[MapScroller] Nhận diện chunkLength theo Terrain: {chunkLength}m");
            }
        }

        // 2. BẢO ĐẢM VÔ TẬN: Nếu chỉ có 1 chunk duy nhất trong Scene, tự động Clone chunk thứ 2
        // để tạo vòng lặp vô tận cuốn chiếu (không bao giờ lộ khoảng trống hư không)
        if (mapChunks.Count == 1 && mapChunks[0] != null)
        {
            GameObject origObj = mapChunks[0].gameObject;
            GameObject cloneObj = Instantiate(origObj, origObj.transform.parent);
            cloneObj.name = origObj.name + "_LoopClone";
            
            // Chia sẻ chung asset TerrainData để không tốn thêm bộ nhớ
            Terrain origTerrain = origObj.GetComponent<Terrain>();
            Terrain cloneTerrain = cloneObj.GetComponent<Terrain>();
            if (origTerrain != null && cloneTerrain != null)
            {
                cloneTerrain.terrainData = origTerrain.terrainData;
            }

            Vector3 clonePos = mapChunks[0].position;
            clonePos.z += chunkLength;
            cloneObj.transform.position = clonePos;

            mapChunks.Add(cloneObj.transform);
            Debug.Log($"[MapScroller] Đã tự động tạo Terrain Chunk thứ 2 tại Z = {clonePos.z} để cuộn vô tận không vết ghép.");
        }

        // 3. Sắp xếp lại toàn bộ các chunk theo thứ tự nối đuôi liên tục
        if (mapChunks.Count > 0 && mapChunks[0] != null)
        {
            float startZ = mapChunks[0].position.z;
            for (int i = 0; i < mapChunks.Count; i++)
            {
                if (mapChunks[i] != null)
                {
                    Vector3 pos = mapChunks[i].position;
                    pos.z = startZ + (i * chunkLength);
                    mapChunks[i].position = pos;
                }
            }
        }
    }

    void Update()
    {
        if (!isMoving || mapChunks == null || mapChunks.Count == 0) return;

        float delta = scrollSpeed * Time.deltaTime;
        float camZ = cachedCamera != null ? cachedCamera.transform.position.z : 0f;

        // 1. Di chuyển toàn bộ các chunk về phía sau theo tốc độ bước chân
        for (int i = 0; i < mapChunks.Count; i++)
        {
            if (mapChunks[i] != null)
            {
                mapChunks[i].Translate(Vector3.back * delta, Space.World);
            }
        }

        // 2. Kiểm tra tái sinh chunk (Recycle) khi mép trước của chunk đã trôi hẳn ra sau lưng Camera
        for (int i = 0; i < mapChunks.Count; i++)
        {
            Transform chunk = mapChunks[i];
            if (chunk == null) continue;

            // Mép trước của chunk địa hình nằm tại (chunk.position.z + chunkLength)
            float frontEdgeZ = chunk.position.z + chunkLength;

            // Khi toàn bộ chunk đã vượt qua sau lưng camera ít nhất 30 mét
            if (frontEdgeZ < camZ - 30f)
            {
                // Tìm vị trí chunk xa nhất ở phía trước
                float maxZ = float.MinValue;
                for (int j = 0; j < mapChunks.Count; j++)
                {
                    if (mapChunks[j] != null && mapChunks[j] != chunk)
                    {
                        if (mapChunks[j].position.z > maxZ)
                        {
                            maxZ = mapChunks[j].position.z;
                        }
                    }
                }

                if (maxZ > float.MinValue)
                {
                    Vector3 newPos = chunk.position;
                    // Bắt khớp chính xác vào mép trước của chunk xa nhất
                    newPos.z = maxZ + chunkLength;
                    chunk.position = newPos;
                }
            }
        }
    }

    // Gọi hàm này khi gặp quái/Boss để dừng map lại
    public void StopMoving()
    {
        isMoving = false;
        if (CombatManager.Instance != null && CombatManager.Instance.playerAnimator != null)
        {
            CombatManager.Instance.playerAnimator.SetBool("IsMoving", false);
        }
    }

    // Gọi hàm này khi dọn xong để tiếp tục hành trình
    public void ResumeMoving()
    {
        isMoving = true;
        if (CombatManager.Instance != null && CombatManager.Instance.playerAnimator != null)
        {
            CombatManager.Instance.playerAnimator.SetBool("IsMoving", true);
        }
    }

    public void StartMoving()
    {
        ResumeMoving();
    }

    public void RandomizeBiomeColor()
    {
        Color randomColor = new Color(Random.Range(0.6f, 1f), Random.Range(0.6f, 1f), Random.Range(0.6f, 1f));
        foreach (Transform chunk in mapChunks)
        {
            if (chunk == null) continue;
            Renderer r = chunk.GetComponentInChildren<Renderer>();
            if (r != null && r.material != null)
            {
                r.material.color = randomColor;
            }
        }
    }
}
