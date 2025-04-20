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
    public float maxScale = 1.5f;
    
    [Header("Reproduction")]
    public bool isFullyGrown = false;
    
    private Vector3 originalScale;
    private SpriteRenderer spriteRenderer;
    private GameManager gameManager;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        gameManager = GameManager.Instance;
        
        // Initial appearance update
        UpdateAppearance();
    }
    
    public void BoostGrowth(float amount)
    {
        // Increment growth level but cap at maximum
        float previousGrowth = growthLevel;
        growthLevel = Mathf.Min(growthLevel + amount, maxGrowthLevel);
        
        // Check if the tree has just reached full growth
        if (previousGrowth < maxGrowthLevel && growthLevel >= maxGrowthLevel)
        {
            isFullyGrown = true;
            Debug.Log("Tree is now fully grown!");
        }
        
        // Update visual appearance
        UpdateAppearance();
    }
    
    public void UpdateAppearance()
    {
        // Calculate scale based on growth level
        float growthRatio = growthLevel / maxGrowthLevel;
        float scaleRatio = Mathf.Lerp(minScale, maxScale, growthRatio);
        transform.localScale = originalScale * scaleRatio;
    }
    
    public bool SpawnNut()
    {
        // Only fully grown trees can spawn nuts
        if (!isFullyGrown)
            return false;
            
        // Tell the GameManager to spawn a nut
        if (gameManager != null)
        {
            return gameManager.SpawnNutNear(gameObject);
        }
        
        return false;
    }
    
    // Called when a tree is consumed by an animal
    public void OnConsumed()
    {
        // Reduce growth level when eaten
        growthLevel = Mathf.Max(growthLevel - 2.0f, 0.5f);
        
        // If it was fully grown, it's not anymore
        if (growthLevel < maxGrowthLevel)
        {
            isFullyGrown = false;
        }
        
        UpdateAppearance();
    }
}