using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


/*
    The purpose of this script is to detect when the end of the level has been reached and to determine the "stage" of the level
    By stage, I mean for scripted events. Such as planting a tree, pointing out when a squirrel eats a nut. Things like that. 
    I want to really guide the player through the ecosystem, slowly building their knowledge

    By seperating this from the game manager, I am also hoping that this will reduce complexity. 

    I have thought about this significantly, and I feel like the only solution to having a narrative that really incorporates the gameplay
    and will build up slowly is by having a unique stage manager for each level that will carefully manage the stage the player is in. 
    This allows the narrative and the gameplay to play off of each other. 
    EX: 
        AI explains about plants and their role with the sun, tells player to plant a plant
        Player plants a plant
        AI can then explain how plants are the foundation of an ecosystem and introduce an herbivore to eat the plant. 
        Player can observe the new interaction
*/



public class StageManager2 : MonoBehaviour
{

    public int currentStage;

    [Serializable]
    public class OrganismNameGoalPair
    {
        public String name;
        public int goal;
    }

    public List<OrganismNameGoalPair> organismsGoalsSerialized = new List<OrganismNameGoalPair>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentStage = 0;
        foreach (var kvp in organismsGoalsSerialized)
        {
            goalPopulations[kvp.name] = kvp.goal;
        }
        DialogueManager.Instance.Interact();

    }

    // Update is called once per frame
    void Update()
    {
        if (currentStage == 0)
        {
            if (GameManager.Instance.organisms["Squirrel"].Count >= 10 && GameManager.Instance.organisms["BeautyBerry"].Count >= 5)
            {
                advanceStage();
            }

        }
        else if (currentStage == 1)
        {
            if (DialogueManager.Instance.inDialog!)
            {
                endLevel();
            }




        }


        // I will have to work out appropriate levels to check for when the previous stage is working
        // StartCoroutine("checkPopulationsForWin");


    }

    IEnumerator endGameInXSeconds(float duration)
    {
        yield return new WaitForSeconds(duration);

        endLevel();
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
        // increment the current stage
        currentStage = currentStage + 1;

        // advance npc dialog to next set of dialog
        DialogueManager.Instance.incrementDialogueStage();

        // Go ahead and start the dialog
        DialogueManager.Instance.Interact();

        // advance goals

    }

    private void endLevel()
    {
        //play some kind of win music / celebration. IDK, just some user feedback

        // when it ends, return to the level select screen
        SceneManager.LoadScene("Menu");
    }

}
