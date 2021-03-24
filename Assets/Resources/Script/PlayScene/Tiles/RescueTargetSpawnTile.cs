using UnityEditor;
using UnityEngine.Tilemaps;

public class RescueTargetSpawnTile : Tile {
#if UNITY_EDITOR
    [MenuItem("Assets/Create/Tiles/RescueTargetSpawnTile")]
    public static void CreateTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save RescueTargetSpawnTile", "New RescueTargetSpawnTile", "asset", "Save RescueTargetSpawnTile", "Assets");
        if (path == "")
            return;

        AssetDatabase.CreateAsset(CreateInstance<RescueTargetSpawnTile>(), path);
    }
#endif
}
