using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class HungerScript : MonoBehaviour
{
    public String reference;
    [SerializeField]
    private float hunger;
    public float hungerDeclineRate;
    public float feedThreshold;
    public float starvingThreshold; // This is when the parents will stop producing offspring
    // public List<PrefabAssetType> potentialFoodTargets;
    public List<String> prey;
    public bool apex; //defines whether the organism has predators
    public List<String> predators;
    public int basePop;
    public int foodAvailableUponConsumption;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (predators.IsUnityNull())
        {
            apex = true;
        }
        else
        {
            apex = false;
        }
        hunger = 10.0f;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public String getName()
    {
        return reference;
    }

    public void changeHunger(float amount)
    {
        hunger = hunger + amount;
        // Debug.Log(name + "'s new hunger: " + hunger);
        if (hunger <= 0)
        {
            GameManager.Instance.killOrganism(gameObject, gameObject.name.Split('(')[0]);
            Destroy(gameObject);
        }

        // if (hunger < -10)
        // {
        //     Destroy(gameObject);
        // }
    }

    public float getHunger()
    {
        return this.hunger;
    }


}
