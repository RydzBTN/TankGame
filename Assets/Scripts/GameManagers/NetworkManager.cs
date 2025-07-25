using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance { get; private set; }

    [Header("RoomList UI Elements")]
    [SerializeField] private GameObject roomListContent;
    [SerializeField] private GameObject roomListItemPrefab;
    public Button refreshRoomListButton;

    [Header("Room UI Elements")]
    [SerializeField] private TextMeshProUGUI roomNameText;
    [SerializeField] private GridLayoutGroup teamAGrid;
    [SerializeField] private GridLayoutGroup teamBGrid;
    [SerializeField] private GameObject playerNamePrefab;

    private Dictionary<string, GameObject> roomListItems = new Dictionary<string, GameObject>();
    private Dictionary<int, GameObject> playerListDictionary = new Dictionary<int, GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Connect to Photon using the settings from PhotonServerSettings asset
        PhotonNetwork.ConnectUsingSettings();
        refreshRoomListButton.onClick.AddListener(RefreshRoomList);
        

        ChangePlayerNickname("Player_" + Random.Range(1000, 9999).ToString());
    }

    #region Display Room List Methods

    private void RefreshRoomList()
    {
        Debug.Log("Refreshing room list...");

        if (!PhotonNetwork.IsConnected)
        {
            Debug.LogWarning("Cannot refresh room list. Not connected to Photon.");
            return;
        }

        // Opuszczamy lobby i do³¹czamy ponownie, aby otrzymaæ zaktualizowan¹ listê pokojów
        PhotonNetwork.LeaveLobby();
        PhotonNetwork.JoinLobby();

        Debug.Log("Room list refreshed");
    }

    private void UpdateRoomList(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            // Remove room from cached room list if it got closed, became invisible or was marked as removed
            if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
            {
                if (roomListItems.ContainsKey(info.Name))
                {
                    Destroy(roomListItems[info.Name]);
                    roomListItems.Remove(info.Name);
                }
                continue;
            }

            // Update cached room info
            if (roomListItems.ContainsKey(info.Name))
            {
                // Update existing room item
                GameObject roomItem = roomListItems[info.Name];
                roomItem.GetComponent<RoomListItem>().Initialize(info.Name, (byte)info.PlayerCount, (byte)info.MaxPlayers);
            }
            else
            {
                // Create new room item
                GameObject newRoomItem = Instantiate(roomListItemPrefab);
                newRoomItem.transform.SetParent(roomListContent.transform);
                newRoomItem.transform.localScale = Vector3.one;

                // Initialize the room item
                newRoomItem.GetComponent<RoomListItem>().Initialize(info.Name, (byte)info.PlayerCount, (byte)info.MaxPlayers);

                // Store reference to the room item
                roomListItems.Add(info.Name, newRoomItem);
            }
        }
    }

    

    private void ClearRoomList()
    {
        foreach (GameObject entry in roomListItems.Values)
        {
            Destroy(entry);
        }

        roomListItems.Clear();
    }

    #endregion

    #region Display Players In Room Methods
    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public void CreatePlayerContainer(Player player)
    {
        if( playerListDictionary.ContainsKey(player.ActorNumber))
        {
            Debug.LogWarning($"Player {player.NickName} already exists in the Room");
            return;
        }
        GameObject playerContainer = Instantiate(playerNamePrefab);
        playerContainer.transform.SetParent(teamAGrid.transform);
        playerContainer.transform.localScale = Vector3.one;
        PlayerName playerNameComponent = playerContainer.GetComponent<PlayerName>();
        if (playerNameComponent != null)
        {
            playerNameComponent.SetName(player.NickName);
            playerListDictionary.Add(player.ActorNumber, playerContainer);
        }
    }

    #endregion

    #region Photon Callbacks

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");
        ClearRoomList();
    }

    // Called when the room list updates
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdateRoomList(roomList);
    }

    // Called when we successfully join a room
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room: " + PhotonNetwork.CurrentRoom.Name);
        CreatePlayerContainer(PhotonNetwork.LocalPlayer);

    }

    // Called if connecting to Photon fails
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("Disconnected from Photon: " + cause.ToString());
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Left Room");

        // Clear the player list when leaving the room
        foreach (GameObject playerContainer in playerListDictionary.Values)
        {
            Destroy(playerContainer);
        }
        playerListDictionary.Clear();

        // If we were the master client (room creator), destroy the room
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            Debug.Log("Destroying room as master client");
            
        }
    }
   

    #endregion

    public void ChangePlayerNickname(string newNickname)
    {
        if(PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LocalPlayer.NickName = newNickname;
            Debug.Log("Nickname changed to: " + newNickname);
        }
        else
        {
            Debug.LogWarning("Cannot change nickname.");
        }
    }
    

    



}
