using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrganismEnergizer : MonoBehaviour
{
    [Header("Energy Settings")]
    public int energyCost = 10;
    public float energyBoostAmount = 5f;
    
    [Header("Visual Feedback")]
    public GameObject boostEffectPrefab;
    
    private EnergyManager energyManager;
    private Camera mainCamera;
    
    void Start()
    {
        energyManager = FindObjectOfType<EnergyManager>();
        mainCamera = Camera.main;
    }
    
    void Update()
    {
        // Check for mouse click
        if (Input.GetMouseButtonDown(0))
        {
            // Cast a ray from the camera to the mouse position
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
            
            // If we hit something
            if (hit.collider != null)
            {
                // Check if it's an organism with a HungerScript
                HungerScript hungerScript = hit.collider.GetComponent<HungerScript>();
                if (hungerScript != null)
                {
                    // Try to spend energy to boost this organism
                    if (energyManager.SpendEnergy(energyCost))
                    {
                        // Boost the organism's hunger
                        hungerScript.changeHunger(energyBoostAmount);
                        
                        // Show visual feedback
                        if (boostEffectPrefab != null)
                        {
                            Instantiate(boostEffectPrefab, hit.collider.transform.position, Quaternion.identity);
                        }
                        
                        Debug.Log("Boosted " + hit.collider.name + " with energy!");
                    }
                    else
                    {
                        Debug.Log("Not enough energy!");
                    }
                }
            }
        }
    }
}
