using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class MonologueTrigger : MonoBehaviour
{
    [Header("Monologue Settings")]
    public DialogueData dialogue;
    public float textDisplayTime = 3f;
    
    [Header("Floating Bubble Prefabs")]
    public GameObject bubblePrefab;
    
    [Header("Bubble Settings")]
    public Vector3 bubbleOffset = new Vector3(0, 2f, 0);

    [Header("Text Animation")]
    public bool useTypewriterEffect = true;
    public float typewriterSpeed = 0.05f; // Скорость печати (секунд на символ)
    
    private DialogueData currentDialogue;
    private int currentLineIndex = 0;
    private Coroutine displayCoroutine;
    private Coroutine typewriterCoroutine;
    private GameObject currentBubble;
    private TextMeshProUGUI currentBubbleText;
    private string stateToSet;

    public System.Action OnInteract;
    
    
    // Ссылка на игрока для позиционирования пузырька
    private Transform playerTransform;
    
    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Подписываемся на изменение состояния
        if (DialogueState.Instance != null)
        {
            DialogueState.Instance.OnStateChanged += OnGameStateChanged;
            DialogueState.Instance.OnMonologUpdate += ForceEndDialogue;
        }
    }
    
    void OnDestroy()
    {
        if (DialogueState.Instance != null)
        {
            DialogueState.Instance.OnStateChanged -= OnGameStateChanged;
            DialogueState.Instance.OnMonologUpdate -= ForceEndDialogue;
        }
    }

    void Update()
    {
        UpdateBubblePosition();
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (dialogue.stateRequired && !string.IsNullOrEmpty(dialogue.requiredState))
            {
                if (DialogueState.Instance != null &&
                    DialogueState.Instance.CheckState(dialogue.requiredState))
                {
                    StartSpecificDialogue(dialogue);
                    DialogueState.Instance.SetActiveMonologData(dialogue);
                    OnInteract?.Invoke();
                    GetComponent<BoxCollider2D>().enabled = false;
                }
            }
        }
    }
    
    public void StartSpecificDialogue(DialogueData dialogue)
    {
        if (dialogue == null || dialogue.lines.Length == 0) return;
        
        currentDialogue = dialogue;
        currentLineIndex = 0;
        ShowLine(currentLineIndex);
        
        Debug.Log($"Начат диалог: {dialogue.dialogueName}");
    }

    void ShowLine(int lineIndex)
    {
        DialogueLine line = currentDialogue.lines[lineIndex];

        // Создаём пузырёк над тем кто говорит
        CreateSpeechBubble(line);

        // Устанавливаем состояние если указано
        if (!string.IsNullOrEmpty(line.setStateOnLine))
        {
            stateToSet = line.setStateOnLine;
        }

        if (useTypewriterEffect)
        {
            // Останавливаем предыдущую анимацию если есть
            if (typewriterCoroutine != null)
                StopCoroutine(typewriterCoroutine);

            // Запускаем анимацию печати
            typewriterCoroutine = StartCoroutine(TypewriterEffect(line.text));
        }
        else
        {
            // Старое поведение - сразу весь текст
            displayCoroutine = StartCoroutine(HideLineAfterDelay());
        }
    }

    IEnumerator TypewriterEffect(string text)
    {
        if (currentBubbleText == null) yield break;

        // Очищаем текст и начинаем анимацию
        currentBubbleText.text = "";

        // Печатаем по одному символу
        foreach (char c in text)
        {
            currentBubbleText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        // После завершения печати ждём и переходим к следующей фразе
        yield return new WaitForSeconds(textDisplayTime);
        ShowNextLine();
    }
    
    void CreateSpeechBubble(DialogueLine line)
    {
        // Уничтожаем предыдущий пузырёк
        if (currentBubble != null)
        {
            Destroy(currentBubble);
        }
        
        // Определяем позицию пузырька
        Vector3 bubblePosition = GetBubblePosition();
        
        // Создаём пузырёк
        currentBubble = Instantiate(bubblePrefab, bubblePosition, Quaternion.identity);
        
        // Находим текстовый компонент
        currentBubbleText = currentBubble.GetComponentInChildren<TextMeshProUGUI>();
        if (currentBubbleText != null)
        {
            currentBubbleText.text = line.text;
        }
    }
    
    Vector3 GetBubblePosition()
    {
        return playerTransform.position + bubbleOffset;
    }

    void UpdateBubblePosition()
    {
        // Обновляем позицию пузырька если он активен
        if (currentBubble != null && currentLineIndex < currentDialogue.lines.Length)
        {
            DialogueLine currentLine = currentDialogue.lines[currentLineIndex];
            Vector3 newPosition = GetBubblePosition();
            currentBubble.transform.position = newPosition;
        }
    }
    
    IEnumerator HideLineAfterDelay()
    {
        yield return new WaitForSeconds(textDisplayTime);
        ShowNextLine();
    }
    
    void ShowNextLine()
    {
        // Останавливаем все корутины
        if (displayCoroutine != null)
            StopCoroutine(displayCoroutine);
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);
        
        currentLineIndex++;
        
        if (currentLineIndex < currentDialogue.lines.Length)
        {
            ShowLine(currentLineIndex);
        }
        else
        {
            OnDialogueComplete();
        }
    }
    
    void OnDialogueComplete()
    {
        Debug.Log($"Диалог завершен: {currentDialogue.dialogueName}");
        EndDialogue();

        if (!string.IsNullOrEmpty(stateToSet))
        {
            DialogueState.Instance?.SetState(stateToSet);
            stateToSet = null;
        }
    }
    
    void OnGameStateChanged(string newState)
    {
        if (newState == "end")
        {
            EndDialogue();
            gameObject.SetActive(false);
        }
        if (newState == "win")
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        currentDialogue = null;

        // Останавливаем все корутины
        if (displayCoroutine != null)
            StopCoroutine(displayCoroutine);
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        if (currentBubble != null)
        {
            Destroy(currentBubble);
            currentBubble = null;
        }
    }
    
    void ForceEndDialogue()
    {
        if (DialogueState.Instance.activeMonologData != dialogue)
        {
            EndDialogue();
        }
        ;
    }
}