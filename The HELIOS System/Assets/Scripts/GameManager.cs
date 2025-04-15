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
            s.decrementHunger(1);
        }
    }

    public void changeEnergy(int change)
    {
        energy += change;
        energyUI.text = "Energy Points: " + energy;
    }
}
