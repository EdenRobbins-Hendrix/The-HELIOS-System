using System.Collections.Generic;
using UnityEngine;

public class NPCDialogueScript : MonoBehaviour
{
    [SerializeField] public bool firstInteraction = true;
    [SerializeField] int repeatStartPosition;

    public string npcName;
    public List<DialogueAsset> dialogueAssets = new List<DialogueAsset>();
    [SerializeField]
    public int dialogueAssetsCurrentPosition;

    [HideInInspector]
    public int startPosition
    {
        get
        {
            if (firstInteraction)
            {
                firstInteraction = false;
                return 0;
            }
            else
            {
                return dialogueAssets[dialogueAssetsCurrentPosition].repeatStartPosition;
            }
        }
    }

    void Start()
    {
        dialogueAssetsCurrentPosition = 0;
    }

    public void incrementCurrentPosition()
    {
        if (dialogueAssetsCurrentPosition < dialogueAssets.Count - 1)
        {
            dialogueAssetsCurrentPosition = dialogueAssetsCurrentPosition + 1;
        }
    }


}
