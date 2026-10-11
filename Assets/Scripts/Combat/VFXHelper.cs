using UnityEngine;

public class VFXHelper : MonoBehaviour
{
    public static VFXHelper Instance { get; private set; }

    [Header("Sprites")]
    public Sprite linhThachSprite; // Kéo thả ảnh 2D vào đây

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void SpawnFlyingLoot(Vector3 startWorldPos, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject loot;
            if (linhThachSprite != null)
            {
                // Dùng Sprite 2D
                loot = new GameObject("FlyingLinhThach_Sprite");
                SpriteRenderer sr = loot.AddComponent<SpriteRenderer>();
                sr.sprite = linhThachSprite;
                loot.transform.localScale = Vector3.one * 0.4f; // Chỉnh nhỏ đi một nửa theo yêu cầu
            }
            else
            {
                // Fallback: Dùng khối Sphere 3D
                loot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                loot.transform.localScale = Vector3.one * 0.3f;
                Destroy(loot.GetComponent<Collider>());
                
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = Color.cyan;
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", Color.cyan * 1.5f);
                loot.GetComponent<Renderer>().material = mat;
            }

            loot.transform.position = startWorldPos + Random.insideUnitSphere * 1.5f;

            // Script bay về màn hình
            FlyingLoot fl = loot.AddComponent<FlyingLoot>();
            fl.Init(startWorldPos, amount);
        }
    }
}

public class FlyingLoot : MonoBehaviour
{
    private Vector3 targetScreenPos;
    private Camera mainCam;
    private float flyTimer = 0f;
    private float maxFlyTime = 1.2f;
    private Vector3 initialScale;

    public void Init(Vector3 start, int amount)
    {
        mainCam = Camera.main;
        
        // Random ra xíu cho nó bung ra trước khi bay
        GetComponent<Rigidbody>(); // Có thể add Rigidbody bắn ra, nhưng ở đây dùng toán cho nhẹ
        
        // Lưu lại scale ban đầu để thu nhỏ dần
        initialScale = transform.localScale;
        
        // Target: Giả sử icon vàng nằm ở góc trên bên trái UI
        // Nâng Z = 20f (như bạn đề xuất) để nó cách xa hẳn ra
        targetScreenPos = new Vector3(Screen.width * 0.1f, Screen.height * 0.9f, 20f); 
    }

    private void Update()
    {
        flyTimer += Time.deltaTime;
        float t = flyTimer / maxFlyTime;
        
        if (t > 1f)
        {
            Destroy(gameObject);
            return;
        }

        // Điểm đầu
        Vector3 currentPos = transform.position;
        // Điểm đích (chuyển đổi UI Screen sang World)
        Vector3 targetWorldPos = mainCam.ScreenToWorldPoint(targetScreenPos);
        
        // Curve animation (bay lên rồi hút về)
        Vector3 lerpedPos = Vector3.Lerp(currentPos, targetWorldPos, t * t); // Accelerate
        transform.position = lerpedPos;

        // Tạo hiệu ứng thu nhỏ dần về 0 để tan biến hẳn vào UI
        transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);

        // Billboarding: Giữ cho Sprite 2D luôn đối diện hướng nhìn của Camera
        if (mainCam != null)
        {
            transform.LookAt(transform.position + mainCam.transform.rotation * Vector3.forward, mainCam.transform.rotation * Vector3.up);
        }
    }
}
