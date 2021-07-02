using System;
using UnityEditor;
using UnityEngine;

public class INO_Vent : InteractiveObject {
    private void Awake() {
        conditionText = "주변에 대원 1명 존재";
    }

    public override void Activate() {
        if (!IsAvailable()) return;
        base.Activate();

        Gas[] gases = TileMgr.Instance.GetGases(floor);
        foreach (Gas gas in gases)
            gas.Disable();
    }
}
