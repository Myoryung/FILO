using UnityEngine;
using UnityEngine.Tilemaps;

public class INO_Socket : InteractiveObject {
    public TileBase electricTile;

    public override bool IsAvailable() {
        return false;
    }

    public override void Activate() {
        base.Activate();

        if (TileMgr.Instance.ExistElectric(tilePos, floor))
            TileMgr.Instance.RemoveElectric(tilePos, floor);
        else
            TileMgr.Instance.CreateElectric(tilePos, floor);
    }
}
