using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using Unity.VisualScripting;

public class DialogueState : MonoBehaviour
{
    public static DialogueState Instance;
    public string currentState = "start"; // Текущее состояние игры
    public PlayerController playerController;
    public Transform playerTpPoint;
    public FearAI fearAI;
    public RageAI rageAI;
    public FakeResentment fakeMonster;
    public PuddleSpawner ps;
    public Transform monsterTpPoint;
    public string sceneToLoad;
    public ElevAnim[] elevAnims;
    public AudioClip deathClip;
    public AudioClip elevOpenClip;
    public AudioClip elevCloseClip;
    public AudioSource pastMusic;
    public DialogueData activeMonologData;
    public string currentPlace;


    // Событие при изменении состояния
    public System.Action<string> OnStateChanged;
    public System.Action OnMonologUpdate;
    public System.Action<string> OnTeleport;
    
    void Start()
    {
        PlaySound(elevCloseClip);
        if (pastMusic != null)
            pastMusic.Stop();
    }

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
            Invoke(nameof(StartWin), 1f);
        }

        if (newState == "end")
        {

            ElevButtonAnim ElevCanvas = FindFirstObjectByType<ElevButtonAnim>();
            ElevCanvas.gameObject.SetActive(true);
            ElevCanvas.PlayForward();
            foreach (ElevAnim elevAnim in elevAnims)
            {
                
                Invoke(nameof(LoadSceneByName), 5f);
            }
        }
    }

    public bool CheckState(string requiredState)
    {
        return currentState == requiredState;
    }

    public IEnumerator LoadSceneByName()
    {
        foreach (ElevAnim elevAnim in elevAnims)
        {
            elevAnim.PlayBackward();
            PlaySound(elevOpenClip);
            Fade.Instance.FadeToBlack(1f);
        }
        yield return new WaitForSeconds(1f);
        if (sceneToLoad != null)
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    public void SetActiveMonologData(DialogueData momolog)
    {
        activeMonologData = momolog;
        OnMonologUpdate?.Invoke();
    }

    public void Teleport(string teleportTo)
    {
            currentPlace = teleportTo;
            if (currentPlace == "past" && pastMusic != null)
                pastMusic.Play();
            if (currentPlace == "present" && pastMusic != null)
                pastMusic.Stop();
            if (fearAI != null)
                fearAI.UpdateLocation(teleportTo);
            if (rageAI != null)
                rageAI.UpdateLocation(teleportTo);
            if (ps != null)
                ps.UpdateLocation(teleportTo);
            OnTeleport.Invoke(currentPlace);
                
    }

    public void StartWin()
    {
        if (playerController != null)
        {
            playerController.transform.position = playerTpPoint.transform.position;
        }
        if (fearAI != null)
            Destroy(fearAI.gameObject);
        if (rageAI != null)
            Destroy(rageAI.gameObject);
        if (fakeMonster != null)
        {
            foreach(GameObject npc in GameObject.FindGameObjectsWithTag("NPC"))
            {
                Destroy(npc);
            }
            fakeMonster.transform.position = monsterTpPoint.transform.position;
            fakeMonster.StartDying();
            PlaySound(deathClip);
        }
        if (ps != null)
        {
            ps.DeleteAll();
        }
    }
    
    void PlaySound(AudioClip clip)
    {
        if (clip != null)
            GetComponent<AudioSource>().PlayOneShot(clip);
    }
}