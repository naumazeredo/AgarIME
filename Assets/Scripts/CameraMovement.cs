using UnityEngine;

public class CameraMovement : MonoBehaviour {
    public Transform follow;

	void Update () {
        if (follow != null) {
            Vector3 position = Vector3.Lerp(
                transform.position,
                follow.position,
                Time.deltaTime
            );
            position.z = -10f;
            transform.position = position;
        }
    }
}
