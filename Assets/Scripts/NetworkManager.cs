using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour {
    public string ipAddress = "127.0.0.1";

    private const string gameTypeName = "AgarIMEio";
    private const string gameName = "RoomIME";
    private HostData[] hostList;
    private bool connecting = false;

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
    }

    void Update() {
        if (connecting && hostList != null) {
            Network.Connect(hostList[0]);
            connecting = false;
            Debug.Log("Connected!");
        }
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
