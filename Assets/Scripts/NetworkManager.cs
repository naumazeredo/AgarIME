using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour {
    public string ipAddress = "127.0.0.1";

    private const string gameTypeName = "AgarIMEio";
    private const string gameName = "RoomIME";
    private HostData[] hostList;
    private bool connecting = false;
    private bool connected = false;

    // Game objects
    public GameObject food;
    public GameObject cell;

    private GameObject player = null;

    void Start() {
        // TODO(naum): Don't use this!
        ChangeServerIpAddress("127.0.0.1");
    }

    public void ChangeServerIpAddress(string ip) {
        ipAddress = ip;
        MasterServer.ipAddress = ipAddress;
    }

    private void StartServer() {
        Network.InitializeServer(32, 25005, !Network.HavePublicAddress());
        MasterServer.RegisterHost(gameTypeName, gameName);
    }

    void OnMasterServerEvent(MasterServerEvent msEvent) {
        if (msEvent == MasterServerEvent.HostListReceived) {
            hostList = MasterServer.PollHostList();
        }
    }

    private void JoinServer() {
        hostList = null;
        MasterServer.RequestHostList(gameTypeName);
        connecting = true;
        connected = false;
    }

    void Update() {
        if (connecting && hostList != null) {
            Network.Connect(hostList[0]);
            connecting = false;
        }

        if (connected && Input.GetKeyDown(KeyCode.Space) && player == null) {
            // TODO(naum): Object pool cells
            // TODO(naum): Calculate best position to instantiate
            player = Network.Instantiate(cell, Vector3.zero, Quaternion.identity, 0) as GameObject;
        }
    }

    void OnServerInitialized() {
        //Network.Instantiate(food, Vector3.zero, Quaternion.identity, 0);
        connected = true;
    }

    void OnConnectedToServer() {
        connected = true;
    }

    void OnDisconnectedFromServer(NetworkDisconnection info) {
        connected = false;
        if (Network.isServer) {
            Debug.Log("Local server connection disconnected");
        } else {
            if (info == NetworkDisconnection.LostConnection)
                Debug.Log("Lost connection to the server");
            else
                Debug.Log("Successfully diconnected from the server");
        }

        // TODO(naum): deactivate all objects in scene
    }

    void OnPlayerConnected(NetworkPlayer netplayer) {
        Debug.Log("Player connected: " + netplayer.ipAddress);
    }

    void OnPlayerDisconnected(NetworkPlayer netplayer) {
        // TODO(naum): Change cell to a "cell fragment" (food)
        Debug.Log("Player disconnected: " + netplayer.ipAddress);
        Network.RemoveRPCs(netplayer);
        Network.DestroyPlayerObjects(netplayer);
    }

    void OnGUI() {
        if (!Network.isClient && !Network.isServer) {
            if (GUI.Button(new Rect(10, 10, 100, 50), "Start Server"))
                StartServer();

            if (GUI.Button(new Rect(10, 70, 100, 50), "Connect"))
                JoinServer();
        }
    }
}
