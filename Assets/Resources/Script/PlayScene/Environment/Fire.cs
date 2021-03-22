using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour {
    private Vector3Int TilePos;

    [SerializeField]
    private GameObject EmberPrefab = null;
    private GameObject Ember;
    private List<Vector3Int> EmberArea = new List<Vector3Int>();
    
    private void Start() {
        TilePos = TileMgr.Instance.WorldToCell(transform.position);

        transform.Translate(0, 0, -1);
        Ember = Instantiate(EmberPrefab, transform);
        Ember.SetActive(false);
    }

    public void MoveEmber() {
        // 빈 영역 확인
        for (int y = -2; y <= 2; y++) {
            for (int x = -2; x <= 2; x++) {
                Vector3Int tempPos = TilePos + new Vector3Int(x, y, 0);
                if (!TileMgr.Instance.ExistObject(tempPos) && !TileMgr.Instance.ExistEnvironment(tempPos))
                    EmberArea.Add(tempPos);
            }
		}

        if (EmberArea.Count == 0) {
            Ember.SetActive(false);
            return;
        }

        int index = Random.Range(0, EmberArea.Count);
        Vector3Int nPos = EmberArea[index];

        Ember.transform.position = TileMgr.Instance.CellToWorld(nPos);
        Ember.SetActive(true);
    }
}
