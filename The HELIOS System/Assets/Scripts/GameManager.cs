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
            bool isFeeding = s.changeHunger(-rate);
            if (isFeeding)
            {
                GameObject target = chooseTarget(organism);
                //assign target
                MovementSteer m = organism.GetComponent<MovementSteer>();
                m.target = target;

                //set hunging bools
                m.isWandering = false;
                m.isHunting = true;
            }
        }
    }

    public GameObject chooseTarget(GameObject predator)
    {
        GameObject target = null;

        //choose best organism based on criteria. (i assume the closest one) (I made a method for this in case it gets more complicated later)
        foreach (GameObject potentialPrey in organisms)
        {
            if (potentialPrey.name.Contains("Prey"))
            {
                target = potentialPrey;
            }
            else
            {
                Debug.Log(predator.name + " has no organisms to eat!");
                target = predator; //hopefully this means that the organism will die? or maybe it just won't move
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
