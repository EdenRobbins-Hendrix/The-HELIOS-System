using UnityEngine;

public class HungerScript : MonoBehaviour
{
    public float hunger;
    public float hungerDeclineRate;
    public float feedThreshold;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hunger = 10;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void changeHunger(float amount)
    {
        hunger = hunger + amount;
    }
}
