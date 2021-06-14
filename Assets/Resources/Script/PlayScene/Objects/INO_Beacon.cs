using System;
using UnityEditor;
using UnityEngine;

public class INO_Beacon : InteractiveObject {

    [SerializeField]
    private GameObject BeaconPrefab = null;

    private void Awake() {
        conditionText = "주변에 대원 2명 존재";
    }

    public override bool IsAvailable() {
        if (!base.IsAvailable()) return false;

        return GameMgr.Instance.GetAroundPlayerCount(tilePos, floor, 2) >= 2;
    }

    public override void Activate() {
        if (!IsAvailable()) return;
        base.Activate();

        Instantiate(BeaconPrefab, transform.position, Quaternion.identity);
    }
}
