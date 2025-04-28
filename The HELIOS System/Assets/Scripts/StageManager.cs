using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;


/*
    The purpose of this script is to detect when the end of the level has been reached and to determine the "stage" of the level
    By stage, I mean for scripted events. Such as planting a tree, pointing out when a squirrel eats a nut. Things like that. 
    I want to really guide the player through the ecosystem, slowly building their knowledge

    By seperating this from the game manager, I am also hoping that this will reduce complexity. 
*/
public class StageManager : MonoBehaviour
{

    [Serializable]
    public class OrganismNameGoalPair
    {
        public String name;
        public int goal;
    }
    public GameObject NPC;

    public List<OrganismNameGoalPair> organismsGoalsSerialized = new List<OrganismNameGoalPair>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (var kvp in organismsGoalsSerialized)
        {
            goalPopulations[kvp.name] = kvp.goal;
        }

    }


    // Update is called once per frame
    void Update()
    {
        StartCoroutine("checkPopulationsForWin");


    }

    Dictionary<String, int> goalPopulations = new Dictionary<string, int>();


    // Compare population to goal population. Return true if acceptable difference. Return false otherwise. 
    bool comparePopulationToGoalPopulation(int acceptableDifference, string speciesName)
    {
        int goal = goalPopulations[speciesName];
        int currentPopulation = GameManager.Instance.organisms[speciesName].Count;

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
        List<string> organismNames = new List<string>(GameManager.Instance.organisms.Keys); // get all organism names in species list
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

    private void advanceStage()
    {
        NPC.GetComponent<NPCDialogueScript>().incrementCurrentPosition();
    }

}
