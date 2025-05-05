using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[ExecuteInEditMode]
public class HologramStyleUI : MonoBehaviour
{
    [Header("Hologram Settings")]
    [SerializeField] private Color hologramColor = new Color(0.4f, 0.8f, 1f, 0.8f);
    [SerializeField] private Color borderColor = new Color(0.6f, 0.9f, 1f, 0.9f);
    [SerializeField] private float borderPulseSpeed = 1.5f;
    [SerializeField] private float borderPulseMin = 0.6f;
    [SerializeField] private float borderPulseMax = 1.0f;
    
    [Header("Scan Line")]
    [SerializeField] private bool useScanLine = true;
    [SerializeField] private GameObject scanLinePrefab;
    [SerializeField] private float scanSpeed = 2.0f;
    
    [Header("Particle Effects")]
    [SerializeField] private bool useParticleEffects = true;
    [SerializeField] private GameObject particleSystemPrefab;
    [SerializeField] private int particleDensity = 15;
    
    [Header("References")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image borderImage;
    
    private GameObject scanLine;
    private GameObject particleEffect;
    private RectTransform rectTransform;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        // Find references if not assigned
        if (backgroundImage == null)
            backgroundImage = transform.Find("Background")?.GetComponent<Image>();
            
        if (borderImage == null)
            borderImage = transform.Find("Border")?.GetComponent<Image>();
            
        // Apply hologram style
        ApplyHologramStyle();
    }
    
    private void OnEnable()
    {
        // Create effects when enabled
        if (useScanLine)
            CreateScanLine();
            
        if (useParticleEffects)
            CreateParticleEffect();
            
        // Start pulsing border
        StartCoroutine(PulseBorder());
    }
    
    private void OnDisable()
    {
        // Clean up effects
        if (scanLine != null)
            DestroyImmediate(scanLine);
            
        if (particleEffect != null)
            DestroyImmediate(particleEffect);
            
        // Stop all coroutines
        StopAllCoroutines();
    }
    
    void Update()
    {
        // Update scan line position in edit mode
        if (useScanLine && scanLine != null)
        {
            UpdateScanLine();
        }
    }
    
    public void ApplyHologramStyle()
    {
        // Style background
        if (backgroundImage != null)
        {
            backgroundImage.color = hologramColor;
        }
        
        // Style border
        if (borderImage != null)
        {
            borderImage.color = borderColor;
        }
    }
    
    private void CreateScanLine()
    {
        // Destroy existing scan line if any
        if (scanLine != null)
            DestroyImmediate(scanLine);
            
        // If we have a prefab, instantiate it
        if (scanLinePrefab != null)
        {
            scanLine = Instantiate(scanLinePrefab, transform);
        }
        else
        {
            // Create a default scan line
            scanLine = new GameObject("ScanLine");
            scanLine.transform.SetParent(transform, false);
            
            // Add components
            Image scanLineImage = scanLine.AddComponent<Image>();
            scanLineImage.color = new Color(hologramColor.r, hologramColor.g, hologramColor.b, 0.3f);
            
            // Configure RectTransform
            RectTransform scanLineRect = scanLine.GetComponent<RectTransform>();
            scanLineRect.anchorMin = new Vector2(0, 0);
            scanLineRect.anchorMax = new Vector2(1, 0);
            scanLineRect.pivot = new Vector2(0.5f, 0.5f);
            scanLineRect.sizeDelta = new Vector2(0, 4); // Height of 4 pixels
        }
    }
    
    private void UpdateScanLine()
    {
        if (scanLine == null) return;
        
        RectTransform scanLineRect = scanLine.GetComponent<RectTransform>();
        if (scanLineRect != null)
        {
            // Calculate position based on time
            float normalizedPos = Mathf.PingPong(Time.unscaledTime * scanSpeed, 1.0f);
            float height = rectTransform.rect.height;
            
            // Update position
            scanLineRect.anchoredPosition = new Vector2(0, normalizedPos * height - height / 2);
        }
    }
    
    private void CreateParticleEffect()
    {
        // Destroy existing effect if any
        if (particleEffect != null)
            DestroyImmediate(particleEffect);
            
        // If we have a prefab, instantiate it
        if (particleSystemPrefab != null)
        {
            particleEffect = Instantiate(particleSystemPrefab, transform);
            
            // Ensure it's positioned and scaled correctly
            RectTransform effectRect = particleEffect.GetComponent<RectTransform>();
            if (effectRect != null)
            {
                effectRect.anchorMin = new Vector2(0, 0);
                effectRect.anchorMax = new Vector2(1, 1);
                effectRect.offsetMin = Vector2.zero;
                effectRect.offsetMax = Vector2.zero;
            }
        }
        else
        {
            // Create a particle system programmatically
            // This is complex and might not be ideal - better to use a prefab
            Debug.LogWarning("No particle system prefab assigned. Consider creating one in the editor.");
        }
    }
    
    private IEnumerator PulseBorder()
    {
        if (borderImage == null) yield break;
        
        while (true)
        {
            float duration = 1f / borderPulseSpeed;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                
                // Sine wave pulse
                float pulse = Mathf.Lerp(borderPulseMin, borderPulseMax, 
                    (Mathf.Sin(t * Mathf.PI * 2) + 1) * 0.5f);
                
                // Update alpha
                Color newColor = borderImage.color;
                newColor.a = pulse;
                borderImage.color = newColor;
                
                yield return null;
            }
        }
    }
}