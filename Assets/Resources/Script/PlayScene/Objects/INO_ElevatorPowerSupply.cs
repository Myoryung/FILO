using System;
using UnityEditor;
using UnityEngine;

public class INO_ElevatorPowerSupply : InteractiveObject {
    private void Awake() {
        conditionText = "주변에 전기 존재";
    }

    public override bool IsActive() {
        return true;
    }
    public override bool IsAvailable() {
        return false;
    }

    public bool ExistAroundElectric() {
        for (int y = -1; y <= 1; y++) {
            for (int x = -1; x <= 1; x++) {
                Vector3Int pos = tilePos + new Vector3Int(x, y, 0);
                if (TileMgr.Instance.ExistElectric(pos))
                    return true;
            }
        }

        return false;
    }
}
