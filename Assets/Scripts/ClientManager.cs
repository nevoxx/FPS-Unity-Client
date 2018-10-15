using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using FPS_Bindings;

public class ClientManager : MonoBehaviour
{
    public static ClientManager instance;
    public int myConnectionId;
    
    [SerializeField] private GameObject connectionPrefab;
    [SerializeField] private string ipAddress;
    [SerializeField] private int port;
    
    private void Awake()
    {
        instance = this;
        
        DontDestroyOnLoad(this);
        
        UnityThread.initUnityThread();
        
        InitPlayers();
        
        ClientHandleData.InitPackets();
        ClientTCP.InitClient(ipAddress, port);
    }

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    void InitPlayers()
    {
        for (int i = 0; i < Constants.MAX_PLAYERS; i++)
        {
            Types.players[i] = new Types.PlayerRect();
        }
    }

    public void InstantiateNetworkPlayer(int connectionId)
    {
        Types.players[connectionId].playerPref = Instantiate(connectionPrefab);
        Types.players[connectionId].playerPref.name = "Player_" + connectionId;

        Types.players[connectionId].playerPref.GetComponent<NetworkPlayerController>().PlayerConnectionId = connectionId;
        
        if (connectionId != myConnectionId)
        {
            Types.players[connectionId].playerPref.GetComponent<NetworkPlayerController>().enabled = false;
            Types.players[connectionId].playerPref.GetComponentInChildren<Camera>().enabled = false;
            Types.players[connectionId].playerPref.GetComponentInChildren<AudioListener>().enabled = false;
        }
    }

    private void OnApplicationQuit()
    {
        ClientTCP.DisconnectFromServer();
    }
}