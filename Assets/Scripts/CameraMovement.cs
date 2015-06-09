using UnityEngine;

public class CameraMovement : MonoBehaviour {
    public Transform follow;

	void FixedUpdate() {
        if (follow != null) {
            Vector3 position = Vector3.Lerp(
                transform.position,
                follow.position,
                2 * Time.deltaTime
            );
            position.z = -10f;
            transform.position = position;
        }
    }
}
