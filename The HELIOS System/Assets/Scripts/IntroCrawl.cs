using TMPro;
using UnityEngine;
using System.Collections;
using System;

public class IntroCrawl : MonoBehaviour
{
    public float charactersPerSecond;
    public TextMeshProUGUI Stage1;
    public TextMeshProUGUI Stage2;
    public TextMeshProUGUI Stage3;
    public TextMeshProUGUI Stage4;
    public TextMeshProUGUI Stage5;
    TextMeshProUGUI CurrentStage;
    string Part1;
    string Part2;
    string Part3;
    string Part4;
    string Part5;
    string TextMemory;
    int TextStage;
    bool Talking;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Talking = false;
        TextStage = 1;
        CurrentStage = Stage1;
        Part1 = Stage1.text;
        Stage1.text = "";
        Part2 = Stage2.text;
        Stage2.text = "";
        Part3 = Stage3.text;
        Stage3.text = "";
        Part4 = Stage4.text;
        Stage4.text = "";
        Part5 = Stage5.text;
        Stage5.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && !Talking)
        {

            if (TextStage == 1)
            {
                Debug.Log("Stage1");
                TextMemory = Part1;
            }
            else if (TextStage == 2)
            {
                Debug.Log("Stage2");
                CurrentStage = Stage2;
                TextMemory = Part2;
            }
            else if (TextStage == 3)
            {
                Debug.Log("Stage3");
                CurrentStage = Stage3;
                TextMemory = Part3;
            }
            else if (TextStage == 4)
            {
                Debug.Log("Stage4");
                CurrentStage = Stage4;
                TextMemory = Part4;
            }
            else if (TextStage == 5)
            {
                Debug.Log("Stage5");
                CurrentStage = Stage5;
                TextMemory = Part5;
            }
            else
            {
                Debug.Log("Begin gameplay");
                Initiate.Fade("LevelSelect", Color.white, 0.5f);
            }
            Debug.Log("Initiate text " + TextMemory);
            StartCoroutine(TypeTextUncapped(TextMemory));
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Text is crawling...");
        }

    }

    IEnumerator TypeTextUncapped(string line)
    {
        Talking = true;
        float timer = 0;
        float interval = 1 / charactersPerSecond;
        string textBuffer = null;
        char[] chars = line.ToCharArray();
        Debug.Log(line);
        int i = 0;

        while (i < chars.Length)
        {
            if (timer < Time.deltaTime)
            {
                textBuffer += chars[i];
                CurrentStage.text = textBuffer;
                timer += interval;
                i++;
                Debug.Log(textBuffer);
            }
            else
            {
                timer -= Time.deltaTime;
                yield return null;
            }
        }
        Talking = false;
        TextStage++;
        if (TextStage > 5)
        {
            Debug.Log("Begin gameplay");
            Initiate.Fade("SampleScene", Color.white, 0.5f);
        }
        StopAllCoroutines();
    }
}
