using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour {
    public GameObject electricPtc;
    public SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;

    private HashSet<Vector3Int> originElectrics = new HashSet<Vector3Int>();

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
}
