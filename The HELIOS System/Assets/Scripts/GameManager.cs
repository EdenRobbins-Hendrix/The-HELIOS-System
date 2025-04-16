using System.Collections.Generic;
using TMPro;
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

    public TextMeshProUGUI energyUI;
    public int energy;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InvokeRepeating("decrementHungerInAllOrganisms", 5.0f, 5.0f);
    }

    // Update is called once per frame
    void Update()
    {

    }
    public List<GameObject> organisms;
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
        if (s.potentialFoodTargets.Contains(consumed))
        {
            Debug.Log("Valid food target");
            feed(consumer, consumed, energyAmount);

        }
        else
        {
            Debug.Log(consumer + " attempting to eat invalid organism!");
        }
    }
    public void feed(GameObject consumer, GameObject consumed, float energyAmount)
    {
        //increase hunger in consumer
        HungerScript s = consumer.GetComponent<HungerScript>();
        s.potentialFoodTargets.Remove(consumed);
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

        //choose best organism based on criteria. (i assume the closest one) (I made a method for this in case it gets more complicated later)
        foreach (GameObject potentialPrey in hScript.potentialFoodTargets)
        {
            float distance = Vector3.Distance(predator.transform.position, potentialPrey.transform.position);
            if (distance < smallestDistance)
            {
                smallestDistance = distance;
                target = potentialPrey;
            }
            else
            {
                Debug.Log(predator.name + " has no organisms to eat!");
                target = predator; //If there is no valid prey, then the predator will just remain still
            }
        }

        return target;
    }

    public void changeEnergy(int change)
    {
        energy += change;
        energyUI.text = "Energy Points: " + energy;
    }
}
