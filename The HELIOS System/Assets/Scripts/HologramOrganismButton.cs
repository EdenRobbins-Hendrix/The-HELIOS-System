using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(CanvasGroup))]
public class HologramOrganismButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image borderImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;
    
    [Header("Hologram Style")]
    [SerializeField] private Color baseColor = new Color(0.4f, 0.8f, 1f, 0.8f);
    [SerializeField] private Color highlightColor = new Color(0.5f, 0.9f, 1f, 0.9f);
    [SerializeField] private Color textColor = new Color(0.7f, 1f, 1f, 1f);
    [SerializeField] private float glowIntensity = 1.2f;
    [SerializeField] private float pulseSpeed = 1.5f;
    [SerializeField] private float borderPulseMin = 0.6f;
    [SerializeField] private float borderPulseMax = 1.0f;
    
    [Header("Effects")]
    [SerializeField] private ParticleSystem glowParticles;
    [SerializeField] private GameObject scanLine;
    
    // Components
    private Button button;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    
    // State
    private bool isHovered = false;
    private bool isInitialized = false;
    private Coroutine pulseBorderCoroutine;
    private Coroutine scanLineCoroutine;
    
    private void Awake()
    {
        button = GetComponent<Button>();
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        
        // Find references if not assigned
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();
            
        if (iconImage == null)
            iconImage = transform.Find("IconImage")?.GetComponent<Image>();
            
        if (borderImage == null)
            borderImage = transform.Find("Border")?.GetComponent<Image>();
            
        if (nameText == null)
            nameText = transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            
        if (costText == null)
            costText = transform.Find("CostContainer/CostText")?.GetComponent<TextMeshProUGUI>();
            
        if (glowParticles == null)
            glowParticles = GetComponentInChildren<ParticleSystem>();
            
        if (scanLine == null)
            scanLine = transform.Find("ScanLine")?.gameObject;
            
        // Set up callbacks
        if (button != null)
        {
            // Store original colors
            ColorBlock colors = button.colors;
            colors.normalColor = baseColor;
            colors.highlightedColor = highlightColor;
            colors.pressedColor = highlightColor * 0.8f;
            colors.selectedColor = highlightColor;
            colors.disabledColor = new Color(baseColor.r, baseColor.g, baseColor.b, 0.5f);
            button.colors = colors;
        }
    }
    
    private void OnEnable()
    {
        if (!isInitialized)
        {
            InitializeHologramStyle();
            isInitialized = true;
        }
        
        // Start effects
        if (borderImage != null)
        {
            pulseBorderCoroutine = StartCoroutine(PulseBorder());
        }
        
        if (scanLine != null)
        {
            scanLineCoroutine = StartCoroutine(AnimateScanLine());
        }
    }
    
    private void OnDisable()
    {
        // Stop all effects
        if (pulseBorderCoroutine != null)
        {
            StopCoroutine(pulseBorderCoroutine);
            pulseBorderCoroutine = null;
        }
        
        if (scanLineCoroutine != null)
        {
            StopCoroutine(scanLineCoroutine);
            scanLineCoroutine = null;
        }
    }
    
    public void InitializeHologramStyle()
    {
        // Style background
        if (backgroundImage != null)
        {
            backgroundImage.color = baseColor;
        }
        
        // Style border
        if (borderImage != null)
        {
            borderImage.color = new Color(highlightColor.r, highlightColor.g, highlightColor.b, borderPulseMin);
        }
        
        // Style text components
        if (nameText != null)
        {
            nameText.color = textColor;
        }
        
        if (costText != null)
        {
            costText.color = textColor;
        }
        
        // Setup glow particles
        if (glowParticles != null)
        {
            var main = glowParticles.main;
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(baseColor.r, baseColor.g, baseColor.b, 0.2f),
                new Color(highlightColor.r, highlightColor.g, highlightColor.b, 0.5f)
            );
            glowParticles.Play();
        }
        
        // Setup scan line
        if (scanLine != null)
        {
            Image scanLineImage = scanLine.GetComponent<Image>();
            if (scanLineImage != null)
            {
                scanLineImage.color = new Color(highlightColor.r, highlightColor.g, highlightColor.b, 0.4f);
            }
            scanLine.SetActive(true);
        }
    }
    
    // Called when the organism is set for this button
    public void SetOrganism(HologramOrganismPanel.OrganismData organism)
    {
        // Set icon
        if (iconImage != null && organism.icon != null)
        {
            iconImage.sprite = organism.icon;
            iconImage.preserveAspect = true;
        }
        
        // Set text elements
        if (nameText != null)
        {
            nameText.text = organism.name;
        }
        
        if (costText != null)
        {
            costText.text = organism.energyCost.ToString();
        }
    }
    
    // Method to highlight on hover
    public void OnPointerEnter()
    {
        isHovered = true;
        
        // Scale up slightly
        transform.localScale = Vector3.one * 1.05f;
        
        // Increase particle emission
        if (glowParticles != null)
        {
            var emission = glowParticles.emission;
            emission.rateOverTime = 20;
        }
    }
    
    // Method to return to normal on exit
    public void OnPointerExit()
    {
        isHovered = false;
        
        // Return to normal scale
        transform.localScale = Vector3.one;
        
        // Decrease particle emission
        if (glowParticles != null)
        {
            var emission = glowParticles.emission;
            emission.rateOverTime = 10;
        }
    }
    
    private IEnumerator PulseBorder()
    {
        if (borderImage == null) yield break;
        
        while (true)
        {
            float time = 0f;
            float duration = 1f / pulseSpeed;
            
            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                float t = time / duration;
                
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
    
    private IEnumerator AnimateScanLine()
    {
        if (scanLine == null) yield break;
        
        RectTransform scanLineRect = scanLine.GetComponent<RectTransform>();
        if (scanLineRect == null) yield break;
        
        while (true)
        {
            // Move scan line from bottom to top
            float duration = 2.0f;
            float time = 0f;
            float height = rectTransform.rect.height;
            
            // Start at bottom
            scanLineRect.anchorMin = new Vector2(0, 0);
            scanLineRect.anchorMax = new Vector2(1, 0);
            scanLineRect.sizeDelta = new Vector2(0, 2); // Height of 2 pixels
            
            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                float normalizedPos = time / duration;
                
                // Set position
                scanLineRect.anchoredPosition = new Vector2(0, normalizedPos * height);
                
                yield return null;
            }
            
            // Wait a bit before next scan
            yield return new WaitForSeconds(1.0f);
        }
    }
    
    // Play a highlight flash effect (for selection)
    public void PlaySelectionEffect()
    {
        StartCoroutine(FlashEffect());
    }
    
    private IEnumerator FlashEffect()
    {
        // Flash the background
        if (backgroundImage != null)
        {
            Color originalColor = backgroundImage.color;
            Color flashColor = highlightColor;
            flashColor.a = originalColor.a;
            
            float duration = 0.3f;
            float time = 0f;
            
            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                float t = time / duration;
                
                // Flash up then back down
                float flashIntensity = Mathf.Sin(t * Mathf.PI);
                backgroundImage.color = Color.Lerp(originalColor, flashColor, flashIntensity);
                
                yield return null;
            }
            
            backgroundImage.color = originalColor;
        }
        
        // Burst the particles
        if (glowParticles != null)
        {
            var emission = glowParticles.emission;
            var originalRate = emission.rateOverTime;
            
            emission.rateOverTime = 30;
            yield return new WaitForSeconds(0.3f);
            emission.rateOverTime = originalRate;
        }
    }
}