using System;
using UnityEditor;
using UnityEngine;

public class INO_ElevatorPowerSupply : InteractiveObject {
    public override bool IsAvailable() {
        return false;
    }

    public bool ExistAroundElectric() {
        for (int y = -1; y <= 1; y++) {
            for (int x = -1; x <= 1; x++) {
                Vector3Int pos = Position + new Vector3Int(x, y, 0);
                if (TileMgr.Instance.ExistElectric(pos))
                    return true;
            }
        }

        return false;
    }
}
