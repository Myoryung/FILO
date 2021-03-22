using UnityEditor;
using UnityEngine.Tilemaps;

public class PlayerSpawnTile : Tile {

#if UNITY_EDITOR
    [MenuItem("Assets/Create/Tiles/PlayerSpawnTile")]
    public static void CreateTile() {
        string path = EditorUtility.SaveFilePanelInProject("Save PlayerSpawnTile", "New PlayerSpawnTile", "asset", "Save PlayerSpawnTile", "Assets");
        if (path == "")
            return;

        AssetDatabase.CreateAsset(CreateInstance<PlayerSpawnTile>(), path);
    }
#endif
}
