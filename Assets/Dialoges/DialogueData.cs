using UnityEngine;

public enum Speaker
{
    Player,
    NPC,
    System
}

[System.Serializable]
public class DialogueLine
{
    [TextArea(2, 4)]
    public string text;
    public Speaker speaker;
    
    [Header("State Change")]
    public string setStateOnLine; // Какое состояние установить после этой фразы
}

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue System/Dialogue")]
public class DialogueData : ScriptableObject
{
    public string dialogueName;
    public DialogueLine[] lines;
    
    [Header("Conditions")]
    public string requiredState; // Какое состояние нужно для этого диалога
    public bool stateRequired = false;
}