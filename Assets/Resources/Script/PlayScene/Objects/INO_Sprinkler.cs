using System;
using System.Collections.Generic;
using UnityEngine;

public class INO_Sprinkler : InteractiveObject {
	private void Awake() {
		conditionText = "주변에 대원 1명 존재";
	}

	public override void Activate() {
        if (!IsAvailable()) return;
        base.Activate();

		for (int y = -2; y <= 2; y++) {
			for (int x = -2; x <= 2; x++) {
				Vector3Int firePos = tilePos + new Vector3Int(x, y, 0);
				TileMgr.Instance.RemoveFire(firePos, floor);
			}
		}
	}
}
