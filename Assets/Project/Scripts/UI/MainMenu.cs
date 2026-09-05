using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SlotGame.UI
{
    public class MainMenu : MonoBehaviour
    {
        [Header("Scene Configuration")]
        [Tooltip("The exact name of the scene to load. Prevents silent typo bugs by keeping it exposed.")]
        [SerializeField] private string gameSceneName = "SlotGame";

        [Header("UI References")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button quitButton;

        private void OnEnable()
        {
            if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
            if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);
        }

        private void OnDisable()
        {
            if (playButton != null) playButton.onClick.RemoveListener(OnPlayClicked);
            if (quitButton != null) quitButton.onClick.RemoveListener(OnQuitClicked);
        }

        private void OnPlayClicked()
        {
            // Lock the button instantly to prevent double-click loads
            playButton.interactable = false;
            SceneManager.LoadScene(gameSceneName);
        }

    
        private void OnQuitClicked()
        {
            quitButton.interactable = false;

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}