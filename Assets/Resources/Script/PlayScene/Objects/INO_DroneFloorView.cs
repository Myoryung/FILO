using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class INO_DroneFloorView : MonoBehaviour {
    private const int ACTIVE_TURN = 3;
    private int activeTurnCount = ACTIVE_TURN;

    public void TurnUpdate() {
        if (--activeTurnCount <= 0) {
            Destroy(this);
        }
    }

    public void SetFloor(int floor) {
        // 층에 따른 FOV 설정
        Vector3 middlePos = TileMgr.Instance.CellToWorld(Vector3Int.zero, floor);
        transform.position = middlePos;

        Vector2 floorSize = TileMgr.Instance.GetFloorSize(floor);
        transform.Find("MainView").GetComponent<SpriteRenderer>().size = floorSize;
        transform.Find("SubView").GetComponent<SpriteRenderer>().size = floorSize;
    }
}
