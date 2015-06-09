using UnityEngine;

public class NetworkManager : MonoBehaviour {
    static public NetworkManager instance;

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

    // Arena
    public static Vector2 arenaSize = new Vector2(40f, 40f);

    // Food managing
    public int foodMaximum = 100;
    public int foodCount = 0;
    private float timer = 0f;

    void Awake() {
        instance = this;
    }

    void Start() {
        // TODO(naum): Don't use this!
        ChangeServerIpAddress("127.0.0.1");

        // TODO(naum): Server build (-nographics, -batchmode, -executeMethod NetworkManager.StartServer)
    }

    public void ChangeServerIpAddress(string ip) {
        ipAddress = ip;
        MasterServer.ipAddress = ipAddress;
    }

    private void StartServer() {
        Network.InitializeServer(32, 25005, !Network.HavePublicAddress());
        //MasterServer.dedicatedServer = true;
        MasterServer.RegisterHost(gameTypeName, gameName);
        for (int i = 0; i < 100; ++i)
            CreateFood();
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
            player = Network.Instantiate(
                cell,
                //Vector3.zero,
                new Vector3(
                    Random.Range(-arenaSize.x / 2, arenaSize.x / 2),
                    Random.Range(-arenaSize.y / 2, arenaSize.y / 2),
                    Random.value),
                Quaternion.identity, 0) as GameObject;
        }

        if (connected && Network.isServer) {
            timer += Time.deltaTime;
            if (timer >= 10f) {
                for (int i = foodCount; i < foodMaximum; ++i)
                    CreateFood();
                timer -= 10f;
            }
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

    void CreateFood() {
        Network.Instantiate(
            food,
            new Vector3(
                Random.Range(-arenaSize.x / 2, arenaSize.x / 2),
                Random.Range(-arenaSize.y / 2, arenaSize.y / 2),
                Random.value),
            Quaternion.identity, 0);
        foodCount++;
    }
}
