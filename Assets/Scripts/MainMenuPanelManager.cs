using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject settingsPanel;
    public GameObject findGamePanel;

    [Header("UI MainButtons")]
    public Button findGameButton;
    public Button settingsButton;
    public Button quitGameButton;

    private void Start()
    {
        settingsPanel.SetActive(false);
        findGamePanel.SetActive(false);

        findGameButton.onClick.AddListener(ShowFindGamePanel);
        settingsButton.onClick.AddListener(ShowSettingsPanel);
    }

    void ShowSettingsPanel()
    {
        findGamePanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    void ShowFindGamePanel()
    {
      settingsPanel.SetActive(false);
      findGamePanel.SetActive(true);
    }

    

}
