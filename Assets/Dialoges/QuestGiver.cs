// QuestGiver.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuestGiver : MonoBehaviour
{
    public QuestData quest;
    public DialogueTrigger dialogueTrigger;
    public InventorySystem playerInventory;
    
    private bool isQuestActive = false;
    private bool isQuestCompleted = false;
    
    void Start()
    {
        // Находим ссылки если не установлены
        if (playerInventory == null)
            playerInventory = FindFirstObjectByType<InventorySystem>();
            
        if (dialogueTrigger == null)
            dialogueTrigger = GetComponent<DialogueTrigger>();

        // Подписываемся на изменения состояния
        if (DialogueState.Instance != null)
        {
            DialogueState.Instance.OnStateChanged += OnStateChanged;
        }
        
        if (dialogueTrigger != null)
        {
            dialogueTrigger.OnInteract += OnDilogueInteract;
        }
    }
    
    void OnDestroy()
    {
        if (DialogueState.Instance != null)
        {
            DialogueState.Instance.OnStateChanged -= OnStateChanged;
        }

        if (dialogueTrigger != null)
        {
            dialogueTrigger.OnInteract -= OnDilogueInteract;
        }
    }

    void OnStateChanged(string newState)
    {
        CheckQuestActivation(newState);
        CheckQuestCompletion(newState);
    }
    
    void OnDilogueInteract()
    {
        CheckQuestCompletion(DialogueState.Instance.currentState);
    }
    
    void CheckQuestActivation(string state)
    {
        // Если квест ещё не активен и состояние совпадает с требуемым
        if (!isQuestActive && !isQuestCompleted && state == quest.requiredState)
        {
            ActivateQuest();
        }
    }
    
    void CheckQuestCompletion(string state)
    {
        // Если квест активен и игрок пытается сдать предметы
        if (isQuestActive && state == quest.requiredState)
        {
            Debug.Log("Попытка сдать квест");
            TryCompleteQuest();
        }
    }
    
    void ActivateQuest()
    {
        isQuestActive = true;
        Debug.Log($"Квест активирован: {quest.questName}");
    }
    
    public void TryCompleteQuest()
    {
        if (!isQuestActive || isQuestCompleted) return;
        
        // Проверяем есть ли у игрока все нужные предметы
        if (HasAllRequiredItems())
        {
            CompleteQuest();
        }
        else
        {
            ShowFailureDialogue();
        }
    }
    
    bool HasAllRequiredItems()
    {
        if (playerInventory == null) return false;
        
        // Проверяем каждый требуемый предмет
        foreach (Item requiredItem in quest.requiredItems)
        {
            if (playerInventory.HasItemByName(requiredItem.itemName) == -1)
            {
                Debug.Log("Нет необходимых предметов");

                return false;
            }
        }
        Debug.Log("Предметы есть");

        return true;
    }
    
    void CompleteQuest()
    {
        // Убираем предметы из инвентаря
        RemoveRequiredItems();
        
        // Меняем состояние
        DialogueState.Instance.SetState(quest.completionState);
        
        isQuestActive = false;
        isQuestCompleted = true;
        
        // Запускаем диалог завершения
        if (quest.completionDialogue != null && dialogueTrigger != null)
        {
            dialogueTrigger.ForceStartDialogue(quest.completionDialogue);
        }
        
        Debug.Log($"Квест завершен: {quest.questName}");
    }
    
    void RemoveRequiredItems()
    {
        // Для каждого требуемого предмета
        foreach (Item requiredItem in quest.requiredItems)
        {
            int itemIndex = playerInventory.HasItemByName(requiredItem.itemName);

            if (itemIndex != -1)
            {
                playerInventory.RemoveItem(itemIndex);
            }
        }
    }
    
    void ShowFailureDialogue()
    {
        if (quest.failureDialogue != null && dialogueTrigger != null)
        {
            dialogueTrigger.ForceStartDialogue(quest.failureDialogue);
        }
    }
    
    // Метод для вызова из диалога или других скриптов
    public void CheckQuest()
    {
        if (isQuestActive)
        {
            TryCompleteQuest();
        }
    }
}