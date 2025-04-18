using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class HungerScript : MonoBehaviour
{
    public float hunger;
    public float hungerDeclineRate;
    public float feedThreshold;
    public float starvingThreshold; // This is when the parents will stop producing offspring
    // public List<PrefabAssetType> potentialFoodTargets;
    public List<String> potentialFoodTargetsNames;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void changeHunger(float amount)
    {
        hunger = hunger + amount;
        if (hunger <= 0)
        {
            GameManager.Instance.killOrganism(gameObject);
        }
    }


}
