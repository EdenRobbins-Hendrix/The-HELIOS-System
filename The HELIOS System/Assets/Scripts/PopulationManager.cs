using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;
public class PopulationManager : MonoBehaviour
{
public Dictionary<String, int> populations = new Dictionary<String, int>();
public GameObject[] consumerPrefabs;

    void Start()
    {
        startingPopulations();
    }
    void startingPopulations() {
	populations.Add("Oak", 5);
	populations.Add("Hazelnut", 5);
	populations.Add("PoisonIvy", 10);
	populations.Add("Dandelion", 10);
	populations.Add("Beautyberry", 10);
	populations.Add("Hawk", 1);
	populations.Add("BlackBear", 1);
	populations.Add("Graywolf", 5);
	populations.Add("Squirrel", 22);
	populations.Add("Cottontail", 24);
	populations.Add("Robin", 28);
	populations.Add("Deer", 20);
	InvokeRepeating("updatePopulations", 1, 1);
}



int calculatePopulation(String organism) {
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
					pop = (int) MathF.Ceiling(((food/countF)-y)*x*(1-x));
					if (pop < 1) {
						pop = 1;
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
		int pop = calculatePopulation(entry.Key);
		if (pop > 0) {
			temp.Add(entry.Key, pop);
            Debug.Log(entry.Key + " " + pop);
        }
		else {
			temp.Add(entry.Key, entry.Value);
		}
    }
	populations = temp;
    }
}


