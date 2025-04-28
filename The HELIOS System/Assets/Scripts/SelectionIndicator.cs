using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionIndicator : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 30f;
    [SerializeField] private float pulseSpeed = 1.5f;
    [SerializeField] private float minScale = 0.9f;
    [SerializeField] private float maxScale = 1.1f;
    
    private SpriteRenderer spriteRenderer;
    private float originalAlpha;
    
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
            originalAlpha = spriteRenderer.color.a;
    }
    
    private void Update()
    {
        // Rotate continuously
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        
        // Pulse scale
        float scale = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f);
        transform.localScale = new Vector3(scale, scale, 1f);
        
        // Pulse opacity
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = originalAlpha * Mathf.Lerp(0.6f, 1f, (Mathf.Sin(Time.time * pulseSpeed * 2f) + 1f) * 0.5f);
            spriteRenderer.color = color;
        }
    }
}