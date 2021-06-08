using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FOV_Obstacle : MonoBehaviour
{
    public BoxCollider2D leftBox;
    public BoxCollider2D rightBox;
    public BoxCollider2D upBox;
    public BoxCollider2D downBox;
    // Start is called before the first frame update
    void Start()
    {
        int obstacleFloor = transform.GetComponentInParent<Floor>().floor;
        Vector3Int pos = TileMgr.Instance.WorldToCell(transform.position, obstacleFloor);
        if (TileMgr.Instance.ExistObstacle(pos + Vector3Int.left, obstacleFloor))
            leftBox.enabled = true;
        if (TileMgr.Instance.ExistObstacle(pos + Vector3Int.right, obstacleFloor))
            rightBox.enabled = true;
        if (TileMgr.Instance.ExistObstacle(pos + Vector3Int.up, obstacleFloor))
            upBox.enabled = true;
        if (TileMgr.Instance.ExistObstacle(pos + Vector3Int.down, obstacleFloor))
            downBox.enabled = true;
    }
}
