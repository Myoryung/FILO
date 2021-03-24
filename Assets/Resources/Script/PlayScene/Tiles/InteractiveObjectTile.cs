using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class InteractiveObjectTile : Tile {
    public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go) {
        if (go != null) {
            InteractiveObject ino = go.GetComponent<InteractiveObject>();
            ino.position = position;
            TileMgr.Instance.SetInteractiveObject(position, ino);
        }

        return base.StartUp(position, tilemap, go);
    }

#if UNITY_EDITOR
    [MenuItem("Assets/Create/Tiles/InteractiveObjectTile")]
    public static void CreateInteractiveObjectTile() {
        string path = EditorUtility.SaveFilePanelInProject("Save InteractiveObjectTile", "New InteractiveObjectTile", "asset", "Save InteractiveObjectTile", "Assets");
        if (path == "")
            return;

        AssetDatabase.CreateAsset(CreateInstance<InteractiveObjectTile>(), path);
    }
#endif
}