using System.Collections.Generic;
using UnityEngine;

public class VFXHelper : MonoBehaviour
{
    public static VFXHelper Instance { get; private set; }

    [Header("Sprites")]
    public Sprite linhThachSprite; // Kéo thả ảnh 2D vào đây

    private readonly Stack<FlyingLoot> lootPool = new Stack<FlyingLoot>();
    private Material cachedFallbackMaterial;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (cachedFallbackMaterial != null)
        {
            Destroy(cachedFallbackMaterial);
        }
    }

    public void SpawnFlyingLoot(Vector3 startWorldPos, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            FlyingLoot fl = GetOrCreateLoot();
            fl.transform.position = startWorldPos + Random.insideUnitSphere * 1.5f;
            fl.gameObject.SetActive(true);
            fl.Init(startWorldPos, amount, ReturnLootToPool);
        }
    }

    private FlyingLoot GetOrCreateLoot()
    {
        while (lootPool.Count > 0)
        {
            FlyingLoot candidate = lootPool.Pop();
            if (candidate != null) return candidate;
        }

        GameObject loot;
        if (linhThachSprite != null)
        {
            loot = new GameObject("FlyingLinhThach_Sprite");
            SpriteRenderer sr = loot.AddComponent<SpriteRenderer>();
            sr.sprite = linhThachSprite;
            loot.transform.localScale = Vector3.one * 0.4f;
        }
        else
        {
            loot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            loot.transform.localScale = Vector3.one * 0.3f;
            Destroy(loot.GetComponent<Collider>());

            if (cachedFallbackMaterial == null)
            {
                cachedFallbackMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                cachedFallbackMaterial.color = Color.cyan;
                cachedFallbackMaterial.EnableKeyword("_EMISSION");
                cachedFallbackMaterial.SetColor("_EmissionColor", Color.cyan * 1.5f);
            }
            loot.GetComponent<Renderer>().material = cachedFallbackMaterial;
        }

        loot.transform.SetParent(transform);
        FlyingLoot fl = loot.AddComponent<FlyingLoot>();
        return fl;
    }

    public void ReturnLootToPool(FlyingLoot fl)
    {
        if (fl == null) return;
        fl.gameObject.SetActive(false);
        if (!lootPool.Contains(fl))
        {
            lootPool.Push(fl);
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
    private System.Action<FlyingLoot> returnCallback;

    public void Init(Vector3 start, int amount, System.Action<FlyingLoot> onComplete)
    {
        returnCallback = onComplete;
        mainCam = Camera.main;
        flyTimer = 0f;

        initialScale = Vector3.one * 0.4f;
        transform.localScale = initialScale;

        // Target: Icon vàng nằm ở góc trên bên trái UI
        targetScreenPos = new Vector3(Screen.width * 0.1f, Screen.height * 0.9f, 20f); 
    }

    private void Update()
    {
        flyTimer += Time.deltaTime;
        float t = flyTimer / maxFlyTime;
        
        if (t > 1f)
        {
            if (returnCallback != null)
            {
                returnCallback(this);
            }
            else
            {
                gameObject.SetActive(false);
            }
            return;
        }

        Vector3 currentPos = transform.position;
        if (mainCam != null)
        {
            Vector3 targetWorldPos = mainCam.ScreenToWorldPoint(targetScreenPos);
            Vector3 lerpedPos = Vector3.Lerp(currentPos, targetWorldPos, t * t);
            transform.position = lerpedPos;

            // Billboarding: Giữ cho Sprite 2D luôn đối diện hướng nhìn của Camera
            transform.LookAt(transform.position + mainCam.transform.rotation * Vector3.forward, mainCam.transform.rotation * Vector3.up);
        }

        transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);
    }
}
