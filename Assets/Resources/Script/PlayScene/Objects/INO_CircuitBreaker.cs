using System;
using System.Collections.Generic;
using UnityEngine;

public class INO_CircuitBreaker : InteractiveObject {

	public override bool IsAvailable() {
		return true;
	}

	public override void Activate() {
        if (!IsAvailable()) return;
        base.Activate();

        INO_Socket socket = (INO_Socket)TileMgr.Instance.GetInteractiveObject(TileMgr.GetSocketPos(Position));
        if (socket != null)
            socket.Activate();
    }
}
