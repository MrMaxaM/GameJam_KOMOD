using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    public Button playButton;
    public Button exitButton;

    void Start()
    {
        // Назначаем методы на кнопки
        playButton.onClick.AddListener(PlayGame);
        exitButton.onClick.AddListener(ExitGame);
        Fade.Instance.FadeFromBlack();
    }

    public void PlayGame()
    {
        // Загружаем игровую сцену (измените на имя вашей сцены)
        SceneManager.LoadScene("Comics1");
    }

    public void ExitGame()
    {
        // Выход из игры
        Debug.Log("Выход из игры...");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}