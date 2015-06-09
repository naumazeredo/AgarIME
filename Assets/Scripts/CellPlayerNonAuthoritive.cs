using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(NetworkView))]
[RequireComponent(typeof(Size))]
public class CellPlayerNonAuthoritive : MonoBehaviour {
    Size theSize;
    float velocity = 5f;

    NetworkView theNetworkView;
    Rigidbody2D theRigidbody2D;
    Collider2D theCollider2D;

    // Networking synchronization
    //bool networkInitialized = false;
    //Vector3 syncPosition;

    void Awake() {
        theNetworkView = GetComponent<NetworkView>();
        theRigidbody2D = GetComponent<Rigidbody2D>();
        theCollider2D = GetComponent<Collider2D>();
        theSize = GetComponent<Size>();
    }

    void Start() {
        if (theNetworkView.isMine) {
            Camera.main.GetComponent<CameraMovement>().follow = this.transform;
        }

        theSize.OnGetEaten = OnGetEaten;
    }

    void OnEnable() {
        float x = NetworkManager.arenaSize.x;
        float y = NetworkManager.arenaSize.y;

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
        float scale = 2 * Mathf.Sqrt(theSize.GetSize() / Mathf.PI);
        transform.localScale = new Vector3(scale, scale);
        velocity = 5f / Mathf.Log(theSize.GetSize() / 2f, 5f);

        Vector3 position = transform.position;
        position.z = -0.001f * scale;

        // Clamp inside arena
        Vector2 arenaHalfSize = new Vector2(
            NetworkManager.arenaSize.x / 2 - theCollider2D.bounds.extents.x,
            NetworkManager.arenaSize.y / 2 - theCollider2D.bounds.extents.y);
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

	void FixedUpdate () {
        theSize.Decay();

        if (theNetworkView.isMine) {
            InputMovement();
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
        
    }

    void OnTriggerStay2D(Collider2D col) {
        if (theNetworkView.isMine) {
            Size colSizeComponent = col.gameObject.GetComponent<Size>();
            float deltaSize = theSize.GetSize() - colSizeComponent.GetSize();
            if (deltaSize > 0f) {
                float proportionalDeltaSize = deltaSize / theSize.GetSize();
                if (proportionalDeltaSize > 0.05f) {
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
        float syncSize = 0f;
        if (stream.isWriting) {
            syncPosition = theRigidbody2D.position;
            syncVelocity = theRigidbody2D.velocity;
            syncSize = theSize.GetSize();

            stream.Serialize(ref syncPosition);
            stream.Serialize(ref syncVelocity);
            stream.Serialize(ref syncSize);
        } else {
            stream.Serialize(ref syncPosition);
            stream.Serialize(ref syncVelocity);
            stream.Serialize(ref syncSize);

            theRigidbody2D.position = syncPosition;
            theSize.size = syncSize;
        }
    }

    void OnGetEaten() {
        Network.Destroy(this.gameObject);
    }
}
