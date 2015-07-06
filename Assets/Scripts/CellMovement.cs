using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Size))]
[RequireComponent(typeof(CellSize))]
public class CellMovement : NetworkBehaviour {
    float velocity = 5f;

    [SyncVar]
    public string cellName;

    [SyncVar]
    public Color color;

    Collider2D theCollider2D;
    Rigidbody2D theRigidbody2D;
    Size theSize;

    Transform playerContainer;

    void Awake() {
        theCollider2D = GetComponent<Collider2D>();
        theRigidbody2D = GetComponent<Rigidbody2D>();
        theSize = GetComponent<Size>();

        playerContainer = GameObject.Find("Player Container").transform;

        transform.SetParent(playerContainer);
    }

    void Start() {
        SetName();
        GetComponent<SpriteRenderer>().color = color;
    }

    public override void OnStartLocalPlayer() {
        base.OnStartLocalPlayer();
        Debug.Log("OnStartLocalPlayer");

        Camera.main.GetComponent<CameraMovement>().follow = this.transform;

        Networking.instance.player = this.gameObject;
    }


    void Update() {
        velocity = 8f / Mathf.Log(theSize.GetSize() / 2f, 5f);
    }

    void FixedUpdate() {
        if (isLocalPlayer) {
            if (GameManager.State == GameManager.GameStates.Playing) {
                InputMovement();
                //ClampPositionToArena();
            }
        }
    }

    void InputMovement() {
        Vector2 dir = Vector2.zero;
        if (Input.GetKey(KeyCode.W))
            dir += Vector2.up;
        if (Input.GetKey(KeyCode.S))
            dir -= Vector2.up;
        if (Input.GetKey(KeyCode.D))
            dir += Vector2.right;
        if (Input.GetKey(KeyCode.A))
            dir -= Vector2.right;

        theRigidbody2D.AddForce(dir.normalized * velocity);
    }

    void ClampPositionToArena() {
        Vector3 position = theRigidbody2D.position;
        Vector2 arenaHalfSize = new Vector2(
                Networking.instance.arenaSize.x / 2 - theCollider2D.bounds.extents.x,
                Networking.instance.arenaSize.y / 2 - theCollider2D.bounds.extents.y);
        Vector2 vel = theRigidbody2D.velocity;
        if (position.x < -arenaHalfSize.x) {
            position.x = -arenaHalfSize.x;
            vel.x = 0f;
        } else if (position.x > arenaHalfSize.x) {
            position.x = arenaHalfSize.x;
            vel.x = 0f;
        }

        if (position.y < -arenaHalfSize.y) {
            position.y = -arenaHalfSize.y;
            vel.y = 0f;
        } else if (position.y > arenaHalfSize.y) {
            position.y = arenaHalfSize.y;
            vel.y = 0f;
        }

        theRigidbody2D.position = position;
        theRigidbody2D.velocity = vel;
    }

    public void SetName() {
        gameObject.name = cellName;

        /*
        Sprite customSkin = CustomSkinManager.GetCustomSkin(name);
        if (customSkin != null) {
            SetColor(Color.white);
            GetComponent<SpriteRenderer>().sprite = customSkin;
        } else {
            GetComponentInChildren<Text>().text = name;
        }
        */
    }

    public void Recolor() {
        Vector2 arenaSize = Networking.instance.arenaSize;
        float r = (transform.position.x + arenaSize.x) / (2 * arenaSize.x);
        float g = (transform.position.y + arenaSize.y) / (2 * arenaSize.y);
        float b = Random.Range(0f, 1f);
        color = new Color(r, g, b);
    }
}
