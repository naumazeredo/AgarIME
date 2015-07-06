using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Networking : NetworkManager {
    static public Networking instance;

    /*
    private const string gameTypeName = "AgarIMEio";
    private const string gameName = "RoomIME";
    private HostData[] hostList;
    */
    private bool connecting = false;
    private bool connected = false;

    // Game objects
    private Transform foodContainer;

    public GameObject food;
    //public GameObject cell;

    public GameObject player;

    // Arena
    public Vector2 arenaSize = new Vector2(80f, 80f);

    // Food managing
    public int foodMaximum = 400;
    public int foodCount = 0;
    private float foodTimer = 0f;
    public float foodRespawnTime = 5f;

    // UI
    public Canvas loginCanvas;
    public InputField playerName;
    public Text errorMessage;
    public InputField ipAddress;

    void Awake() {
        instance = this;

        foodContainer = GameObject.Find("Food Container").transform;
    }

    public override void OnServerConnect(NetworkConnection conn) {
        Debug.Log("OnServerConnect: " + conn.connectionId);
        base.OnServerConnect(conn);
    }

    public override void OnServerDisconnect(NetworkConnection conn) {
        print("OnServerDisconnect: " + conn.connectionId);
        base.OnServerDisconnect(conn);
    }

    public override void OnServerReady(NetworkConnection conn) {
        print("OnServerReady: " + conn.connectionId);
        base.OnServerReady(conn);
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId) {
        print("OnServerAddPlayer (" + conn.connectionId + "): " + playerControllerId);
        //base.OnServerAddPlayer(conn, playerControllerId);

        player = (GameObject) Instantiate(
            playerPrefab,
            new Vector3(
                Random.Range(-arenaSize.x / 2, arenaSize.x / 2),
                Random.Range(-arenaSize.y / 2, arenaSize.y / 2),
                Random.value),
            Quaternion.identity);
        player.GetComponent<CellMovement>().cellName = playerName.text;
        player.GetComponent<CellMovement>().Recolor();

        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
    }

    public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player) {
        print("OnServerRemovePlayer(" + conn.connectionId + "): " + player);
        base.OnServerRemovePlayer(conn, player);
    }

    public override void OnServerError(NetworkConnection conn, int errorCode) {
        print("OnServerError(" + conn.connectionId + "): " + errorCode);
        base.OnServerError(conn, errorCode);
    }

    public override void OnClientConnect(NetworkConnection conn) {
        print("OnClientConnect: " + conn.connectionId);
        base.OnClientConnect(conn);
        connected = true;
    }

    public override void OnClientDisconnect(NetworkConnection conn) {
        print("OnClientDisconnect: " + conn.connectionId);
        base.OnClientDisconnect(conn);
    }

    public override void OnStartClient(NetworkClient client) {
        print("OnStartClient: " + client);
        base.OnStartClient(client);
    }

    void Update() {
        if (connected && Input.GetKeyDown(KeyCode.Space) && player == null) {
            ClientScene.AddPlayer(0);
        }
    }

    /*
    public void SpawnPlayer(int connectionId) {
        player = (GameObject) Instantiate(
            playerPrefab,
            new Vector3(
                Random.Range(-arenaSize.x / 2, arenaSize.x / 2),
                Random.Range(-arenaSize.y / 2, arenaSize.y / 2),
                Random.value),
            Quaternion.identity);
        player.GetComponent<CellMovement>().SetName(playerName.text);
        player.transform.SetParent(playerContainer);

        foreach (var c in NetworkServer.connections) {
            Debug.Log(c.connectionId);
            if (c.connectionId == connectionId) {
                NetworkServer.AddPlayerForConnection(c, player, 0);
                return;
            }
        }

        foreach (var c in NetworkServer.localConnections) {
            Debug.Log(c.connectionId);
            if (c.connectionId == connectionId) {
                NetworkServer.AddPlayerForConnection(c, player, 0);
                return;
            }
        }
    }
    */

    /*
    void Start() {
        // TODO(naum): Don't use this!
        //ChangeServerIpAddress("127.0.0.1");

        // TODO(naum): Server build (-nographics, -batchmode, -executeMethod NetworkManager.StartServer)
    }

    //public void ChangeServerIpAddress(string ip) {
    public void ChangeServerIpAddress() {
        //ipAddress.text = ip;
        MasterServer.ipAddress = ipAddress.text;
    }

    public void StartServer() {
        if (!ValidatePlayerName())
            return;
        if (!ValidateIpAddress())
            return;
        //Network.InitializeServer(32, 25005, !Network.HavePublicAddress());
        Network.InitializeServer(32, 80, false);
        //MasterServer.dedicatedServer = true;
        MasterServer.RegisterHost(gameTypeName, gameName);
        for (int i = 0; i < foodMaximum; ++i)
            CreateFood();
    }

    void OnMasterServerEvent(MasterServerEvent msEvent) {
        if (msEvent == MasterServerEvent.HostListReceived) {
            hostList = MasterServer.PollHostList();
        }
    }

    public void JoinServer() {
        if (!ValidatePlayerName())
            return;
        if (!ValidateIpAddress())
            return;
        hostList = null;
        MasterServer.RequestHostList(gameTypeName);
        connecting = true;
        connected = false;
        Debug.Log("Joining Server!");
    }

    void Update() {
        if (!Network.isClient && !Network.isServer) {
            ShowLoginUI();
            GameManager.ChangeState(GameManager.GameStates.Login);
        } else {
            HideLoginUI();
            if (GameManager.State == GameManager.GameStates.Login) {
                GameManager.ChangeState(GameManager.GameStates.Playing);
            }
        }

        if (connecting && hostList != null && hostList.Length > 0) {
            Debug.Log("Connecting and received hostList");
            Network.Connect(hostList[0]);
            connecting = false;
        }

        if (connected && Input.GetKeyDown(KeyCode.Space) && player == null) {
            // TODO(naum): Object pool cells
            // TODO(naum): Calculate best position to instantiate
            SpawnPlayer();
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

    void CreateFood() {
        GameObject newFood = Network.Instantiate(
            food,
            new Vector3(
                Random.Range(-arenaSize.x / 2, arenaSize.x / 2),
                Random.Range(-arenaSize.y / 2, arenaSize.y / 2),
                Random.value),
            Quaternion.identity, 0) as GameObject;
        newFood.transform.SetParent(foodContainer);
        foodCount++;
    }

    void SpawnPlayer() {
        player = Network.Instantiate(
            cell,
            //Vector3.zero,
            new Vector3(
                Random.Range(-arenaSize.x / 2, arenaSize.x / 2),
                Random.Range(-arenaSize.y / 2, arenaSize.y / 2),
                Random.value),
            Quaternion.identity, 0) as GameObject;
        player.GetComponent<CellPlayerNonAuthoritative>().SetName(playerName.text);
        player.transform.SetParent(playerContainer);
    }

    public void ShowLoginUI() {
        loginCanvas.gameObject.SetActive(true);
    }

    public void HideLoginUI() {
        loginCanvas.gameObject.SetActive(false);
    }

    bool ValidatePlayerName() {
        // TODO(naum): Avoid name collisions
        if (playerName.text.Length == 0)
            errorMessage.text = "Player name should not be empty!";
        else
            HideErrorMessage();
        return playerName.text.Length > 0;
    }

    void HideErrorMessage() {
        errorMessage.text = "";
    }

    public bool ValidateIpAddress() {
        IPAddress ip;
        bool valid = IPAddress.TryParse(ipAddress.text, out ip);
        if (valid) {
            ChangeServerIpAddress();
        } else {
            errorMessage.text = "Incorrect ip!";
        }
        return valid;
    }
    */
}
