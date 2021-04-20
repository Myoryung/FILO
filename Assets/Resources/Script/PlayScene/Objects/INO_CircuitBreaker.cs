using System;
using System.Collections.Generic;
using UnityEngine;

public class INO_CircuitBreaker : InteractiveObject {

    private void Awake() {
        conditionText = "주변에 대원 1명 존재";
    }

    public override bool IsActive() {
        return true;
    }
    public override bool IsAvailable() {
		return true;
	}

	public override void Activate() {
        if (!IsAvailable()) return;
        base.Activate();

        INO_Socket socket = TileMgr.Instance.GetMatchedSocket(tilePos);
        if (socket != null)
            socket.Activate();
    }
}
