using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantEnergizer : MonoBehaviour
{
    [Header("Energy Settings")]
    public int energyCost = 5;
    public float growthBoostAmount = 1.0f;
    
    [Header("Visual Feedback")]
    public GameObject boostEffectPrefab;
    public float clickRadius = 0.5f;
    public AudioClip boostSound;
    
    private EnergyManager energyManager;
    private AudioSource audioSource;
    
    void Start()
    {
        energyManager = FindObjectOfType<EnergyManager>();
        if (energyManager == null)
        {
            Debug.LogError("EnergyManager not found in the scene!");
        }
        
        // Add AudioSource for sound effects if needed
        audioSource = gameObject.AddComponent<AudioSource>();
    }
    
    void Update()
    {
        // Check for mouse click
        if (Input.GetMouseButtonDown(0))
        {
            // Get mouse position in world coordinates
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10; // Set a distance from the camera
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
            
            // Debug info
            Debug.Log("Click at world position: " + mouseWorldPos);
            
            // Find all colliders in a circle around the click point
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(mouseWorldPos, clickRadius);
            
            Debug.Log("Found " + hitColliders.Length + " colliders at click position");
            
            // Process the colliders
            foreach (Collider2D hitCollider in hitColliders)
            {
                Debug.Log("Hit object: " + hitCollider.gameObject.name);
                
                // Check if it has a PlantScript
                PlantScript plantScript = hitCollider.GetComponent<PlantScript>();
                
                if (plantScript != null)
                {
                    Debug.Log("Found plant: " + hitCollider.gameObject.name);
                    
                    // Try to spend energy
                    if (energyManager.SpendEnergy(energyCost))
                    {
                        // Apply the boost
                        plantScript.BoostGrowth(growthBoostAmount);
                        
                        // Show visual effect
                        if (boostEffectPrefab != null)
                        {
                            GameObject effect = Instantiate(boostEffectPrefab, hitCollider.transform.position, Quaternion.identity);
                            
                            // Destroy effect after a few seconds
                            Destroy(effect, 2.0f);
                        }
                        
                        // Play sound if available
                        if (boostSound != null && audioSource != null)
                        {
                            audioSource.PlayOneShot(boostSound);
                        }
                        
                        Debug.Log("Successfully boosted plant: " + hitCollider.gameObject.name);
                        break; // Only boost one plant per click
                    }
                    else
                    {
                        Debug.Log("Not enough energy to boost plant!");
                    }
                }
            }
        }
    }
    
    // Visualize the click radius in Scene view (visible in Editor only)
    void OnDrawGizmos()
    {
        if (Camera.main != null && Input.GetMouseButton(0))
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10;
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(mouseWorldPos, clickRadius);
        }
    }
}