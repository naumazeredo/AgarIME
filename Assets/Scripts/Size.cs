using UnityEngine;
using UnityEngine.Networking;

public class Size : NetworkBehaviour {
    [SerializeField]
    [SyncVar]
    float size = 1f;

    public static float minimumCellSize = 10f;

    public float GetSize() {
        return size;
    }

    public delegate void GetEatenDelegate();
    public GetEatenDelegate OnGetEaten;

    public delegate void EatDelegate();
    public EatDelegate OnEat;

    public void Decay() {
        size -= 0.001f * size * Time.deltaTime;
        if (size < minimumCellSize)
            size = minimumCellSize;
    }

    public void Eat(Size eaten) {
        RpcGrow(eaten.size);

        OnEat();
        eaten.OnGetEaten();
    }

    [ClientRpc]
    public void RpcGrow(float grow) {
        size += grow;
    }
}
