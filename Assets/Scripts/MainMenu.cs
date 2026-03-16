using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private bool _isLoading;

    private void Update()
    {
        if (_isLoading || Keyboard.current == null)
        {
            return;
        }

        if (!Keyboard.current.enterKey.wasPressedThisFrame && !Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            return;
        }

        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.name != "Play")
        {
            return;
        }

        PlayGame();
    }

    public void PlayGame()
    {
        if (_isLoading)
        {
            return;
        }

        _isLoading = true;
        OverworldStoryState.ResetProgress();
        return_to_game loader = FindObjectOfType<return_to_game>();

        if (loader != null)
        {
            loader.load_level("OverworldScene");
            return;
        }

        SceneManager.LoadSceneAsync("OverworldScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
