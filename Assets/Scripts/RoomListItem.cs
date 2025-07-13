using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class RoomListItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roomNameText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private Button joinButton;

    private string roomName;

    private void Start()
    {
        // Add listener to the join button
        joinButton.onClick.AddListener(() => {
            NetworkManager networkManager = FindAnyObjectByType<NetworkManager>();
            if (networkManager != null)
            {
                networkManager.JoinRoom(roomName);
            }
        });
    }

    // Initialize the room list item with room info
    public void Initialize(string name, byte currentPlayers, byte maxPlayers)
    {
        roomName = name;
        roomNameText.text = name;
        playerCountText.text = currentPlayers + " / " + maxPlayers;
    }
}
