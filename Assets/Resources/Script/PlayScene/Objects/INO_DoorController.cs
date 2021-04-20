using System;
using UnityEditor;
using UnityEngine;

public class INO_DoorController : InteractiveObject {
    private void Awake() {
        conditionText = "주변에 대원 1명 존재";
    }

    public override void Activate() {
        if (!IsAvailable()) return;
        base.Activate();

        INO_Door door = TileMgr.Instance.GetMatchedDoor(tilePos);
        if (door != null)
            door.Activate();
    }
}
