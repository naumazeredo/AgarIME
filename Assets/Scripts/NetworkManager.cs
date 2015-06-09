using UnityEngine;
using UnityEngine.UI;
using System.Net;

public class NetworkManager : MonoBehaviour {
    static public NetworkManager instance;

    //public string ipAddress = "127.0.0.1";

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
    public Vector2 arenaSize = new Vector2(80f, 80f);

    // Food managing
    public int foodMaximum = 400;
    public int foodCount = 0;
    private float timer = 0f;

    // UI
    public Canvas loginCanvas;
    public InputField playerName;
    public Text errorMessage;
    public InputField ipAddress;

    void Awake() {
        instance = this;
    }

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
        Network.InitializeServer(32, 25005, false);
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
        } else {
            HideLoginUI();
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

    /*
    void OnGUI() {
        if (!Network.isClient && !Network.isServer) {
            if (GUI.Button(new Rect(10, 10, 100, 50), "Start Server"))
                StartServer();

            if (GUI.Button(new Rect(10, 70, 100, 50), "Connect"))
                JoinServer();
        }
    }
    */

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
}
