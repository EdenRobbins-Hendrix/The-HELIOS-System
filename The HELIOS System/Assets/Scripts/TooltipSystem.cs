using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TooltipSystem : MonoBehaviour
{
    public static TooltipSystem Instance { get; private set; }
    
    [Header("Main References")]
    [SerializeField] private GameObject tooltipGameObject;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private RectTransform backgroundRectTransform;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image hologramBorder;
    
    [Header("Animal UI References")]
    [SerializeField] private GameObject animalStatsPanel;
    [SerializeField] private Slider hungerBar;
    [SerializeField] private TextMeshProUGUI speedInfoText;
    [SerializeField] private TextMeshProUGUI behaviorInfoText;

    [Header("Plant UI References")]
    [SerializeField] private GameObject plantStatsPanel;
    [SerializeField] private Slider growthBar;
    [SerializeField] private TextMeshProUGUI statusInfoText;
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem glowParticles;
    [SerializeField] private GameObject scanLine;
    
    private Coroutine growthAnimationCoroutine;
    private Coroutine borderPulseCoroutine;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        
        // Initially hide the tooltip
        tooltipGameObject.SetActive(false);
    }
    
    private void Update()
    {
        // Follow mouse position with slight delay for holographic effect
        if (tooltipGameObject.activeSelf)
        {
            Vector2 mousePosition = Input.mousePosition;
            
            // Get the RectTransform
            RectTransform rt = GetComponent<RectTransform>();
            
            // Position tooltip near mouse but keep on screen
            float tooltipWidth = rt.rect.width;
            float tooltipHeight = rt.rect.height;
            
            // Default position is to the right and above mouse cursor
            float xPos = mousePosition.x + 20;
            float yPos = mousePosition.y + 20;
            
            // Check if tooltip would go off screen
            if (xPos + tooltipWidth > Screen.width)
                xPos = mousePosition.x - tooltipWidth - 20;
                
            if (yPos + tooltipHeight > Screen.height)
                yPos = mousePosition.y - tooltipHeight - 20;
            
            // Set the position in screen space
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                rt.position = new Vector2(xPos, yPos);
            }
            else if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                // Convert screen point to world point
                Vector2 worldPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform,
                    new Vector2(xPos, yPos),
                    canvas.worldCamera,
                    out worldPos);
                    
                rt.position = canvas.transform.TransformPoint(worldPos);
            }
        }
    }
    
    public void ShowAnimalTooltip(string title, float speed, float hunger, float maxHunger, string behavior)
    {
        // First, make the tooltip active
        tooltipGameObject.SetActive(true);
        
        // Now set up the animal tooltip content
        titleText.text = title;
        speedInfoText.text = $"Speed: {speed}";
        behaviorInfoText.text = $"Behavior: {behavior}";
        
        // Show animal stats, hide plant stats
        animalStatsPanel.SetActive(true);
        plantStatsPanel.SetActive(false);
        
        // Set hunger bar value
        hungerBar.maxValue = maxHunger;
        hungerBar.value = hunger;
        
        // Color the hunger bar based on value (red when low, green when high)
        Image hungerFill = hungerBar.fillRect.GetComponent<Image>();
        if (hungerFill != null)
        {
            hungerFill.color = Color.Lerp(Color.red, Color.green, hunger / maxHunger);
        }
        
        // Activate additional effects
        ActivateVisualEffects();
    }
    
    public void ShowPlantTooltip(string title, float growth, float maxGrowth, bool isFullyGrown)
    {
        // First, make the tooltip active
        tooltipGameObject.SetActive(true);
        
        // Now set up the plant tooltip content
        titleText.text = title;
        statusInfoText.text = isFullyGrown ? "Status: Fully Grown" : "Status: Growing";
        
        // Show plant stats, hide animal stats
        animalStatsPanel.SetActive(false);
        plantStatsPanel.SetActive(true);
        
        // Set growth bar value and max
        growthBar.maxValue = maxGrowth;
        
        // If there's a previous animation running, stop it
        if (growthAnimationCoroutine != null)
            StopCoroutine(growthAnimationCoroutine);
        
        // Now that the GameObject is active, start the animation
        growthAnimationCoroutine = StartCoroutine(AnimateGrowthBar(growth));
        
        // Activate additional effects
        ActivateVisualEffects();
    }
    
    private void ActivateVisualEffects()
    {
        // Activate scan line
        if (scanLine != null)
        {
            scanLine.SetActive(true);
        }
        
        // Play hologram effect
        if (glowParticles != null)
        {
            glowParticles.Stop();
            glowParticles.Play();
        }
            
        // Pulse the border for holographic effect
        if (borderPulseCoroutine != null)
            StopCoroutine(borderPulseCoroutine);
            
        borderPulseCoroutine = StartCoroutine(PulseHologramBorder());
    }
    
    private IEnumerator AnimateGrowthBar(float targetValue)
    {
        float startValue = growthBar.value;
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            
            // Ease in-out animation
            t = t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
            
            growthBar.value = Mathf.Lerp(startValue, targetValue, t);
            
            // Update color based on growth (from blue to green)
            Image growthFill = growthBar.fillRect.GetComponent<Image>();
            if (growthFill != null)
            {
                growthFill.color = Color.Lerp(new Color(0.2f, 0.6f, 1f), Color.green, growthBar.value / growthBar.maxValue);
            }
            
            yield return null;
        }
        
        // Ensure we reach exactly the target value
        growthBar.value = targetValue;
    }
    
    private IEnumerator PulseHologramBorder()
    {
        if (hologramBorder == null) yield break;
        
        Color originalColor = hologramBorder.color;
        Color brightColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0.8f);
        
        while (tooltipGameObject.activeSelf)
        {
            float duration = 1.5f;
            float elapsed = 0f;
            
            while (tooltipGameObject.activeSelf && elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.PingPong(elapsed * 2f, 1f);
                hologramBorder.color = Color.Lerp(originalColor, brightColor, t);
                yield return null;
            }
        }
        
        hologramBorder.color = originalColor;
    }
    
    public void Hide()
    {
        tooltipGameObject.SetActive(false);
        
        // Stop particle effects
        if (glowParticles != null)
            glowParticles.Stop();
            
        // Stop any running coroutines
        if (growthAnimationCoroutine != null)
            StopCoroutine(growthAnimationCoroutine);
            
        if (borderPulseCoroutine != null)
            StopCoroutine(borderPulseCoroutine);
    }
}