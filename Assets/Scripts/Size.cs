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
    public event GetEatenDelegate OnGetEaten;

    public delegate void EatDelegate();
    public event EatDelegate OnEat;

    public void Decay() {
        size -= 0.001f * size * Time.deltaTime;
        if (size < minimumCellSize)
            size = minimumCellSize;
    }

    public void Eat(Size eaten) {
        Grow(eaten.size);
        if (OnEat != null)
            OnEat();
        if (eaten.OnGetEaten != null)
            eaten.OnGetEaten();
    }

    public void Grow(float grow) {
        size += grow;
    }
}
