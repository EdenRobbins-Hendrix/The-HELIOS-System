using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementIndicator : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private float pulseSpeed = 1.5f;
    [SerializeField] private float minScale = 0.9f;
    [SerializeField] private float maxScale = 1.1f;
    [SerializeField] private float rotationSpeed = 30f;
    [SerializeField] private float transparency = 0.5f;
    
    // Ring sprite settings
    [Header("Ring Settings")]
    [SerializeField] private bool useRingStyle = true;
    [SerializeField] private float ringThickness = 0.1f;
    
    private SpriteRenderer spriteRenderer;
    private float originalAlpha;
    
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            // Store original alpha
            originalAlpha = spriteRenderer.color.a;
            
            // Set transparency
            Color color = spriteRenderer.color;
            color.a = transparency;
            spriteRenderer.color = color;
            
            // Make sure it's a ring-style sprite if requested
            if (useRingStyle)
            {
                // This works best with a dedicated ring sprite
                // If using a circle sprite, you might need to adjust its material
                // or shader to create a ring effect
            }
        }
        
        // Ensure the indicator is below any dragged objects in sorting order
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = -1;
        }
    }
    
    private void Update()
    {
        // Rotate the indicator
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        
        // Pulse the scale
        float scale = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f);
        transform.localScale = new Vector3(scale, scale, scale);
        
        // Pulse opacity
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = transparency * Mathf.Lerp(0.6f, 1f, (Mathf.Sin(Time.time * pulseSpeed * 2f) + 1f) * 0.5f);
            spriteRenderer.color = color;
        }
    }
    
    // Call this method to update the indicator color based on placement validity
    public void UpdateColor(Color newColor)
    {
        if (spriteRenderer != null)
        {
            Color color = newColor;
            color.a = transparency;
            spriteRenderer.color = color;
        }
    }
}
