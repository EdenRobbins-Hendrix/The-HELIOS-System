using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;
using Unity.VisualScripting;
public class PopulationManager : MonoBehaviour
{
public Dictionary<String, int> populations = new Dictionary<String, int>();
public GameObject[] consumerPrefabs;

[Serializable]
public class Organisms {
        public String Name;
        public int InitialPopulation;
		public bool IsPlant;
    }

    public List<Organisms> OrganismsSerialized = new List<Organisms>();


    void Start()
    {
		List<String> plants = new List<String>();
		foreach (Organisms organism in OrganismsSerialized) {
			populations.Add(organism.Name, organism.InitialPopulation);
			if (organism.IsPlant) {
				plants.Add(organism.Name);
			}
		}
		GameManager.Instance.InitializeEnergyLevels(plants);
		InvokeRepeating("updatePopulations", 10, 10);
    }
    
int calculatePopulation(String organism) {
		Dictionary<String, int> energyLevels = GameManager.Instance.energyLevels;
        Debug.Log("Calculating");
		int pop = 0;
		foreach (GameObject thing in consumerPrefabs) {
			if (thing.TryGetComponent(out HungerScript profile)) {
                Debug.Log("Valid organism");
                String[] names = thing.name.Split('(');
                names = names[0].Split(' ');
                Debug.Log(1+ organism + " " + 2 + names[0]);
				if (names[0].Equals(organism)) {
                    Debug.Log("Match!");
					List<String> prey =  profile.prey;
					List<String> predators = profile.predators;
					int x = profile.basePop; //For calculating prey-predator
					int y = 0; //For calculating prey-predator
					int food = 0; //How many available food sources
					int countF = 0; //How many unique food sources
 					int eater = 0; //How many predators
					int countE = 0; //How many unique predators
					foreach (String p in prey) {
					if (populations.TryGetValue(p, out int n)) {
                            if (p.Contains("Plant") || p.Contains("Tree")) {
								foreach (String plant in energyLevels.Keys) {
									if (plant.Equals(p)){
										energyLevels.TryGetValue(plant, out int energy);
										n *= energy;
									}
								}
							}
							Debug.Log(p + " " + n);
							food += n;
							countF++;					
						}
                    }
					if (!profile.apex) {
						foreach (String p in predators) {
							if (populations.TryGetValue(p, out int n)) {
                                Debug.Log(p + " " + n);
								eater += n;
								countE++;
							}
                        }
                    if (countE < 1) {
                        countE = 1;
                    }
					y = eater/countE;
					pop = (int) MathF.Ceiling(((food/countF)-y)+x);
					if (pop < 1) {
						pop = 1;
					}
					else if (pop > 50) {
						pop = 50;
					}
					return pop;}
				}
			}
		}
		return pop;	
}

void updatePopulations() {
    Debug.Log("Analyzing...");
	Dictionary<String, int> temp = new Dictionary<String, int>();
	foreach (KeyValuePair<String, int> entry in populations) {
		if (!entry.Key.IsUnityNull()){
			int pop = calculatePopulation(entry.Key);
			if (pop > 0) {
				temp.Add(entry.Key, pop);
            	Debug.Log(entry.Key + " " + pop);
        	}
			else {
				temp.Add(entry.Key, entry.Value);
			}
		}
    }
	populations = temp;
	GameManager.Instance.updatePopulations(populations);
    }
}


