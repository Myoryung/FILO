using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour {
    public GameObject electricPtc;
    public Sprite[] waterRule;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;

    private HashSet<Vector3Int> originElectrics = new HashSet<Vector3Int>();
    private Vector3Int _position;
    public Vector3Int position {
        set { _position = value; }
        get { return _position; }
    }

    private void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    public void Electrify(Vector3Int originPos) {
        if (originElectrics.Count == 0) {
            tag = "Water(Electric)";
            boxCollider.isTrigger = true;
            electricPtc.SetActive(true);
        }
        originElectrics.Add(originPos);
    }
    public void RemoveElectric(Vector3Int originPos) {
        originElectrics.Remove(originPos);
        if (originElectrics.Count == 0) {
            tag = "Water";
            boxCollider.isTrigger = false;
            electricPtc.SetActive(false);
        }
    }
    public bool ExistOriginElectric(Vector3Int pos) {
        return originElectrics.Contains(pos);
	}

    private void ChangeRuleSprite()
    {
        bool top = TileMgr.Instance.ExistWater(_position + Vector3Int.up);
        bool bottom = TileMgr.Instance.ExistWater(_position + Vector3Int.down);
        bool left = TileMgr.Instance.ExistWater(_position + Vector3Int.left);
        bool right = TileMgr.Instance.ExistWater(_position + Vector3Int.right);

        if(top && bottom && left && right)
        {

        }
    }
}
