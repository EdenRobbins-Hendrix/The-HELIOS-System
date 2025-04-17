using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantScript : MonoBehaviour
{
    [Header("Growth Settings")]
    public float growthLevel = 1.0f;
    public float maxGrowthLevel = 5.0f;
    public float growthRate = 0.1f;
    
    [Header("Visual Settings")]
    public float minScale = 0.7f;
    public float maxScale = 1.3f;
    public Color matureColor = new Color(0.0f, 0.8f, 0.0f); // Dark green
    public Color youngColor = new Color(0.5f, 0.8f, 0.2f);  // Light green
    
    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("No SpriteRenderer found on plant object!");
        }
        
        originalScale = transform.localScale;
        UpdateAppearance();
    }
    
    public void BoostGrowth(float amount)
    {
        // Increment growth level but cap at maximum
        growthLevel = Mathf.Min(growthLevel + amount, maxGrowthLevel);
        
        // Update visual appearance
        UpdateAppearance();
        
        // Debug info
        Debug.Log($"Plant {gameObject.name} boosted! Growth level: {growthLevel}/{maxGrowthLevel}");
    }
    
    void UpdateAppearance()
    {
        // Calculate scale based on growth level
        float scaleRatio = Mathf.Lerp(minScale, maxScale, growthLevel / maxGrowthLevel);
        transform.localScale = originalScale * scaleRatio;
        
        // Update color based on growth level
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(youngColor, matureColor, growthLevel / maxGrowthLevel);
        }
    }
    
    // Called by GameManager to determine if plant is ready to multiply
    public bool IsReadyToMultiply()
    {
        return growthLevel >= maxGrowthLevel * 0.8f; // 80% grown is ready to multiply
    }
    
    // Called when plant is consumed by an animal
    public void OnConsumed()
    {
        // Reduce growth level when partially eaten
        growthLevel = Mathf.Max(growthLevel - 2.0f, 0.5f);
        UpdateAppearance();
    }
}