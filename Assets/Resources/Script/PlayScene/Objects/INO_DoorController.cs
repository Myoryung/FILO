using System;
using UnityEditor;
using UnityEngine;

public class INO_DoorController : InteractiveObject {
	public override void Activate() {
        if (!IsAvailable()) return;
        base.Activate();

        INO_Door door = TileMgr.Instance.GetMatchedDoor(Position);
        if (door != null)
            door.Activate();
    }
}
