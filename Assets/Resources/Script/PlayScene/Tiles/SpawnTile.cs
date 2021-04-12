using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SpawnTile : Tile {
#if UNITY_EDITOR
    [MenuItem("Assets/Create/Tiles/SpawnTile")]
    public static void CreateTile() {
        string path = EditorUtility.SaveFilePanelInProject("Save SpawnTile", "New SpawnTile", "asset", "Save SpawnTile", "Assets");
        if (path == "")
            return;

        AssetDatabase.CreateAsset(CreateInstance<SpawnTile>(), path);
    }
#endif
}
