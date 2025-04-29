using UnityEngine;

[CreateAssetMenu(fileName = "DialogueAsset", menuName = "Scriptable Objects/DialogueAsset")]
public class DialogueAsset : ScriptableObject
{
    [TextArea]
    public string[] dialogue;
    public int repeatStartPosition;

}
