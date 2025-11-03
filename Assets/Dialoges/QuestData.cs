using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Quest", menuName = "Dialogue System/Quest Data")]
public class QuestData : ScriptableObject
{
    public string questName;
    
    public string requiredState; // Состояние, при котором квест активируется
    public Item[] requiredItems; // Нужные предметы
    public string completionState; // Состояние после завершения квеста
    public DialogueData completionDialogue; // Диалог после сдачи предметов
    public DialogueData failureDialogue; // Диалог если предметов нет
}