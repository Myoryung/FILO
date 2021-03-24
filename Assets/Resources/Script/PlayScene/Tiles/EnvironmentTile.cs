using UnityEditor;
using UnityEngine.Tilemaps;

public class EnvironmentTile : Tile
{
#if UNITY_EDITOR
    [MenuItem("Assets/Create/Tiles/EnvironmentTile")]
    public static void CreateTile() {
        string path = EditorUtility.SaveFilePanelInProject("Save EnvironmentTile", "New EnvironmentTile", "asset", "Save EnvironmentTile", "Assets");
        if (path == "") return;

        AssetDatabase.CreateAsset(CreateInstance<EnvironmentTile>(), path);
    }
#endif
}
