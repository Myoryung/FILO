using System;
using UnityEditor;
using UnityEngine;

public class INO_DoorController : InteractiveObject {
	public override void Activate() {
        if (!IsAvailable()) return;
        base.Activate();

        INO_Door door = (INO_Door)TileMgr.Instance.GetInteractiveObject(TileMgr.GetDoorPos(Position));
        if (door != null)
            door.Activate();
    }
}
