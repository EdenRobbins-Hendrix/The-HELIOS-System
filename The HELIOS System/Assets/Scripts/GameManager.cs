using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static GameManager Instance { get; private set; }

    [Header("Energy System")]
    public TextMeshProUGUI energyUI;
    public int energy;

    [Header("Animal Management")]
    public List<GameObject> organisms;
    public List<GameObject> speciesPrefabs;

    [Header("Plant Management")]
    public List<GameObject> plants = new List<GameObject>();
    public GameObject plantPrefab;
    public float plantSpreadDistance = 2.0f;
    public int maxPlantsInScene = 50;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Animal management
        InvokeRepeating("decrementHungerInAllOrganisms", 5.0f, 5.0f);
        InvokeRepeating("increasePopulationGlobally", 10.0f, 10.0f);
        
        // Plant management - optional if you want passive growth
        InvokeRepeating("CheckPlantGrowth", 8.0f, 8.0f);
    }

    // Update is called once per frame
    void Update()
    {
        // Empty update method
    }

    #region Animal Management

    void decrementHungerInAllOrganisms()
    {
        foreach (GameObject organism in organisms)
        {
            HungerScript s = organism.GetComponent<HungerScript>();
            float rate = s.hungerDeclineRate;
            s.changeHunger(-rate);
            setOrganismBehavior(organism);
        }
    }

    void setOrganismBehavior(GameObject organism)
    {
        HungerScript h = organism.GetComponent<HungerScript>();
        MovementSteer m = organism.GetComponent<MovementSteer>();
        if (h.hunger > h.feedThreshold)
        {
            m.isHunting = false;
            m.isWandering = true;
        }
        else
        {
            m.isHunting = true;
            m.isWandering = false;

            //organism is now hunting
            GameObject target = chooseTarget(organism, h);
            //assign target
            m.target = target;
        }
    }

    //this method exists basically so that the game manager is the only script looking at other scripts
    public void attemptFeed(GameObject consumer, GameObject consumed, float energyAmount)
    {
        HungerScript s = consumer.GetComponent<HungerScript>();
        
        // Check if consumed is a plant
        PlantScript plantScript = consumed.GetComponent<PlantScript>();
        if (plantScript != null)
        {
            // Check if this animal eats plants
            foreach (string p in s.potentialFoodTargetsNames)
            {
                if (consumed.name.Contains(p))
                {
                    Debug.Log("Valid plant food target");
                    
                    // Plants aren't completely consumed - they're just reduced in size
                    s.changeHunger(energyAmount);
                    plantScript.OnConsumed();
                    
                    // Reset behavior to either hunt or wander
                    setOrganismBehavior(consumer);
                    
                    return;
                }
            }
        }
        
        // Original code for animal consumption
        foreach (string p in s.potentialFoodTargetsNames)
        {
            if (consumed.name.Contains(p))
            {
                Debug.Log("Valid food target");
                feed(consumer, consumed, energyAmount);
                return;
            }
        }
        
        Debug.Log(consumer.name + " attempting to eat invalid organism: " + consumed.name);
    }

    public void feed(GameObject consumer, GameObject consumed, float energyAmount)
    {
        //increase hunger in consumer
        HungerScript s = consumer.GetComponent<HungerScript>();
        s.changeHunger(energyAmount);

        //reset behavior to either hunt or wander
        setOrganismBehavior(consumer);

        //destroy consumed
        organisms.Remove(consumed);
        Destroy(consumed);
    }

    public GameObject chooseTarget(GameObject predator, HungerScript hScript)
    {
        GameObject target = null;
        float smallestDistance = float.PositiveInfinity;

        List<GameObject> allPotentialTargets = new List<GameObject>(organisms);
        
        // Add plants to potential targets if the predator is a herbivore
        foreach (string targetName in hScript.potentialFoodTargetsNames)
        {
            if (targetName.Contains("Plant") || targetName.Contains("Tree"))
            {
                // Add plants to the search list
                allPotentialTargets.AddRange(plants);
                break;
            }
        }

        //choose best organism based on criteria (the closest one)
        foreach (GameObject potentialPrey in allPotentialTargets)
        {
            if (potentialPrey == predator) continue; // Skip self
            
            HungerScript s = predator.GetComponent<HungerScript>();
            bool isValidTarget = false;
            
            foreach (string p in s.potentialFoodTargetsNames)
            {
                if (potentialPrey.name.Contains(p))
                {
                    Debug.Log(potentialPrey.name + " is a valid food target for " + predator.name);
                    isValidTarget = true;
                    break;
                }
            }
            
            if (isValidTarget)
            {
                float distance = Vector3.Distance(predator.transform.position, potentialPrey.transform.position);
                if (distance < smallestDistance)
                {
                    smallestDistance = distance;
                    target = potentialPrey;
                }
            }
        }
        
        if (target == null)
        {
            Debug.Log(predator.name + " has no organisms to eat!");
            target = predator; // If there is no valid prey, then the predator will just remain still
        }

        return target;
    }

    // I need an additional method for this so that I can call a repeating function 
    // in my start method. 
    void increasePopulationGlobally()
    {
        foreach (GameObject species in speciesPrefabs)
        {
            increasePopulationofSpecies(species);
        }
    }

    void increasePopulationofSpecies(GameObject species)
    {
        // Get count of organisms of species type
        int count = 0;
        foreach (GameObject organism in organisms)
        {
            if (organism.name.Contains(species.name))
            {
                count = count + 1;
            }
        }

        // spawn correct number of species
        int numOfSpawns = Mathf.FloorToInt(count / 2);
        for (int i = 0; i < numOfSpawns; i++)
        {
            //spawn the organism
            GameObject newOrganism = Instantiate(species);
            organisms.Add(newOrganism);

            print("Organism Spawned!!");
        }
    }

    #endregion

    #region Plant Management

    // This updated method doesn't rely on IsReadyToMultiply
    void CheckPlantGrowth()
    {
        // This is just a placeholder method that doesn't do automatic spreading
        // since we're now handling tree reproduction through player clicks
        
        foreach (GameObject plant in plants)
        {
            PlantScript plantScript = plant.GetComponent<PlantScript>();
            if (plantScript != null)
            {
                // Optional: in the future maybe we can add a tiny amount of passive growth over time
                // plantScript.BoostGrowth(0.02f);
            }
        }
    }
    
    // Method for spawning a new tree near a parent tree
    public bool SpawnTreeNear(GameObject parentTree)
    {
        // Don't spawn new trees if we're at the limit
        if (plants.Count >= maxPlantsInScene)
            return false;
            
        // Calculate random position near the parent tree
        float spawnDistance = plantSpreadDistance;
        Vector2 randomDirection = UnityEngine.Random.insideUnitCircle.normalized;
        Vector3 newPosition = parentTree.transform.position + 
            new Vector3(randomDirection.x, randomDirection.y, 0) * spawnDistance;
            
        // Create new tree
        GameObject newTree = Instantiate(plantPrefab, newPosition, Quaternion.identity);
        
        // Initialize the new tree
        PlantScript newPlantScript = newTree.GetComponent<PlantScript>();
        if (newPlantScript != null)
        {
            // Start with a small tree
            newPlantScript.growthLevel = 1.0f;
            newPlantScript.UpdateAppearance(); // Make sure this method is public
        }
        
        // Add to our list of plants
        plants.Add(newTree);
        
        Debug.Log("New tree spawned near parent tree!");
        return true;
    }

    #endregion

    #region Energy Management

    public void changeEnergy(int change)
    {
        energy += change;
        energyUI.text = "Energy Points: " + energy;
    }

    #endregion
}