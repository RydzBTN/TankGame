using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelManager : MonoBehaviour
{
    [Header("Settings Type Buttons")]
    public Button graphicsSettingsButton;
    public Button displaySettingsButton;
    public Button controlsSettingsButton;
    public Button audioSettingsButton;

    [Header("Settings Type Panels")]
    public GameObject graphicsSettingsPanel;
    public GameObject displaySettingsPanel;
    public GameObject controlsSettingsPanel;
    public GameObject audioSettingsPanel;

    [Header("Graphics Setings Inputs")]

    [Header("Display Setings Inputs")]

    [Header("Controls Setings Inputs")]

    [Header("Audio Setings Inputs")]

    [Header("Bottom Buttons")]
    public Button cancelButton;
    public Button saveButton;


    private void Start()
    {
        graphicsSettingsPanel.SetActive(true);
        displaySettingsPanel.SetActive(false);
        controlsSettingsPanel.SetActive(false);
        audioSettingsPanel.SetActive(false);

        graphicsSettingsButton.onClick.AddListener(ShowGraphicsSettingsPanel);
        displaySettingsButton.onClick.AddListener(ShowDisplaySettingsPanel);
        controlsSettingsButton.onClick.AddListener(ShowControlsSettingsPanel);
        audioSettingsButton.onClick.AddListener(ShowAudioSettingsPanel);
    }

    public void SaveSettingsChanges()
    {
        // Logic to save the changes made in the settings panels
        Debug.Log("Settings changes saved.");
    }

    public void CancelSettingsChanges()
    {
        // Logic to revert any changes made in the settings panels
    }

    public void ShowGraphicsSettingsPanel()
    {
        displaySettingsPanel.SetActive(false);
        controlsSettingsPanel.SetActive(false);
        audioSettingsPanel.SetActive(false);
        ////////////////////////////////////
        graphicsSettingsPanel.SetActive(true);
    }

    public void ShowControlsSettingsPanel()
    {
        audioSettingsPanel.SetActive(false);
        displaySettingsPanel.SetActive(false);
        graphicsSettingsPanel.SetActive(false);
        /////////////////////////////////////
        controlsSettingsPanel.SetActive(true);
    }

    public void ShowDisplaySettingsPanel()
    {
        audioSettingsPanel.SetActive(false);
        controlsSettingsPanel.SetActive(false);
        graphicsSettingsPanel.SetActive(false);
        /////////////////////////////////////
        displaySettingsPanel.SetActive(true);
    }

    public void ShowAudioSettingsPanel()
    {
        controlsSettingsPanel.SetActive(false);
        displaySettingsPanel.SetActive(false);
        graphicsSettingsPanel.SetActive(false);
        /////////////////////////////////////
        audioSettingsPanel.SetActive(true);
    }



}
