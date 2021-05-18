using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gas : MonoBehaviour {
    private int dirCount = 0;

    public void Move() {
        Vector3Int dir = Vector3Int.zero;
        switch (dirCount) {
        case 0: dir = Vector3Int.right; break;
        case 1: dir = Vector3Int.down;  break;
        case 2: dir = Vector3Int.left;  break;
        case 3: dir = Vector3Int.up;    break;
        }

        transform.position += dir;

        if (++dirCount >= 4)
            dirCount = 0;
    }
}
