using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour {
    private const float CAMERA_Z = -100;

    private Transform target = null;

	private void LateUpdate() {
        if (target != null) {
            Vector3 pos = target.position;
            transform.position = Vector3.Lerp(transform.position, pos, 3.0f * Time.deltaTime); // 카메라 이동
            transform.position = new Vector3(transform.position.x, transform.position.y, CAMERA_Z); // Z값 고정
        }
    }

    public void SetTarget(Transform target) {
        this.target = target;
    }
    public void SetPosition(Vector3 pos) {
        transform.position = new Vector3(pos.x, pos.y, CAMERA_Z);
    }
}
