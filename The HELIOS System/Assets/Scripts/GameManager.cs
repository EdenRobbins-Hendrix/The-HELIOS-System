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
    public GameObject nutPrefab; // New field for nut prefab
    public float plantSpreadDistance = 2.0f;
    public float nutSpreadDistance = 1.5f; // Closer than tree spread
    public int maxPlantsInScene = 50;
    public int maxNutsInScene = 30; // Limit for nuts

    // List to track nuts
    private List<GameObject> nuts = new List<GameObject>();

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

    //This code ensures that there are the same amount of organisms present on the screen/in memory as dictated by the population manager
    public void populationUpdate() {

    }
    
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

    public void setOrganismBehavior(GameObject organism)
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
            foreach (string p in s.prey)
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

        // Check if consumed is a nut
        NutScript nutScript = consumed.GetComponent<NutScript>();
        if (nutScript != null)
        {
            // Check if this animal eats nuts
            foreach (string p in s.prey)
            {
                if (consumed.name.Contains(p))
                {
                    Debug.Log("Valid nut food target");

                    // Call the nut's consumption method
                    nutScript.OnConsumed(consumer);

                    // Reset behavior to either hunt or wander
                    setOrganismBehavior(consumer);

                    return;
                }
            }
        }

        // Original code for animal consumption
        foreach (string p in s.prey)
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

        
        //generates replacement to maintain population level
        GameObject replacement = Instantiate(consumed);
        replacement.SetActive(false);
        Camera cam = GameObject.FindAnyObjectByType<Camera>();
        replacement.transform.position = new Vector2(UnityEngine.Random.Range(cam.aspect, -cam.aspect), UnityEngine.Random.Range(cam.orthographicSize, -cam.orthographicSize));

        //destroy consumed
        organisms.Remove(consumed);
        Destroy(consumed);
        replacement.SetActive(true);
        organisms.Add(replacement);
        
    }

    public GameObject chooseTarget(GameObject predator, HungerScript hScript)
    {
        GameObject target = null;
        float smallestDistance = float.PositiveInfinity;

        List<GameObject> allPotentialTargets = new List<GameObject>(organisms);

        // Add plants and nuts to potential targets if the predator eats them
        foreach (string targetName in hScript.prey)
        {
            if (targetName.Contains("Plant") || targetName.Contains("Tree"))
            {
                // Add plants to the search list
                allPotentialTargets.AddRange(plants);
            }

            if (targetName.Contains("Nut") || targetName.Contains("Acorn"))
            {
                // Add nuts to the search list
                allPotentialTargets.AddRange(nuts);
            }
        }

        //choose best organism based on criteria (the closest one)
        foreach (GameObject potentialPrey in allPotentialTargets)
        {
            if (potentialPrey == predator) continue; // Skip self

            bool isValidTarget = false;
            
            foreach (string p in hScript.prey)
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
        float averageHunger = 0;
        foreach (GameObject organism in organisms)
        {
            if (organism.name.Contains(species.name)) // ensure the organism is of the organism type
            {

                HungerScript h = organism.GetComponent<HungerScript>();
                if (h.hunger > h.starvingThreshold) // ensure the organism is not starving
                {
                    count = count + 1;
                    averageHunger = averageHunger + h.hunger;
                }
            }
        }

        //  calculate average hunger of population (if parents are starving then offspring should be starving)
        averageHunger = averageHunger / count;

        // spawn correct number of species
        int numOfSpawns = Mathf.FloorToInt(count / 2);
        for (int i = 0; i < numOfSpawns; i++)
        {
            //spawn the organism
            GameObject newOrganism = Instantiate(species);
            organisms.Add(newOrganism);
            newOrganism.GetComponent<HungerScript>().hunger = Mathf.FloorToInt(averageHunger); //set new organism hunger to average hunger of parents

            print("Organism Spawned!!");
        }
    }

    public void killOrganism(GameObject organim)
    {
        GameObject replacement = Instantiate(organim);
        replacement.SetActive(false);
        Camera cam = GameObject.FindAnyObjectByType<Camera>();
        replacement.transform.position = new Vector2(UnityEngine.Random.Range(cam.aspect, -cam.aspect), UnityEngine.Random.Range(cam.orthographicSize, -cam.orthographicSize));

        //destroy consumed
        organisms.Remove(organim);
        Destroy(organim);
        replacement.SetActive(true);
        organisms.Add(replacement);
    }

    #endregion

    #region Plant and Nut Management

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
                // Optional: Add a tiny amount of passive growth over time
                // plantScript.BoostGrowth(0.02f);
            }
        }
    }

    // Method for spawning a nut near a tree
    public bool SpawnNutNear(GameObject parentTree)
    {
        // Don't spawn new nuts if we're at the limit
        if (nuts.Count >= maxNutsInScene)
            return false;

        // Calculate random position near the parent tree
        float spawnDistance = nutSpreadDistance;
        Vector2 randomDirection = UnityEngine.Random.insideUnitCircle.normalized;
        Vector3 newPosition = parentTree.transform.position +
            new Vector3(randomDirection.x, randomDirection.y, 0) * spawnDistance;

        // Create new nut
        GameObject newNut = Instantiate(nutPrefab, newPosition, Quaternion.identity);

        // Add to our list of nuts
        nuts.Add(newNut);

        Debug.Log("New nut spawned near tree!");
        return true;
    }

    // Method to remove a nut when it's consumed
    public void RemoveNut(GameObject nut)
    {
        if (nuts.Contains(nut))
        {
            nuts.Remove(nut);
        }
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