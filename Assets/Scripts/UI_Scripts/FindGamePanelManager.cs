using Photon.Pun;
using Photon.Pun.Demo.Cockpit;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class FindGamePanelManager : MonoBehaviourPunCallbacks
{
    [Header("Panels")]
    [SerializeField] GameObject searchRoomPanel;
    [SerializeField] GameObject createRoomPanel;
    [SerializeField] GameObject insideRoomPanel;

    [Header("SearchRoomPanel UI elements")]
    public Button createRoomButton;
    public Button closeButton;

    [Header("insideRoomPanel UI elements")]
    public Button leaveRoomButton;
    public Button startGameButton;

    [Header("CreateRoomPanel UI elements")]
    public Button applyRoomCreation;
    public Button createRoomCloseButton;

    [Header("CreateRoomPanel Input Fields")]
    public TMP_InputField roomNameInputField;
    public TMP_Dropdown maxPlayersDropdown;
    public TMP_Dropdown mapDropdown;
    public TMP_Dropdown gameModeDropdown;
    public TMP_Dropdown weatherDropdown;
    public TMP_InputField gameTimeInputField;
    public Toggle isPublic;

    private void Start()
    {
        searchRoomPanel.SetActive(true);
        createRoomPanel.SetActive(false);
        insideRoomPanel.SetActive(false);


        applyRoomCreation.onClick.AddListener(OnCreateRoomButtonClicked);
        createRoomCloseButton.onClick.AddListener(OnCreateRoomCloseButtonClick);
        createRoomButton.onClick.AddListener(OnCreateRoomClick);
        leaveRoomButton.onClick.AddListener(LeaveRoom);
        startGameButton.onClick.AddListener(StartGame);
    }
    public override void OnCreatedRoom()
    {

        insideRoomPanel.SetActive(true);
        searchRoomPanel.SetActive(false);
        createRoomPanel.SetActive(false);

        UpdateStartGameButton();
    }
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        UpdateStartGameButton();

        Debug.Log((string)PhotonNetwork.CurrentRoom.CustomProperties["map"]);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        UpdateStartGameButton();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        UpdateStartGameButton();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);
        UpdateStartGameButton();
    }
    public void OnCreateRoomClick()
    {
        createRoomPanel.SetActive(true);
    }

    public void OnCreateRoomCloseButtonClick()
    {
        createRoomPanel.SetActive(false);
    }
    private void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("Only the Master Client can start the game!");
            return;
        }

        // Set room as closed so no new players can join
        PhotonNetwork.CurrentRoom.IsOpen = false;

        // Send RPC to all players to start the game
        photonView.RPC("StartGameRPC", RpcTarget.All);
    }
    [PunRPC]
    private void StartGameRPC()
    {
        Debug.Log("Game is starting!");

        // Load the game scene
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel($"{(string)PhotonNetwork.CurrentRoom.CustomProperties["map"]}");
        }
    }
    // Call this when the Create Room button is clicked
    public void OnCreateRoomButtonClicked()
    {
        string roomName = roomNameInputField.text;

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
            EmptyRoomTtl = 1000, // 1 sekunda gdy pusty
            PlayerTtl = 60000,    // 1 minuta na reconnect

            // ustawienia niestandardowe
            CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
            {
                //mapa
                {"map", mapDropdown.options[mapDropdown.value].text},
                {"mode", gameModeDropdown.value},
                {"weather", weatherDropdown.value},

                //rozgrywka
                {"duration", gameTimeInputField.text}
                
            }
        };

        // Create the room
        PhotonNetwork.CreateRoom(roomName, options);
    }
    private void UpdateStartGameButton()
    {
        if (startGameButton == null) return;

        // Only show start button to Master Client
        startGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);

        // Enable button only if there are enough players (minimum 2 for example)
        startGameButton.interactable = PhotonNetwork.CurrentRoom.PlayerCount >= 1;
    }
      

    private void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        insideRoomPanel.SetActive(false);
        searchRoomPanel.SetActive(true);
    }

}
