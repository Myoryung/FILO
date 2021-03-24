using System;
using System.Collections.Generic;
using UnityEngine;

public class INO_Sprinkler : InteractiveObject {
    public override void Activate() {
        if (!IsAvailable()) return;
        base.Activate();

        //List<Fire> fires = TileMgr.Instance.Fires;
        //int count = (int)Math.Ceiling(fires.Count / 2.0);

        //for (int i = 0; i < count; i++) {
        //    int index = UnityEngine.Random.Range(0, fires.Count);
        //    GameMgr.Instance.Obstacle.SetTile(fires[index].Pos, null);
        //    fires.RemoveAt(index);
        //}
    }
}
