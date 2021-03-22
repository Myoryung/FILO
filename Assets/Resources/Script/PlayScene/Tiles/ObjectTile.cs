using UnityEditor;
using UnityEngine.Tilemaps;

public class ObjectTile : Tile {
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
