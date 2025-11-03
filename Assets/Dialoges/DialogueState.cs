using UnityEngine;
using System.Collections.Generic;

public class DialogueState : MonoBehaviour
{
    public static DialogueState Instance;
    public string currentState = "start"; // Текущее состояние игры
    
    // Событие при изменении состояния
    public System.Action<string> OnStateChanged;

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
    
    public void SetState(string newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            Debug.Log($"Состояние диалогов изменилось на: {newState}");
            OnStateChanged?.Invoke(newState);
        }
    }
    
    public bool CheckState(string requiredState)
    {
        return currentState == requiredState;
    }
}