using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour {
    public Sprite WaterSprite, WaterElectricSprite;
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
            spriteRenderer.sprite = WaterElectricSprite;
        }
        originElectrics.Add(originPos);
    }
    public void RemoveElectric(Vector3Int originPos) {
        originElectrics.Remove(originPos);
        if (originElectrics.Count == 0) {
            tag = "Water";
            boxCollider.isTrigger = false;
            spriteRenderer.sprite = WaterSprite;
        }
    }
    public bool ExistOriginElectric(Vector3Int pos) {
        return originElectrics.Contains(pos);
	}
}
