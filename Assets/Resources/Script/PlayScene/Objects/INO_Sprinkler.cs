using System;
using System.Collections.Generic;
using UnityEngine;

public class INO_Sprinkler : InteractiveObject {
    public override void Activate() {
        if (!IsAvailable()) return;
        base.Activate();

        List<GameObject> fires = new List<GameObject>(GameObject.FindGameObjectsWithTag("Fire"));
		int count = (int)Math.Ceiling(fires.Count / 2.0);

		for (int i = 0; i < count; i++) {
			int index = UnityEngine.Random.Range(0, fires.Count);
			Vector3Int tilePos = fires[index].GetComponent<Fire>().TilePos;
			TileMgr.Instance.RemoveFire(tilePos);
			fires.RemoveAt(index);
		}
	}
}
