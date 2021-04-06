using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gas : MonoBehaviour {
    private Vector3Int tilePos;
    private int dirCount = 0;
    
    private void Start() {
        tilePos = TileMgr.Instance.WorldToCell(transform.position);
    }

    public void Move() {
        Vector3Int dir = Vector3Int.zero;
        switch (dirCount) {
        case 0: dir = Vector3Int.right; break;
        case 1: dir = Vector3Int.down;  break;
        case 2: dir = Vector3Int.left;  break;
        case 3: dir = Vector3Int.up;    break;
        }

        tilePos += dir;
        transform.position = TileMgr.Instance.CellToWorld(tilePos);

        if (++dirCount >= 4)
            dirCount = 0;
    }
}
