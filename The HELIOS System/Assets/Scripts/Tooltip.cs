using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tooltip : MonoBehaviour
{
    private HungerScript hungerScript;
    private MovementSteer movementScript;
    private PlantScript plantScript;
    
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRadius = 0.5f;
    [SerializeField] private bool useMouseHover = true;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject selectionIndicator;
    
    private bool isSelected = false;
    
    private void Start()
    {
        // Get references to the necessary components
        hungerScript = GetComponent<HungerScript>();
        movementScript = GetComponent<MovementSteer>();
        plantScript = GetComponent<PlantScript>();
        
        // Initialize selection indicator if available
        if (selectionIndicator != null)
            selectionIndicator.SetActive(false);
    }
    
    private void Update()
    {
        // Handle clicks for plants (to show growth animation)
        if (Input.GetMouseButtonDown(0) && plantScript != null)
        {
            // Check if mouse is over this plant
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
            
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                // Show tooltip with animation
                ShowPlantTooltip();
            }
        }
    }
    
    private void OnMouseEnter()
    {
        if (!useMouseHover) return;
        
        isSelected = true;
        
        // Show selection indicator if available
        if (selectionIndicator != null)
            selectionIndicator.SetActive(true);
            
        // Show appropriate tooltip based on organism type
        if (hungerScript != null && movementScript != null)
        {
            ShowAnimalTooltip();
        }
        else if (plantScript != null)
        {
            ShowPlantTooltip();
        }
    }
    
    private void OnMouseExit()
    {
        if (!useMouseHover) return;
        
        isSelected = false;
        
        // Hide selection indicator
        if (selectionIndicator != null)
            selectionIndicator.SetActive(false);
            
        // Hide tooltip
        if (TooltipSystem.Instance != null)
            TooltipSystem.Instance.Hide();
    }
    
    private void ShowAnimalTooltip()
    {
        if (TooltipSystem.Instance == null) return;
        
        string behaviorText = "Unknown";
        
        if (movementScript.isHunting)
            behaviorText = "Hunting";
        else if (movementScript.isWandering)
            behaviorText = "Wandering";
        else if (movementScript.isAvoidingPredator)
            behaviorText = "Fleeing";
            
        // Get the clean name without "(Clone)" or other suffixes
        string cleanName = gameObject.name.Split('(')[0].Trim();
        
        TooltipSystem.Instance.ShowAnimalTooltip(
            cleanName,
            movementScript.speed,
            hungerScript.hunger,
            hungerScript.feedThreshold,
            behaviorText
        );
    }
    
    private void ShowPlantTooltip()
    {
        if (TooltipSystem.Instance == null || plantScript == null) return;
        
        // Get the clean name without "(Clone)" or other suffixes
        string cleanName = gameObject.name.Split('(')[0].Trim();
        
        TooltipSystem.Instance.ShowPlantTooltip(
            cleanName,
            plantScript.growthLevel,
            plantScript.maxGrowthLevel,
            plantScript.isFullyGrown
        );
    }
}