using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject playPanel;

    [Header("UI MainButtons")]
    [SerializeField] private Button findGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitGameButton;

    private void Start()
    {
        settingsPanel.SetActive(false);
        playPanel.SetActive(false);
        

        findGameButton.onClick.AddListener(ShowFindGamePanel);
        settingsButton.onClick.AddListener(ShowSettingsPanel);
    }

    void ShowSettingsPanel()
    {
        playPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    void ShowFindGamePanel()
    {
      settingsPanel.SetActive(false);
      playPanel.SetActive(true);
    }

    

}
