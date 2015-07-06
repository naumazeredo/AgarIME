using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Size))]
public class CellSize : NetworkBehaviour {
    Size theSize;
    Collider2D theCollider2D;

    // Animation
    float lastSize = 0f;
    float size = 0f;
    float growTime = 0f;
    static float growTimespan = 0.3f;

    public override void OnStartClient() {
        base.OnStartClient();

        growTime = growTimespan;
        lastSize = theSize.GetSize();
    }

    void Awake() {
        theSize = GetComponent<Size>();
        theCollider2D = GetComponent<Collider2D>();

        theSize.OnGetEaten = OnGetEaten;
        theSize.OnEat = RpcOnEat;
    }
	
	void Update () {
        theSize.Decay();

        // Animation
        if (theSize.GetSize() > lastSize)
            growTime = 0f;
        lastSize = theSize.GetSize();

        UpdateScale();
        UpdateZ();
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
    }

    void OnGetEaten() {
        NetworkServer.Destroy(this.gameObject);
    }

    [ClientRpc]
    void RpcOnEat() {
        growTime = 0f;
    }

    void OnTriggerStay2D(Collider2D col) {
        // Only allow eating on server
        if (isServer) {
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
}
