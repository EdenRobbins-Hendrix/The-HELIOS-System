using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
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

    public static DialogueManager Instance { get; private set; }

    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] GameObject dialoguePanel;
    [SerializeField] GameObject NPC;
    public static event Action OnDialogueStarted;
    public static event Action OnDialogueEnded;
    private bool inDialog;
    bool skipLineTriggered;

    public void Interact()
    {
        Debug.Log("Interact");
        if (inDialog)
        {
            Debug.Log("Skipping Line");
            SkipLine();
        }

        else
        {
            inDialog = true;
            NPCDialogueScript dialogueScript = NPC.GetComponent<NPCDialogueScript>();
            StartDialogue(dialogueScript.dialogueAssets[dialogueScript.dialogueAssetsCurrentPosition].dialogue, dialogueScript.startPosition, dialogueScript.npcName);
        }
    }

    public void StartDialogue(string[] dialogue, int startPosition, string name)
    {
        nameText.text = name + "...";
        dialoguePanel.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(RunDialogue(dialogue, startPosition));
    }

    IEnumerator RunDialogue(string[] dialogue, int startPosition)
    {
        skipLineTriggered = false;
        OnDialogueStarted?.Invoke();

        for (int i = startPosition; i < dialogue.Length; i++)
        {
            //dialogueText.text = dialogue[i];
            dialogueText.text = null;
            StartCoroutine(TypeTextUncapped(dialogue[i]));

            while (skipLineTriggered == false)
            {
                // Wait for the current line to be skipped
                yield return null;
            }
            skipLineTriggered = false;
        }

        OnDialogueEnded?.Invoke();
        dialoguePanel.SetActive(false);
        inDialog = false;
        NPC.GetComponent<NPCDialogueScript>().incrementCurrentPosition(); // this is temporary. more of a test really
    }

    public void SkipLine()
    {
        skipLineTriggered = true;
    }

    public void ShowDialogue(string dialogue, string name)
    {
        nameText.text = name + "...";
        StartCoroutine(TypeTextUncapped(dialogue));
        dialoguePanel.SetActive(true);
    }

    public void EndDialogue()
    {
        nameText.text = null;
        dialogueText.text = null;
        dialoguePanel.SetActive(false);
    }

    float charactersPerSecond = 90;

    IEnumerator TypeTextUncapped(string line)
    {
        float timer = 0;
        float interval = 1 / charactersPerSecond;
        string textBuffer = null;
        char[] chars = line.ToCharArray();
        int i = 0;

        while (i < chars.Length)
        {
            if (timer < Time.deltaTime)
            {
                textBuffer += chars[i];
                dialogueText.text = textBuffer;
                timer += interval;
                i++;
            }
            else
            {
                timer -= Time.deltaTime;
                yield return null;
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inDialog = false;

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }

    }
}
