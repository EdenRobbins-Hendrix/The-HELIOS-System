using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FoodWeb
{
    public class Tooltip : MonoBehaviour
    {
        private HungerScript hungerScript;
        private MovementSteer movementScript;
        
        private void Start()
        {
            // Get references to the necessary components
            hungerScript = GetComponent<HungerScript>();
            movementScript = GetComponent<MovementSteer>();
        }
        
        private void OnMouseEnter()
        {
            if (TooltipSystem.Instance == null)
                return;
                
            if (hungerScript != null && movementScript != null)
            {
                string tooltipText = GenerateTooltipText();
                TooltipSystem.Instance.Show(tooltipText);
            }
            else if (GetComponent<PlantScript>() != null)
            {
                PlantScript plantScript = GetComponent<PlantScript>();
                string tooltipText = GeneratePlantTooltipText(plantScript);
                TooltipSystem.Instance.Show(tooltipText);
            }
        }
        
        private void OnMouseExit()
        {
            if (TooltipSystem.Instance != null)
                TooltipSystem.Instance.Hide();
        }
        
        private string GenerateTooltipText()
        {
            string behaviorText = "Behavior: ";
            
            if (movementScript.isHunting)
            {
                behaviorText += "Hunting";
            }
            else if (movementScript.isWandering)
            {
                behaviorText += "Wandering";
            }
            else if (movementScript.isAvoidingPredator)
            {
                behaviorText += "Avoiding predator";
            }
            
            return $"<b>{gameObject.name.Split('(')[0]}</b>\n" +
                   $"Speed: {movementScript.speed}\n" +
                   $"Hunger: {hungerScript.hunger:F1}/{hungerScript.feedThreshold:F1}\n" +
                   behaviorText;
        }
        
        private string GeneratePlantTooltipText(PlantScript plantScript)
        {
            string growthStatus = plantScript.isFullyGrown ? "Fully grown" : 
                                 $"Growing ({plantScript.growthLevel:F1}/{plantScript.maxGrowthLevel:F1})";
            
            return $"<b>{gameObject.name.Split('(')[0]}</b>\n" +
                   $"Type: Plant\n" +
                   $"Status: {growthStatus}";
        }
    }
}