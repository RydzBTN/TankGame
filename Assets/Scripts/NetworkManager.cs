using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject roomListContent;
    [SerializeField] private GameObject roomListItemPrefab;
    public Button refreshRoomListButton;

    private Dictionary<string, GameObject> roomListItems = new Dictionary<string, GameObject>();

    private void Start()
    {
        // Connect to Photon using the settings from PhotonServerSettings asset
        PhotonNetwork.ConnectUsingSettings();

        //if (createRoom != null) createRoom.interactable = false;
        //if (refreshRoomListButton != null) refreshRoomListButton.interactable = false;

        refreshRoomListButton.onClick.AddListener(RefreshRoomList);
    }

    public void RefreshRoomList()
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
    }

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
    }

    // Called if connecting to Photon fails
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("Disconnected from Photon: " + cause.ToString());
    }

    // Call this method when a Join Room button is clicked
    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
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

   

}
