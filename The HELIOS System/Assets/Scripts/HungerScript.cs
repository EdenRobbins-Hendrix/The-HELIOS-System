using UnityEngine;

public class HungerScript : MonoBehaviour
{
    public int hunger;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hunger = 10;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void decrementHunger(int amount)
    {
        hunger = hunger - amount;
    }
}
