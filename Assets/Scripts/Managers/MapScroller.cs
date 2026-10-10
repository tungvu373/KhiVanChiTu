using UnityEngine;
using System.Collections.Generic;

public class MapScroller : MonoBehaviour
{
    [Header("Cài đặt Cuốn Chiếu (Treadmill)")]
    public float scrollSpeed = 5f; // Tốc độ trôi của map (tốc độ đi bộ của nhân vật)
    public bool isMoving = true;   // Khi nào đánh quái thì set false để dừng lại

    [Header("Map Chunks (Các block map)")]
    public List<Transform> mapChunks; // Bỏ 3 chunk map giống nhau vào đây
    public float chunkLength = 20f;   // Chiều dài của mỗi chunk

    public static MapScroller Instance { get; private set; }

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
        // Tự động nhận diện kích thước nếu người dùng thay chunk map bằng Unity Terrain
        if (mapChunks != null && mapChunks.Count > 0 && mapChunks[0] != null)
        {
            Terrain terrain = mapChunks[0].GetComponent<Terrain>();
            if (terrain != null && terrain.terrainData != null)
            {
                chunkLength = terrain.terrainData.size.z;
                Debug.Log("[MapScroller] Đã tự động cập nhật chunkLength theo kích thước Terrain: " + chunkLength);

                // Tự động căn chỉnh vị trí nối đuôi nhau dựa trên chunk đầu tiên (giữ nguyên offset ban đầu)
                if (mapChunks.Count > 0 && mapChunks[0] != null)
                {
                    float startZ = mapChunks[0].position.z;
                    for (int i = 0; i < mapChunks.Count; i++)
                    {
                        if (mapChunks[i] != null)
                        {
                            Vector3 pos = mapChunks[i].position;
                            // Giữ nguyên X và Y, chỉ sắp xếp lại trục Z dựa theo chunk đầu tiên
                            pos.z = startZ + (i * chunkLength);
                            mapChunks[i].position = pos;
                        }
                    }
                }
            }
        }
    }

    void Update()
    {
        if (!isMoving) return;

        foreach (Transform chunk in mapChunks)
        {
            // Di chuyển chunk về phía sau (ngược chiều nhân vật nhìn)
            chunk.Translate(Vector3.back * scrollSpeed * Time.deltaTime, Space.World);

            // Nếu chunk trôi qua khỏi camera (phía sau nhân vật)
            if (chunk.position.z < -chunkLength)
            {
                // Dịch chuyển nó lên tít phía trước để tái sử dụng
                Vector3 newPos = chunk.position;
                newPos.z += chunkLength * mapChunks.Count;
                chunk.position = newPos;
            }
        }
    }

    // Gọi hàm này khi gặp quái để dừng map lại
    public void StopMoving()
    {
        isMoving = false;
        // Gắn logic đổi Animation nhân vật sang trạng thái Idle/Combat ở đây
    }

    // Gọi hàm này khi quái chết để đi tiếp
    public void ResumeMoving()
    {
        isMoving = true;
        // Gắn logic đổi Animation nhân vật sang trạng thái Run/Walk ở đây
    }

    public void RandomizeBiomeColor()
    {
        // Simple color tinting for terrains or meshes
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
