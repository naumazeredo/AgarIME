using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class Size : MonoBehaviour {
    [SerializeField]
    float size = 1f;

    public float GetSize() {
        return size;
    }

    public delegate void GetEatenDelegate();
    public GetEatenDelegate OnGetEaten;

    public delegate void EatDelegate();
    public EatDelegate OnEat;

    public void Decay() {
        size -= 0.001f * size * Time.deltaTime;
        if (size < 10f)
            size = 10f;
    }

    public void Eat(Size eaten) {
        size += eaten.size;
        if (OnEat != null)
            OnEat();
        if (eaten.OnGetEaten != null)
            eaten.OnGetEaten();
    }

    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
        float syncSize = 0f;
        if (stream.isWriting) {
            syncSize = size;
            stream.Serialize(ref syncSize);
        } else {
            stream.Serialize(ref syncSize);
            size = syncSize;
        }
    }
}
