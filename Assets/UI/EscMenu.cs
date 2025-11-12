using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EscapeMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject menuPanel;          // Панель всего меню
    public GameObject gamePanel;          // Панель игрового UI
    public Button continueButton;         // Кнопка "Продолжить"
    public Button mainMenuButton;         // Кнопка "Выйти в главное меню"

    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.Escape;
    public bool pauseTime = true;

    private bool isMenuActive = false;

    void Start()
    {
        // Назначаем обработчики кнопок
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(ContinueGame);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }

        // Скрываем меню при старте
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }
    }

    void Update()
    {
        // Открываем/закрываем меню по клавише Escape
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleMenu();
        }
    }

    // Переключение меню
    public void ToggleMenu()
    {
        isMenuActive = !isMenuActive;

        if (menuPanel != null)
        {
            menuPanel.SetActive(isMenuActive);
            gamePanel.SetActive(!isMenuActive);
        }

        // Останавливаем или возобновляем время
        if (pauseTime)
        {
            Time.timeScale = isMenuActive ? 0f : 1f;
        }
    }

    // Продолжить игру
    public void ContinueGame()
    {
        isMenuActive = false;

        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
            gamePanel.SetActive(true);
        }

        // Возобновляем время
        if (pauseTime)
        {
            Time.timeScale = 1f;
        }
    }

    // Выйти в главное меню
    public void GoToMainMenu()
    {
        // Восстанавливаем нормальную скорость времени перед загрузкой сцены
        Time.timeScale = 1f;

        // Загружаем главное меню
        SceneManager.LoadScene("Menu");
    }

    // Автоматически восстанавливаем время при уничтожении объекта
    void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    // Для отладки в редакторе
    [ContextMenu("Show Menu")]
    void ShowMenuEditor()
    {
        if (Application.isPlaying)
        {
            if (!isMenuActive) ToggleMenu();
        }
    }

    [ContextMenu("Hide Menu")]
    void HideMenuEditor()
    {
        if (Application.isPlaying)
        {
            if (isMenuActive) ToggleMenu();
        }
    }
}