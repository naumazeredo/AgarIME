using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(NetworkView))]
[RequireComponent(typeof(Size))]
public class CellPlayerNonAuthoritative : MonoBehaviour {
    Size theSize;
    float velocity = 5f;

    NetworkView theNetworkView;
    Rigidbody2D theRigidbody2D;
    Collider2D theCollider2D;

    // Networking
    bool syncInit = false;
    Vector3 syncStartPosition = Vector3.zero;
    Vector3 syncEndPosition = Vector3.zero;
    float syncTime = 0f;
    static float syncDelay = 0.1f;

    // Animation
    float lastSize = 0f;
    float size = 0f;
    float growTime = 0f;
    static float growTimespan = 0.3f;

    void Awake() {
        theNetworkView = GetComponent<NetworkView>();
        theRigidbody2D = GetComponent<Rigidbody2D>();
        theCollider2D = GetComponent<Collider2D>();
        theSize = GetComponent<Size>();

        if (theNetworkView.isMine) {
            Camera.main.GetComponent<CameraMovement>().follow = this.transform;
        }

        theSize.OnGetEaten = OnGetEaten;
        theSize.OnEat = OnEat;
    }

    void OnEnable() {
        float x = NetworkManager.instance.arenaSize.x;
        float y = NetworkManager.instance.arenaSize.y;

        float r = (transform.position.x + x) / (2 * x);
        float g = (transform.position.y + y) / (2 * y);
        float b = transform.position.z;
        SetColor(new Color(r, g, b));

        transform.position = (Vector2) transform.position;
    }

    void SetColor(Color color) {
        GetComponent<SpriteRenderer>().color = color;
    }

    void Update() {
        if (theNetworkView.isMine) {
            theSize.Decay();
        }

        if (theSize.GetSize() > lastSize)
            growTime = 0f;
        lastSize = theSize.GetSize();

        UpdateScale();
        UpdateZ();
    }

    void FixedUpdate() {
        if (theNetworkView.isMine) {
            InputMovement();
            ClampPositionToArena();
        } else {
            SyncMovement();
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

    void SyncMovement() {
        syncTime += Time.fixedDeltaTime;
        transform.position = Vector3.Lerp(
            syncStartPosition,
            syncEndPosition,
            syncTime / syncDelay);
    }

    void UpdateZ() {
        Vector3 position = transform.position;
        position.z = -0.001f * theSize.GetSize();
        transform.position = position;
    }

    void UpdateScale() {
        growTime = Mathf.Clamp(growTime + Time.fixedDeltaTime, 0f, growTimespan);
        size = Mathf.Lerp(size, theSize.GetSize(), growTime / growTimespan);

        float scale = 2 * Mathf.Sqrt(size / Mathf.PI);
        transform.localScale = new Vector3(scale, scale);
        velocity = 8f / Mathf.Log(theSize.GetSize() / 2f, 5f);
    }

    void ClampPositionToArena() {
        Vector3 position = transform.position;
        Vector2 arenaHalfSize = new Vector2(
                NetworkManager.instance.arenaSize.x / 2 - theCollider2D.bounds.extents.x,
                NetworkManager.instance.arenaSize.y / 2 - theCollider2D.bounds.extents.y);
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

        transform.position = position;
        theRigidbody2D.velocity = vel;
    }

    void OnTriggerStay2D(Collider2D col) {
        if (theNetworkView.isMine) {
            Size colSizeComponent = col.gameObject.GetComponent<Size>();
            float deltaSize = theSize.GetSize() - colSizeComponent.GetSize();
            if (deltaSize > 0f) {
                //float proportionalDeltaSize = deltaSize / theSize.GetSize();
                //if (proportionalDeltaSize > 0.05f) {
                {
                    float sqrDist = (col.transform.position - transform.position).sqrMagnitude;
                    float deltaRadius = theCollider2D.bounds.extents.x - col.bounds.extents.x;

                    // If the smaller is container in the greater
                    if (sqrDist <= deltaRadius * deltaRadius) {
                        theSize.Eat(colSizeComponent);
                    }
                }
            }
        }
    }

    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
        Vector3 syncPosition = Vector3.zero;
        Vector3 syncVelocity = Vector3.zero;
        if (stream.isWriting) {
            syncPosition = theRigidbody2D.position;
            syncVelocity = theRigidbody2D.velocity;

            stream.Serialize(ref syncPosition);
            stream.Serialize(ref syncVelocity);
        } else {
            stream.Serialize(ref syncPosition);
            stream.Serialize(ref syncVelocity);

            if (!syncInit) {
                syncInit = true;
                theRigidbody2D.position = syncPosition;
                theRigidbody2D.velocity = syncVelocity;
                syncStartPosition = syncPosition;
            } else {
                syncStartPosition = theRigidbody2D.position;
            }

            syncEndPosition = syncPosition;
            syncTime = 0f;
        }
    }

    void OnGetEaten() {
        Network.Destroy(this.gameObject);
    }

    void OnEat() {
        growTime = 0f;
    }

    public void SetName(string name) {
        GetComponentInChildren<Text>().text = name;
        theNetworkView.RPC("ChangeName", RPCMode.OthersBuffered, name);
    }

    [RPC]
    void ChangeName(string name) {
        GetComponentInChildren<Text>().text = name;
    }
}
