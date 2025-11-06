using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public List<DialogueData> dialogues = new List<DialogueData>();
    public float interactionRadius = 2f;
    public float textDisplayTime = 3f;
    
    [Header("Floating Bubble Prefabs")]
    public GameObject playerBubblePrefab;
    public GameObject npcBubblePrefab;
    public GameObject systemBubblePrefab;
    
    [Header("Bubble Settings")]
    public Vector3 bubbleOffset = new Vector3(0, 2f, 0);
    public float bubbleHeight = 1.5f;

    [Header("Text Animation")]
    public bool useTypewriterEffect = true;
    public float typewriterSpeed = 0.05f; // Скорость печати (секунд на символ)

    [Header("Interaction Hint")]
    public GameObject interactionHintPrefab; // Префаб облачка с иконкой "E"
    public Vector3 hintOffset = new Vector3(0, 1.5f, 0); // Смещение над головой
    [Header("Animation Settings")]
    public float bounceAmount = 0.2f;
    public float animationSpeed = 2f;

    private bool isInRange = false;
    private bool isDialogueActive = false;
    private DialogueData currentDialogue;
    private int currentLineIndex = 0;
    private Coroutine displayCoroutine;
    private Coroutine typewriterCoroutine;
    private GameObject currentBubble;
    private TextMeshProUGUI currentBubbleText;
    private GameObject currentHint; // Текущее отображаемое облачко
    private Vector3 originalScale;

    public System.Action OnInteract;
    
    
    // Ссылка на игрока для позиционирования пузырька
    private Transform playerTransform;
    
    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        originalScale = transform.localScale;

        // Подписываемся на изменение состояния
        if (DialogueState.Instance != null)
        {
            DialogueState.Instance.OnStateChanged += OnGameStateChanged;
        }
        
                StartCoroutine(Bounce());
    }
    
    void OnDestroy()
    {
        if (DialogueState.Instance != null)
        {
            DialogueState.Instance.OnStateChanged -= OnGameStateChanged;
        }
    }
    
    void Update()
    {
        CheckPlayerDistance();
        HandleDialogueInput();
        UpdateBubblePosition();
        UpdateHintPosition();
    }
    
    void CheckPlayerDistance()
    {
        if (playerTransform == null) return;
        
        float distance = Vector2.Distance(transform.position, playerTransform.position);
        bool wasInRange = isInRange;
        isInRange = distance <= interactionRadius;
        
        if (isInRange && !wasInRange && !isDialogueActive)
        {
            ShowInteractionHint();
        }
        else if (!isInRange && wasInRange)
        {
            HideInteractionHint();
            if (isDialogueActive)
            {
                EndDialogue();
            }
        }
    }
    
    void HandleDialogueInput()
    {
        if (Input.GetKeyDown(KeyCode.E) && isInRange && !isDialogueActive)
        {
            StartAppropriateDialogue();
            OnInteract?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.E) && isDialogueActive)
        {
            ShowNextLine();
        }
    }
    
    void StartAppropriateDialogue()
    {
        DialogueData selectedDialogue = FindSuitableDialogue();
        
        if (selectedDialogue != null)
        {
            StartSpecificDialogue(selectedDialogue);
        }
        else if (dialogues.Count > 0)
        {
            StartSpecificDialogue(dialogues[0]);
        }
    }
    
    DialogueData FindSuitableDialogue()
    {
        foreach (DialogueData dialogue in dialogues)
        {
            if (CheckDialogueConditions(dialogue))
            {
                return dialogue;
            }
        }
        return null;
    }
    
    bool CheckDialogueConditions(DialogueData dialogue)
    {
        // Проверяем состояние если требуется
        if (dialogue.stateRequired && !string.IsNullOrEmpty(dialogue.requiredState))
        {
            if (DialogueState.Instance == null || !DialogueState.Instance.CheckState(dialogue.requiredState))
            {
                return false;
            }
        }
        
        return true;
    }
    
    public void StartSpecificDialogue(DialogueData dialogue)
    {
        if (dialogue == null || dialogue.lines.Length == 0) return;
        
        currentDialogue = dialogue;
        isDialogueActive = true;
        currentLineIndex = 0;
        HideInteractionHint();
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
            DialogueState.Instance?.SetState(line.setStateOnLine);
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
    
    IEnumerator Bounce()
    {
        while (true)
        {
            // Плавно растягиваем вверх и возвращаем
            float timer = 0f;
            while (timer < 1f)
            {
                timer += Time.deltaTime * animationSpeed;
                float bounce = Mathf.Sin(timer * Mathf.PI) * bounceAmount;
                transform.localScale = originalScale + Vector3.up * bounce;
                yield return null;
            }
        }
    }
    
    void CreateSpeechBubble(DialogueLine line)
    {
        // Уничтожаем предыдущий пузырёк
        if (currentBubble != null)
        {
            Destroy(currentBubble);
        }
        
        // Выбираем префаб в зависимости от говорящего
        GameObject bubblePrefab = GetBubblePrefab(line.speaker);
        if (bubblePrefab == null) return;
        
        // Определяем позицию пузырька
        Vector3 bubblePosition = GetBubblePosition(line.speaker);
        
        // Создаём пузырёк
        currentBubble = Instantiate(bubblePrefab, bubblePosition, Quaternion.identity);
        
        // Находим текстовый компонент
        currentBubbleText = currentBubble.GetComponentInChildren<TextMeshProUGUI>();
        if (currentBubbleText != null)
        {
            currentBubbleText.text = line.text;
        }
    }
    
    GameObject GetBubblePrefab(Speaker speaker)
    {
        switch (speaker)
        {
            case Speaker.Player: return playerBubblePrefab;
            case Speaker.NPC: return npcBubblePrefab;
            case Speaker.System: return systemBubblePrefab;
            default: return npcBubblePrefab;
        }
    }
    
    Vector3 GetBubblePosition(Speaker speaker)
    {
        switch (speaker)
        {
            case Speaker.Player:
                return playerTransform.position + bubbleOffset;
            case Speaker.NPC:
                return transform.position + bubbleOffset;
            case Speaker.System:
                // Системные сообщения по центру экрана или над NPC
                return transform.position + bubbleOffset + Vector3.up * bubbleHeight;
            default:
                return transform.position + bubbleOffset;
        }
    }

    void UpdateBubblePosition()
    {
        // Обновляем позицию пузырька если он активен
        if (currentBubble != null && currentLineIndex < currentDialogue.lines.Length)
        {
            DialogueLine currentLine = currentDialogue.lines[currentLineIndex];
            Vector3 newPosition = GetBubblePosition(currentLine.speaker);
            currentBubble.transform.position = newPosition;
        }
    }
    
    // Метод для обновления позиции облачка
    void UpdateHintPosition()
    {
        if (currentHint != null)
        {
            // Обновляем позицию относительно NPC
            currentHint.transform.position = transform.position + hintOffset;
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
        
        if (isInRange)
        {
            ShowInteractionHint();
        }
    }
    
    void OnGameStateChanged(string newState)
    {
        // Автоматически перезапускаем подходящий диалог при изменении состояния
        if (isInRange && !isDialogueActive)
        {
            DialogueData newDialogue = FindSuitableDialogue();
            if (newDialogue != null && newDialogue != currentDialogue)
            {
                StartSpecificDialogue(newDialogue);
            }
        }
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
    
    void ShowInteractionHint()
    {
        // Если облачко уже показано или префаба нет - выходим
        if (currentHint != null || interactionHintPrefab == null) return;
        
        // Создаём облачко над NPC
        Vector3 hintPosition = transform.position + hintOffset;
        currentHint = Instantiate(interactionHintPrefab, hintPosition, Quaternion.identity);
        
        // Делаем облачко дочерним объектом NPC чтобы двигалось вместе с ним
        currentHint.transform.SetParent(transform);
        
        Debug.Log("Показано облачко взаимодействия");
    }

    void HideInteractionHint()
    {
        if (currentHint != null)
        {
            Destroy(currentHint);
            currentHint = null;
        }
    }
    
    void EndDialogue()
    {
        isDialogueActive = false;
        currentLineIndex = 0;
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
    
    // Публичные методы
    public void ForceStartDialogue(DialogueData specificDialogue = null)
    {
        if (!isDialogueActive)
        {
            EndDialogue();
        }
        
        if (specificDialogue != null)
        {
            StartSpecificDialogue(specificDialogue);
        }
        else
        {
            StartAppropriateDialogue();
        }

    }
    
    public void ForceStartDialogueByName(string dialogueName)
    {
        DialogueData dialogue = dialogues.Find(d => d.dialogueName == dialogueName);
        if (dialogue != null)
        {
            ForceStartDialogue(dialogue);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
        
        // Показываем позицию пузырька NPC
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + bubbleOffset, 0.2f);
    }
}