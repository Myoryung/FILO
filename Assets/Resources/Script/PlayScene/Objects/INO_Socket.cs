using UnityEngine;
using UnityEngine.Tilemaps;

public class INO_Socket : InteractiveObject {
    public TileBase electricTile;

    public override bool IsAvailable() {
        return false;
    }

    public override void Activate() {
        base.Activate();

        if (TileMgr.Instance.ExistElectric(Position))
            TileMgr.Instance.RemoveElectric(Position);
        else
            TileMgr.Instance.CreateElectric(Position);
    }
}
