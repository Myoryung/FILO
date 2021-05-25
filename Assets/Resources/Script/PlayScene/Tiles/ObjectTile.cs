using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Rendering;


public class ObjectTile : Tile {
    public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go) {
        if (go != null) {
            int order = TileMgr.GetOrder(go.transform.position, position);
            go.GetComponent<SortingGroup>().sortingOrder = order;
        }

        return base.StartUp(position, tilemap, go);
    }

#if UNITY_EDITOR
    [MenuItem("Assets/Create/Tiles/ObjectTile")]
    public static void CreateTile() {
        string path = EditorUtility.SaveFilePanelInProject("Save ObjectTile", "New ObjectTile", "asset", "Save ObjectTile", "Assets");
        if (path == "")
            return;

        AssetDatabase.CreateAsset(CreateInstance<ObjectTile>(), path);
    }
#endif
}
