using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractiveObject : MonoBehaviour
{
    protected Vector3Int position;
    public Vector3Int Position {
        get { return position; }
	}
    private bool IsBeenUsed = false;

	private void Start() {
        position = TileMgr.Instance.WorldToCell(transform.position);
        TileMgr.Instance.SetInteractiveObject(Position, this);
    }

	private void OnDestroy() {
        TileMgr.Instance.SetInteractiveObject(Position, null);
    }

    public virtual bool IsAvailable() {
        if (IsBeenUsed) return false;
        return true;
    }

    public virtual void Activate() {
        if (!IsAvailable()) return;
        IsBeenUsed = true;
    }
}
