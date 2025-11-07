using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CutsceneSequence : MonoBehaviour
{
    [System.Serializable]
    public class ObjectBlock
    {
        public string blockName;
        public List<CanvasGroup> objects = new List<CanvasGroup>(); // Images и TMP тексты
        public float delayBetweenObjects = 0.3f; // Задержка между появлением объектов
        public float displayDuration = 3f; // Время показа блока
        public bool waitForInput = false; // Ждать ввода вместо таймера
        public float fadeDuration = 0.5f; // Длительность плавного появления/исчезновения
    }

    [Header("Sequence Settings")]
    public List<ObjectBlock> objectBlocks = new List<ObjectBlock>();
    public float delayBetweenBlocks = 1f; // Пауза между блоками
    public bool skipOnClick = true; // Пропуск по клику
    public bool autoStart = true; // Автозапуск
    public string nextSceneName;

    [Header("References")]
    public CanvasGroup canvasGroup; // Основной Canvas Group

    private int currentBlockIndex = 0;
    private bool isPlaying = false;
    private Coroutine sequenceCoroutine;
    private ObjectBlock currentBlock;

    void Start()
    {
        // Скрываем все объекты в начале
        HideAllObjects();

        Fade.Instance.FadeFromBlack();

        if (autoStart)
        {
            StartSequence();
        }
    }

    void Update()
    {
        // Пропуск по клику
        if (skipOnClick && Input.anyKeyDown && isPlaying)
        {
            SkipToNextBlock();
        }
    }

    // Запустить последовательность
    public void StartSequence()
    {
        if (isPlaying) return;

        isPlaying = true;
        currentBlockIndex = 0;
        sequenceCoroutine = StartCoroutine(SequenceRoutine());
    }

    // Остановить последовательность
    public void StopSequence()
    {
        if (!isPlaying) return;

        isPlaying = false;
        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
            sequenceCoroutine = null;
        }

        HideAllObjects();
    }

    private IEnumerator SequenceRoutine()
    {
        while (currentBlockIndex < objectBlocks.Count)
        {
            currentBlock = objectBlocks[currentBlockIndex];

            Debug.Log($"Starting block: {currentBlock.blockName}");

            // Показываем объекты блока по очереди
            yield return StartCoroutine(ShowBlockObjects(currentBlock));

            // Ожидание перед скрытием блока
            if (currentBlock.waitForInput)
            {
                yield return StartCoroutine(WaitForInput());
            }
            else
            {
                yield return new WaitForSeconds(currentBlock.displayDuration);
            }

            // Скрываем объекты блока
            yield return StartCoroutine(HideBlockObjects(currentBlock));

            // Пауза между блоками
            yield return new WaitForSeconds(delayBetweenBlocks);

            currentBlockIndex++;
        }

        // Завершение последовательности
        Debug.Log("Cutscene sequence completed");
        isPlaying = false;

        Fade.Instance.FadeToBlack();
        Invoke(nameof(NextScene), 1f);
    }
    
    void NextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }

    // Показать объекты блока по очереди
    private IEnumerator ShowBlockObjects(ObjectBlock block)
    {
        foreach (CanvasGroup obj in block.objects)
        {
            if (currentBlock != block)  yield break;

            if (obj != null)
            {
                yield return StartCoroutine(FadeObject(obj, 0f, 1f, block.fadeDuration));
                yield return new WaitForSeconds(block.delayBetweenObjects);
            }
        }
    }

    // Скрыть объекты блока
    private IEnumerator HideBlockObjects(ObjectBlock block)
    {
        foreach (CanvasGroup obj in block.objects)
        {
            if (obj != null)
            {
                StartCoroutine(FadeObject(obj, 1f, 0f, block.fadeDuration));
            }
        }

        // Ждем пока все объекты скроются
        yield return new WaitForSeconds(block.fadeDuration);
    }

    // Плавное изменение прозрачности объекта
    private IEnumerator FadeObject(CanvasGroup obj, float from, float to, float duration)
    {
        if (obj == null) yield break;

        float timer = 0f;
        obj.alpha = from;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            obj.alpha = Mathf.Lerp(from, to, timer / duration);
            yield return null;
        }

        obj.alpha = to;
    }

    // Ожидание ввода
    private IEnumerator WaitForInput()
    {
        bool inputReceived = false;

        while (!inputReceived)
        {
            if (Input.GetMouseButtonDown(0) || Input.anyKeyDown)
            {
                inputReceived = true;
            }
            yield return null;
        }
    }

    // Пропустить к следующему блоку
    public void SkipToNextBlock()
    {
        if (!isPlaying) return;

        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
        }

        // Скрываем текущий блок
        if (currentBlockIndex < objectBlocks.Count)
        {
            StartCoroutine(HideBlockObjects(objectBlocks[currentBlockIndex]));
        }

        // Переходим к следующему блоку
        currentBlockIndex++;
        if (currentBlockIndex < objectBlocks.Count)
        {
            sequenceCoroutine = StartCoroutine(SequenceRoutine());
        }
        else
        {
            currentBlock = null;
            isPlaying = false;
            Debug.Log("Cutscene sequence completed (skipped)");
            Fade.Instance.FadeToBlack();
            Invoke(nameof(NextScene), 1f);
        }
    }

    // Скрыть все объекты
    private void HideAllObjects()
    {
        foreach (ObjectBlock block in objectBlocks)
        {
            foreach (CanvasGroup obj in block.objects)
            {
                if (obj != null)
                {
                    obj.alpha = 0f;
                }
            }
        }
    }

    // Публичные методы для управления
    public void AddBlock(ObjectBlock newBlock)
    {
        objectBlocks.Add(newBlock);
    }

    public void InsertBlock(int index, ObjectBlock newBlock)
    {
        if (index >= 0 && index <= objectBlocks.Count)
        {
            objectBlocks.Insert(index, newBlock);
        }
    }

    public void RemoveBlock(int index)
    {
        if (index >= 0 && index < objectBlocks.Count)
        {
            objectBlocks.RemoveAt(index);
        }
    }

    // Для отладки в редакторе
    [ContextMenu("Start Cutscene")]
    private void StartCutsceneEditor()
    {
        if (Application.isPlaying)
        {
            StartSequence();
        }
    }

    [ContextMenu("Stop Cutscene")]
    private void StopCutsceneEditor()
    {
        if (Application.isPlaying)
        {
            StopSequence();
        }
    }
}