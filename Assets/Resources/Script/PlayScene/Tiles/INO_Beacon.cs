using System;
using UnityEditor;
using UnityEngine;

public class INO_Beacon : InteractiveObject {

    [SerializeField]
    private GameObject BeaconPrefab = null;

    public override bool IsAvailable() {
        if (!base.IsAvailable()) return false;

        int aroundPlayerCount = 0;
        foreach (Player player in GameMgr.Instance.Comp_Players) {
            if ((position-player.currentTilePos).magnitude < 2)
                aroundPlayerCount++;
        }

        return aroundPlayerCount >= 2;
    }

    public override void Activate() {
        if (!IsAvailable()) return;
        base.Activate();

        Instantiate(BeaconPrefab, transform.position, Quaternion.identity);
    }
}
