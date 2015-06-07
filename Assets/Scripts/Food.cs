using UnityEngine;

public class Food : MonoBehaviour {
    [SerializeField]
    private float size = 1f;

    public void SetColor(Color color) {
        GetComponent<SpriteRenderer>().color = color;
    }
}
