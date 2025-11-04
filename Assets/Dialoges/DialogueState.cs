using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class DialogueState : MonoBehaviour
{
    public static DialogueState Instance;
    public string currentState = "start"; // Текущее состояние игры
    public PlayerController playerController;
    public Transform playerTpPoint;
    public FearAI fearAI;
    public RageAI rageAI;
    public FakeResentment resentment;
    public PuddleSpawner ps;
    public Transform monsterTpPoint;
    public string sceneToLoad;
    public ElevAnim[] elevAnims;
    
    // Событие при изменении состояния
    public System.Action<string> OnStateChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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

        if (newState == "win")
        {
            Fade.Instance.FadeInOut(1f);
            Invoke(nameof(StartWin),1f);
        }

        if (newState == "end")
        {
            foreach (ElevAnim elevAnim in elevAnims)
            {
                elevAnim.PlayBackward();
                Fade.Instance.FadeToBlack(1f);
                Invoke(nameof(LoadSceneByName), 1f);
            }
        }
    }

    public bool CheckState(string requiredState)
    {
        return currentState == requiredState;
    }

    public void LoadSceneByName()
    {
        if (sceneToLoad != null)
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
    
    public void StartWin()
    {
        if (playerController != null)
        {
            playerController.transform.position = playerTpPoint.transform.position;
        }
        if (fearAI != null)
        {
            fearAI.transform.position = monsterTpPoint.transform.position;
            fearAI.StartDying();
        }
        if (rageAI != null)
        {
            rageAI.transform.position = monsterTpPoint.transform.position;
            rageAI.StartDying();
        }
        if (resentment != null)
        {
            resentment.transform.position = monsterTpPoint.transform.position;
            resentment.StartDying();
        }
        if (ps != null)
        {
            ps.DeleteAll();
        }
    }
}