using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NutScript : MonoBehaviour
{
    [Header("Nut Properties")]
    public float nutritionalValue = 3.0f; // How much hunger it satisfies
    
    // Called when a squirrel or other animal eats the nut
    public void OnConsumed(GameObject consumer)
    {
        // Give nutrition to the consumer
        HungerScript hungerScript = consumer.GetComponent<HungerScript>();
        if (hungerScript != null)
        {
            hungerScript.changeHunger(nutritionalValue);
        }
        
        // Remove from GameManager's list and destroy
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.RemoveNut(gameObject);
        }
        
        Destroy(gameObject);
    }
}