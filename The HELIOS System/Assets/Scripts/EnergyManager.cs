using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EnergyManager : MonoBehaviour
{
    [Header("Energy Settings")]
    public int startingEnergy = 100;
    public int energyGainRate = 5;
    public float energyGainInterval = 10f;
    
    [Header("UI References")]
    public TextMeshProUGUI energyText;
    
    private GameManager gameManager;
    
    void Start()
    {
        // Get reference to the GameManager
        gameManager = GameManager.Instance;
        
        // Set the initial energy
        if (gameManager != null)
        {
            gameManager.energy = startingEnergy;
            
            // Assign the TextMeshPro component to the GameManager's energyUI reference
            gameManager.energyUI = energyText;
            
            // Update the UI immediately
            gameManager.changeEnergy(0);
        }
        else
        {
            Debug.LogError("GameManager instance not found!");
        }
        
        // Start gaining energy over time
        InvokeRepeating("GainEnergy", energyGainInterval, energyGainInterval);
    }
    
    void GainEnergy()
    {
        if (gameManager != null)
        {
            gameManager.changeEnergy(energyGainRate);
        }
    }
    
    // Method to spend energy (returns true if successful)
    public bool SpendEnergy(int amount)
    {
        if (gameManager == null) return false;
        
        if (gameManager.energy >= amount)
        {
            gameManager.changeEnergy(-amount);
            return true;
        }
        
        // Not enough energy
        return false;
    }
}