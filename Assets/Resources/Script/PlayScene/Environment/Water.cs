using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour {
    public Sprite WaterSprite, WaterElectricSprite;
    private SpriteRenderer spriteRenderer;
    private HashSet<Vector3Int> originElectrics;
    private Vector3Int _position;
    public Vector3Int position {
        set { _position = value; }
        get { return _position; }
    }


    private void Start() {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        originElectrics = new HashSet<Vector3Int>();
    }

	private void OnDestroy() {
        TileMgr.Instance.RemoveWater(position);
    }

	void Update() {
    }

    public void Electrify(Vector3Int originPos) {
        if (originElectrics.Count == 0) {
            tag = "Water(Electric)";
            spriteRenderer.sprite = WaterElectricSprite;
        }
        originElectrics.Add(originPos);
    }
    public void RemoveElectric(Vector3Int originPos) {
        originElectrics.Remove(originPos);
        if (originElectrics.Count == 0) {
            tag = "Water";
            spriteRenderer.sprite = WaterSprite;
        }
    }
    public bool ExistOriginElectric(Vector3Int pos) {
        return originElectrics.Contains(pos);
	}
}
