using UnityEngine;

public class Flammable : MonoBehaviour {
    public bool isConnectedLeft, isConnectedUp, isConnectedRight, isConnectedDown;
    private int flamingCount = 0;

    public void CatchFire() {
        tag = "Flammable";
    }
    public void Extinguish() {
        flamingCount = 0;
        tag = "Flaming";
    }

    public void Flaming() {
        if (flamingCount++ >= 5) {
            Vector3Int pos = TileMgr.Instance.WorldToCell(transform.position);
            TileMgr.Instance.RemoveFlaming(pos);
		}
	}
}
