using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    private TextMesh textMesh;
    private float moveYSpeed = 3f;
    private float disappearTimer;
    private float disappearTimerMax = 0.8f;
    private Color textColor;
    private Camera mainCamera;

    public void Setup(float damageAmount, bool isCritical)
    {
        textMesh = GetComponent<TextMesh>();
        if (textMesh == null) return;
        
        textMesh.text = damageAmount.ToString("F0");
        
        if (isCritical) 
        { 
            textColor = Color.red; 
            textMesh.characterSize = 0.3f; 
        }
        else 
        { 
            textColor = new Color(1f, 0.6f, 0f); // Màu cam
            textMesh.characterSize = 0.2f; 
        }
        
        textMesh.color = textColor;
        disappearTimer = disappearTimerMax;
        mainCamera = Camera.main;
    }

    private void Update()
    {
        transform.position += new Vector3(0, moveYSpeed) * Time.deltaTime;
        
        // Luôn xoay mặt về Camera (Billboarding)
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
        }
        
        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            float disappearSpeed = 3f;
            textColor.a -= disappearSpeed * Time.deltaTime;
            if (textMesh != null) textMesh.color = textColor;
            if (textColor.a < 0) Destroy(gameObject);
        }
    }
}
