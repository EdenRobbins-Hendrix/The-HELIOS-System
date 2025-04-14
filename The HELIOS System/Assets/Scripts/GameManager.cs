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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void changeEnergy(int change) {
        energy += change;
        energyUI.text = "Energy Points: " + energy;
    }
}
