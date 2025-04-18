using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class HungerScript : MonoBehaviour
{
    public String reference;
    public float hunger;
    public float hungerDeclineRate;
    public float feedThreshold;
    // public List<PrefabAssetType> potentialFoodTargets;
    public List<String> prey;
    public bool apex; //defines whether the organism has predators
    public List<String> predators;
    public int basePop;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (predators.IsUnityNull())
        {
            apex = true;
        }
        else {
            apex = false;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public String getName() {
        return reference;
    }

    public void changeHunger(float amount)
    {
        hunger = hunger + amount;
    }


}
