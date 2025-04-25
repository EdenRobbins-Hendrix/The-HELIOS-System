using System;
using System.Collections;
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
    //Allows for segregation based upon organism type
    public List<GameObject> speciesPrefabs;

    [Serializable] //Serializable dictionary adaptation taken from PraetorBlue at https://discussions.unity.com/t/cant-see-dictionaries-in-inspector/801746
    public class OrganismNameObjectPair
    {
        public String Name;
        public List<GameObject> Instance;
    }

    public class OrganismNameGoalPair
    {
        public String name;
        public int goal;
    }

    public List<OrganismNameObjectPair> OrganismsSerialized = new List<OrganismNameObjectPair>();
    Dictionary<String, List<GameObject>> organisms = new Dictionary<String, List<GameObject>>();


    [Header("Plant Management")]
    public List<GameObject> plants = new List<GameObject>();
    public Dictionary<String, int> energyLevels;
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
        foreach (var kvp in OrganismsSerialized)
        {
            organisms[kvp.Name] = kvp.Instance;
        }
        // Animal management
        InvokeRepeating("decrementHungerInAllOrganisms", 5.0f, 5.0f);

        // Plant management - optional if you want passive growth
        InvokeRepeating("CheckPlantGrowth", 8.0f, 8.0f);

        StartCoroutine("checkPopulationsForWin");
    }

    // Update is called once per frame
    void Update()
    {
        // Empty update method
    }

    #region Animal Management

    //Every population manager should call this method in their start()
    public void InitializeEnergyLevels(List<String> plantTypes)
    {
        if (!energyLevels.IsUnityNull())
        {
            energyLevels.Clear();
        }
        else
        {
            energyLevels = new Dictionary<String, int>();
        }
        foreach (String plant in plantTypes)
        {
            energyLevels.Add(plant, 1);
        }
    }

    void decrementHungerInAllOrganisms()
    {
        foreach (List<GameObject> organismType in organisms.Values)
        {
            foreach (GameObject organism in organismType)
            {
                if (!(organism == null) && organism.TryGetComponent(out HungerScript s))
                {
                    float rate = s.hungerDeclineRate;
                    s.changeHunger(-rate);
                    setOrganismBehavior(organism);
                }
            }
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


        killOrganism(consumed, consumed.name.Split('(')[0]);

    }

    public GameObject chooseTarget(GameObject predator, HungerScript hScript)
    {
        GameObject target = null;
        float smallestDistance = float.PositiveInfinity;


        List<GameObject> allPotentialTargets = new List<GameObject>();
        foreach (List<GameObject> creatures in organisms.Values)
        {
            foreach (GameObject creature in creatures)
            {
                allPotentialTargets.Add(creature);
            }
        }

        bool eatsPlants = false;
        bool eatsNuts = false;
        // Add plants and nuts to potential targets if the predator eats them
        foreach (string targetName in hScript.prey)
        {
            //Important!!! In the fields in HungerScript for each prefab, ensure that the names match with what is given in the population manager!
            //This ensures the correct things have the correct labels in their name
            if (targetName.Contains("Plant") || targetName.Contains("Tree"))
            {
                // Add plants to the search list
                eatsPlants = true;
            }

            if (targetName.Contains("Nut") || targetName.Contains("Acorn"))
            {
                // Add nuts to the search list
                eatsNuts = true;
            }
        }
        if (eatsPlants)
        {
            allPotentialTargets.AddRange(plants);
        }
        if (eatsNuts)
        {
            allPotentialTargets.AddRange(nuts);
        }

        //choose best organism based on criteria (the closest one)
        foreach (GameObject potentialPrey in allPotentialTargets)
        {
            if (potentialPrey == predator) continue; // Skip self

            bool isValidTarget = false;

            foreach (string p in hScript.prey)
            {
                if (!(p == null) && (potentialPrey != null) && potentialPrey.name.Contains(p))
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

    // The new form of this is called from the Population Manager, it takes in the assigned population dictionary for the level
    // and ensures all values are matched in the organisms list and in the game world
    public void updatePopulations(Dictionary<String, int> reference)
    {
        foreach (String organism in reference.Keys)
        {
            if (organisms.TryGetValue(organism, out List<GameObject> objects))
            {
                int goalPop = reference[organism];
                if (objects.Count < goalPop)
                {
                    for (int i = goalPop - objects.Count; i < goalPop; i++)
                    {
                        // Get precies prefab from the list
                        foreach (GameObject organismPrefab in speciesPrefabs)
                        {
                            if (organismPrefab.name == organism)
                            {
                                // TODO: decide on a location to spawn the organism. Right now they all spawn on top of each other and then spread out
                                // TBH, I kind of like it
                                GameObject newOrg = Instantiate(organismPrefab);
                                objects.Add(newOrg);

                            }
                            else
                            {
                                Debug.Log("SERIOUS CRITICAL ERROR: Cannot find a species prefab for " + organism + " when attempting to increase population!!!");
                            }
                        }
                    }
                }
                else if (objects.Count > goalPop)
                {
                    for (int i = objects.Count - goalPop; i > goalPop; i--)
                    {
                        GameObject oldOrg = objects[-1];
                        objects.Remove(oldOrg);
                        Destroy(oldOrg);
                    }
                }
                objects.TrimExcess();
            }

        }
    }

    //    void increasePopulationofSpecies(GameObject species)
    //    {
    //        // Get count of organisms of species type
    //        int count = 0;
    //        float averageHunger = 0;
    //        foreach (GameObject organism in organisms)
    //        {
    //            if (organism.name.Contains(species.name)) // ensure the organism is of the organism type
    //            {
    //
    //                HungerScript h = organism.GetComponent<HungerScript>();
    //                if (h.hunger > h.starvingThreshold) // ensure the organism is not starving
    //                {
    //                    count = count + 1;
    //                    averageHunger = averageHunger + h.hunger;
    //                }
    //            }
    //        }

    //  calculate average hunger of population (if parents are starving then offspring should be starving)
    //        averageHunger = averageHunger / count;

    // spawn correct number of species
    //        int numOfSpawns = Mathf.FloorToInt(count / 2);
    //        for (int i = 0; i < numOfSpawns; i++)
    //        {
    //            //spawn the organism
    //            GameObject newOrganism = Instantiate(species);
    //            organisms.Add(newOrganism);
    //            newOrganism.GetComponent<HungerScript>().hunger = Mathf.FloorToInt(averageHunger); //set new organism hunger to average hunger of parents
    //
    //            print("Organism Spawned!!");
    //        }
    //    }

    public void killOrganism(GameObject creature, String name)
    {
        if (organisms.TryGetValue(name, out List<GameObject> creatures))
        {
            if (creatures.Count > 1 && creatures[1] != null)
            {
                GameObject old = creatures[1];
                GameObject replacement = Instantiate(old);
                replacement.SetActive(false);
                Camera cam = GameObject.FindAnyObjectByType<Camera>();
                replacement.transform.position = new Vector2(UnityEngine.Random.Range(cam.aspect, -cam.aspect), UnityEngine.Random.Range(cam.orthographicSize, -cam.orthographicSize));
                creatures.Remove(creature);
                Destroy(creature);
                replacement.SetActive(true);
                creatures.Add(replacement);
            }
        }
        //organisms.Remove(organim);
        //Destroy(organim);
        //replacement.SetActive(true);
        //organisms.Add(replacement);
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

    public void IncrementInUseEnergy(GameObject plant)
    {
        String name = plant.name.Split('(')[0];
        if (energyLevels.ContainsKey(name))
        {
            energyLevels.TryGetValue(name, out energy);
            energy++;
        }
        else
        {
            energyLevels.Add(name, 1);
        }
    }

    public void DecrementInUseEnergy(GameObject plant)
    {
        String name = plant.name.Split('(')[0];
        if (energyLevels.TryGetValue(name, out energy) && energy > 1)
        {
            energy--;
        }
        else if (!energyLevels.TryGetValue(name, out energy))
        {
            energyLevels.Add(name, 0);
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

    #region Level Management

    public List<OrganismNameGoalPair> organismNameGoalPairs = new List<OrganismNameGoalPair>();
    Dictionary<String, int> goalPopulations = new Dictionary<string, int>();


    // Compare population to goal population. Return true if acceptable difference. Return false otherwise. 
    bool comparePopulationToGoalPopulation(int acceptableDifference, string speciesName)
    {
        int goal = goalPopulations[speciesName];
        int currentPopulation = organisms[speciesName].Count;

        int difference = Math.Abs(goal - currentPopulation);
        if (difference < acceptableDifference)
        {
            return true;
        }
        else
        {
            Debug.Log("Comparison Failed. " + speciesName + " has a population of " + currentPopulation + " but the goal is " + goal);

            return false;

        }
    }

    // Compare all populations to goal populations. Return true if all species are in acceptable range. Return false otherwise. 
    bool compareAllPopulations()
    {
        List<string> organismNames = new List<string>(organisms.Keys); // get all organism names in species list
        foreach (String species in organismNames)
        {
            // Temporary solution. Acceptable difference is 10% of goal population
            int acceptableDifference = (int)Math.Ceiling(goalPopulations[species] * .1);
            bool a = comparePopulationToGoalPopulation(acceptableDifference, species);
            if (!a)
            {
                return false; // This funciton will return false if just one goal population is off
            }
        }
        return true; // We made it to the end, all populations are within acceptable bounds
    }

    // My plan: Use a coroutine to compare all populations, wait 2 seconds, repeat. Repeat this for 1 minute or something then if the condition is still true, I can end it and return true
    IEnumerator checkPopulationsForWin()
    {

        int iterations = 0;
        while (iterations < 20)
        {
            bool withinAllGoalPopulations = compareAllPopulations();

            if (withinAllGoalPopulations)
            {
                iterations = iterations + 1;
            }
            else
            {
                iterations = 0;
            }
            yield return new WaitForSeconds(2.0f);
            Debug.Log("Populations checked");

        }

        // Win Code
        Debug.Log("Win achieved!!");



        yield return null;
    }


    #endregion





}