using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Size))]
public class Food : NetworkBehaviour {
    void Start() {
        GetComponent<Size>().OnGetEaten += CmdGetEaten;
    }

    void OnEnable() {
        float x = Networking.instance.arenaSize.x;
        float y = Networking.instance.arenaSize.y;
        float r = (transform.position.x + x) / (2 * x);
        float g = (transform.position.y + y) / (2 * y);
        float b = transform.position.z;
        SetColor(new Color(r, g, b));

        transform.position = (Vector2) transform.position;
    }

    public void SetColor(Color color) {
        GetComponent<SpriteRenderer>().color = color;
    }

    [Command]
    void CmdGetEaten() {
        //Network.Destroy(this.gameObject);
        NetworkServer.Destroy(this.gameObject);
        Networking.instance.foodCount--;
    }
}
