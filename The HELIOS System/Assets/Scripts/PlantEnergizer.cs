using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantEnergizer : MonoBehaviour
{
    [Header("Energy Settings")]
    public int energyCost = 5;
    public float growthBoostAmount = 1.0f;
    public int spawnTreeCost = 15;
    
    [Header("Visual Feedback")]
    public GameObject boostEffectPrefab;
    public GameObject spawnEffectPrefab;
    public float clickRadius = 0.5f;
    
    private EnergyManager energyManager;
    
    void Start()
    {
        energyManager = FindObjectOfType<EnergyManager>();
        if (energyManager == null)
        {
            Debug.LogError("EnergyManager not found in the scene!");
        }
    }
    
    void Update()
    {
        // Check for mouse click
        if (Input.GetMouseButtonDown(0))
        {
            // Get mouse position in world coordinates
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10;
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
            
            // Find all colliders in a circle around the click point
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(mouseWorldPos, clickRadius);
            
            foreach (Collider2D hitCollider in hitColliders)
            {
                // Check if it has a PlantScript
                PlantScript plantScript = hitCollider.GetComponent<PlantScript>();
                
                if (plantScript != null)
                {
                    // Handle interaction differently based on growth state
                    if (plantScript.isFullyGrown)
                    {
                        // Fully grown tree can spawn a new tree
                        HandleFullyGrownTree(plantScript, hitCollider.transform.position);
                    }
                    else
                    {
                        // Growing tree gets a growth boost
                        HandleGrowingTree(plantScript, hitCollider.transform.position);
                    }
                    
                    // Only interact with one tree per click
                    break;
                }
            }
        }
    }
    
    void HandleGrowingTree(PlantScript plantScript, Vector3 position)
    {
        // Try to spend energy to boost growth
        if (energyManager.SpendEnergy(energyCost))
        {
            // Apply the boost
            plantScript.BoostGrowth(growthBoostAmount);
            
            // Show visual effect
            if (boostEffectPrefab != null)
            {
                GameObject effect = Instantiate(boostEffectPrefab, position, Quaternion.identity);
                Destroy(effect, 2.0f);
            }
            
            Debug.Log("Boosted tree growth!");
        }
        else
        {
            Debug.Log("Not enough energy to boost tree!");
        }
    }
    
    void HandleFullyGrownTree(PlantScript plantScript, Vector3 position)
    {
        // Try to spend energy to spawn a new tree
        if (energyManager.SpendEnergy(spawnTreeCost))
        {
            bool success = plantScript.SpawnNewTree();
            
            if (success)
            {
                // Show special spawn effect
                if (spawnEffectPrefab != null)
                {
                    GameObject effect = Instantiate(spawnEffectPrefab, position, Quaternion.identity);
                    Destroy(effect, 2.0f);
                }
                else if (boostEffectPrefab != null)
                {
                    // Use regular effect if no special effect is defined
                    GameObject effect = Instantiate(boostEffectPrefab, position, Quaternion.identity);
                    Destroy(effect, 2.0f);
                }
                
                Debug.Log("Spawned a new tree!");
            }
            else
            {
                // Refund energy if spawning failed
                energyManager.GainEnergy(spawnTreeCost);
                Debug.Log("Failed to spawn a new tree!");
            }
        }
        else
        {
            Debug.Log("Not enough energy to spawn a new tree!");
        }
    }
}