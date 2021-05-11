using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEngine;
using System;

public class INO_Door : InteractiveObject {
    public override bool IsAvailable() {
        return false;
    }

    public override void Activate() {
        base.Activate();

        TileMgr.Instance.RemoveDoor(tilePos, floor);
    }
}
