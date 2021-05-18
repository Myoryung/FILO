using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour {
    private int floor;
    private Vector3Int tilePos;

    [SerializeField]
    private GameObject EmberPrefab = null;
    private GameObject Ember;
    private List<Vector3Int> EmberArea = new List<Vector3Int>();
    
    private void Start() {
        Ember = Instantiate(EmberPrefab, transform);
        Ember.SetActive(false);
        floor = transform.parent.parent.GetComponent<Floor>().floor;
        tilePos = TileMgr.Instance.WorldToCell(transform.position, floor);
    }

    public void MoveEmber() {
        // 빈 영역 확인
        for (int y = -2; y <= 2; y++) {
            for (int x = -2; x <= 2; x++) {
                Vector3Int tempPos = tilePos + new Vector3Int(x, y, 0);
                if (!TileMgr.Instance.ExistObject(tempPos, floor) && !TileMgr.Instance.ExistEnvironment(tempPos, floor))
                    EmberArea.Add(tempPos);
            }
		}

        if (EmberArea.Count == 0) {
            Ember.SetActive(false);
            return;
        }

        int index = Random.Range(0, EmberArea.Count);
        Vector3Int nPos = EmberArea[index];

        Ember.transform.position = TileMgr.Instance.CellToWorld(nPos, floor);
        Ember.SetActive(true);
    }

    public Vector3Int TilePos {
        get { return tilePos; }
    }
}
