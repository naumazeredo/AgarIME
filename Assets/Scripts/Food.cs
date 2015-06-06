using UnityEngine;

public class Food : MonoBehaviour {
	void OnEnable() {
        GetComponent<SpriteRenderer>().color = new Color(Random.value, Random.value, Random.value);
	}
}
