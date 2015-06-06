using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(NetworkView))]
public class CellPlayerNonAuthoritive : MonoBehaviour {
    public float size = 10f;
    float velocity = 5f;

    NetworkView theNetworkView;
    Rigidbody2D theRigidbody2D;

    void Awake() {
        theNetworkView = GetComponent<NetworkView>();
        theRigidbody2D = GetComponent<Rigidbody2D>();
    }

    void Update() {
        float scale = 2 * Mathf.Sqrt(size / Mathf.PI);
        transform.localScale = new Vector3(scale, scale);
        velocity = 5f / Mathf.Log(size / 2f, 5f);

        Debug.Log(velocity);
    }

	void FixedUpdate () {
        size -= 0.001f * size * Time.fixedDeltaTime; // Reduce size over time
        if (size < 10f)
            size = 10f;

        if (theNetworkView.isMine) {
            InputMovement();
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
}
