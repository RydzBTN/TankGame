using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FindGamePanelManager : MonoBehaviourPunCallbacks
{
    [Header("Room list UI elements")]
    public Button createRoomButton;
    public Button closeButton;
    
    [Header("CreateRoom UI elements")]
    public GameObject createRoomPanel;
    public Button applyRoomCreation;
    public Button createRoomCloseButton;

    [Header("Room Setings Inputs")]
    public TMP_InputField roomNameInputField;
    public TMP_Dropdown maxPlayersDropdown;
    public TMP_Dropdown mapDropdown;
    public TMP_Dropdown gameModeDropdown;
    public TMP_Dropdown weatherDropdown;
    public TMP_InputField gameTimeInputField;
    public Toggle isPublic;





    private void Start()
    {
        applyRoomCreation.onClick.AddListener(() =>
        {
            OnCreateRoomButtonClicked(roomNameInputField.text);
        });

        createRoomCloseButton.onClick.AddListener(OnCreateRoomCloseButtonClick);
        createRoomButton.onClick.AddListener(OnCreateRoomClick);
    }

    public void OnCreateRoomClick()
    {
        createRoomPanel.SetActive(true);
    }

    public void OnCreateRoomCloseButtonClick()
    {
        createRoomPanel.SetActive(false);
    }

    // Call this when the Create Room button is clicked
    public void OnCreateRoomButtonClicked(string roomName)
    {

        if (string.IsNullOrEmpty(roomName))
        {
            roomName = "Room " + Random.Range(1000, 10000);
        }

        // Create room options - customize as needed
        RoomOptions options = new RoomOptions
        {
            MaxPlayers = maxPlayersDropdown.value,
            IsVisible = isPublic.isOn,
            IsOpen = true,

            // Czas ¿ycia pokoju
            EmptyRoomTtl = 60000, // 1 minuta gdy pusty
            PlayerTtl = 60000,    // 1 minuta na reconnect

            // ustawienia niestandardowe
            CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
            {
                //mapa
                {"mapName", mapDropdown.value},
                {"gameMode", gameModeDropdown.value},
                {"weather", weatherDropdown.value},

                //rozgrywka
                {"matchDuration", gameTimeInputField.text}
                
            }
        };

        // Create the room
        PhotonNetwork.CreateRoom(roomName, options);
    }
}
