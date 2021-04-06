using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Xml;

public class TileMgr {
    private static TileMgr m_instance; // Singleton

    private Tilemap BackgroundTilemap, ObjectTilemap, EnvironmentTilemap, SpawnTilemap;
    private Tilemap EffectTilemap, WarningTilemap;

    [SerializeField]
    private TileBase FireTile = null, FireWallTile = null, ElectricTile = null, EffectTile = null;

    private float EmberMoveTime = 0.0f;
<<<<<<< HEAD
    private bool isChangedFire = false;
    private int _currentFloor;
=======
>>>>>>> 28227f5... Del: 화재진압 목표 삭제

    private List<GameObject> Floors;

    private readonly int FloorSize, MinFloor, MaxFloor, StartFloor;
    private Dictionary<Vector3Int, InteractiveObject> m_interactiveObjects;
    private Dictionary<int, Stair> UpStairs = new Dictionary<int, Stair>();
    private Dictionary<int, Stair> DownStairs = new Dictionary<int, Stair>();

    private static readonly List<Dictionary<Vector3Int, Vector3Int>> DoorPairs = new List<Dictionary<Vector3Int, Vector3Int>>(){
        new Dictionary<Vector3Int, Vector3Int>(){
            {new Vector3Int(-2, -4, 0), new Vector3Int(0, -4, 0)},
        }
    };
    private static readonly List<Dictionary<Vector3Int, Vector3Int>> SocketPairs = new List<Dictionary<Vector3Int, Vector3Int>>(){
        new Dictionary<Vector3Int, Vector3Int>(){
            {new Vector3Int(2, -2, 0), new Vector3Int(4, -3, 0)},
        }
    };
    public static Vector3Int GetDoorPos(Vector3Int pos) {
        return DoorPairs[GameMgr.Instance.stage][pos];
    }
    public static Vector3Int GetSocketPos(Vector3Int pos) {
        return SocketPairs[GameMgr.Instance.stage][pos];
    }

    public static TileMgr Instance {
        get {
            if (m_instance == null)
                m_instance = new TileMgr();
            return m_instance;
        }
    }

    public TileMgr() {
        Floors = new List<GameObject>();
        m_interactiveObjects = new Dictionary<Vector3Int, InteractiveObject>();

        // Load XML
        XmlDocument doc = new XmlDocument();
        TextAsset textAsset = (TextAsset)Resources.Load("Stage/Stage" + GameMgr.Instance.stage);
        doc.LoadXml(textAsset.text);

        XmlNode floorNode = doc.SelectSingleNode("Stage/Floors");
        FloorSize = int.Parse(floorNode.SelectSingleNode("FloorSize").InnerText);
        MinFloor = int.Parse(floorNode.SelectSingleNode("MinFloor").InnerText);
        MaxFloor = int.Parse(floorNode.SelectSingleNode("MaxFloor").InnerText);
        StartFloor = int.Parse(floorNode.SelectSingleNode("StartFloor").InnerText);
        _currentFloor = StartFloor;

        GameObject ParentFloor = GameObject.Find("Floor");
        for (int i = MinFloor; i<=MaxFloor; i++) {
            Object floorPath = Resources.Load("Stage/Stage" + GameMgr.Instance.stage + "/Floor" + i);
            Floors.Add((GameObject)MonoBehaviour.Instantiate(floorPath, Vector3.zero, Quaternion.identity, ParentFloor.transform));
            Floors[i - MinFloor].name = "Floor" + i;
        }
        //  Load Tilemap Object
        SwitchFloorTilemap(StartFloor, 0);

        // Load Prefab
        FireTile = Resources.Load<TileBase>("Tilemap/Enviroment/Fire");
        FireWallTile = Resources.Load<TileBase>("Tilemap/Object/FireWall");
        ElectricTile = Resources.Load<TileBase>("Tilemap/Enviroment/Electric");
        EffectTile = Resources.Load<TileBase>("Tilemap/Effect/Effect");
    }

    public void SetInteractiveObject(Vector3Int pos, InteractiveObject obj) {
        m_interactiveObjects.Remove(pos);
        m_interactiveObjects.Add(pos, obj);
    }
    public InteractiveObject GetInteractiveObject(Vector3Int pos) {
        if (m_interactiveObjects.ContainsKey(pos))
            return m_interactiveObjects[pos];
        return null;
    }

    public void SpreadFire() {
        foreach (GameObject floor in Floors) {
            SwitchFloorTilemap(floor);

            Fire[] fires = EnvironmentTilemap.GetComponentsInChildren<Fire>();
            Dictionary<Vector3Int, float> createProb = new Dictionary<Vector3Int, float>();

            // 확률 계산
            foreach (Fire fire in fires) {
                for (int y = -1; y <= 1; y++) {
                    for (int x = -1; x <= 1; x++) {
                        Vector3Int tPos = fire.TilePos + new Vector3Int(x, y, 0);
                        if (!createProb.ContainsKey(tPos))
                            createProb.Add(tPos, 0.0f);
                        createProb[tPos] += 0.1f;
                    }
                }
            }

            // 불 생성
            foreach (Vector3Int pos in createProb.Keys) {
                if (ExistObject(pos) || ExistEnvironment(pos)) continue;

                float prob = Random.Range(0.0f, 1.0f);
                if (prob <= createProb[pos])
                    CreateFire(pos);
            }
        }
    }
    public void MoveEmbers() {
        EmberMoveTime += Time.deltaTime;
        if (EmberMoveTime < 2.0f)
            return;
        EmberMoveTime = 0.0f;

        foreach (GameObject floor in Floors) {
            SwitchFloorTilemap(floor);

            Fire[] fires = EnvironmentTilemap.GetComponentsInChildren<Fire>();
            foreach (Fire fire in fires)
                fire.MoveEmber();
        }
    }

    private void Electrify(Vector3Int electricPos) {
        HashSet<Vector3Int> searchHistory = new HashSet<Vector3Int>();
        Queue<Vector3Int> searchQueue = new Queue<Vector3Int>();

        for (int y = -1; y <= 1; y++) {
            for (int x = -1; x <= 1; x++) {
                Vector3Int wPos = electricPos + new Vector3Int(x, y, 0);
                if (ExistWater(wPos)) {
                    searchQueue.Enqueue(wPos);
                    searchHistory.Add(wPos);
                }
            }
        }

        while (searchQueue.Count > 0) {
            Vector3Int currPos = searchQueue.Dequeue();
            Water water = GetWater(currPos);
            water.Electrify(electricPos);

            for (int y = -1; y <= 1; y++) {
                for (int x = -1; x <= 1; x++) {
                    Vector3Int nextPos = currPos + new Vector3Int(x, y, 0);
                    if (!searchHistory.Contains(nextPos) && ExistWater(nextPos)) {
                        searchQueue.Enqueue(nextPos);
                        searchHistory.Add(nextPos);
                    }
                }
            }
        }
    }
    private void Diselectrify(Vector3Int electricPos) {
        HashSet<Vector3Int> searchHistory = new HashSet<Vector3Int>();
        Queue<Vector3Int> searchQueue = new Queue<Vector3Int>();

        for (int y = -1; y <= 1; y++) {
            for (int x = -1; x <= 1; x++) {
                Vector3Int wPos = electricPos + new Vector3Int(x, y, 0);
                if (ExistWater(wPos)) {
                    searchQueue.Enqueue(wPos);
                    searchHistory.Add(wPos);
                }
            }
        }

        while (searchQueue.Count > 0) {
            Vector3Int currPos = searchQueue.Dequeue();
            Water water = GetWater(currPos);
            water.RemoveElectric(electricPos);

            for (int y = -1; y <= 1; y++) {
                for (int x = -1; x <= 1; x++) {
                    Vector3Int nextPos = currPos + new Vector3Int(x, y, 0);
                    if (!searchHistory.Contains(nextPos) && ExistWater(nextPos)) {
                        searchQueue.Enqueue(nextPos);
                        searchHistory.Add(nextPos);
                    }
                }
            }
        }
    }
    
    public Vector3Int WorldToCell(Vector3 pos) {
        return BackgroundTilemap.WorldToCell(pos);
    }
    public Vector3 CellToWorld(Vector3Int pos) {
        return BackgroundTilemap.CellToWorld(pos) + BackgroundTilemap.cellSize/2.0f;
    }

    public void SetEffect(Vector3Int pos, Color color) {
        EffectTilemap.SetTile(pos, EffectTile);
        EffectTilemap.SetTileFlags(pos, TileFlags.None);
        EffectTilemap.SetColor(pos, color);
    }
    public void RemoveEffect(Vector3Int pos) {
        EffectTilemap.SetTile(pos, null);
    }

    public void SetWarning(Vector3Int pos) {
        WarningTilemap.SetTile(pos, EffectTile);
        WarningTilemap.SetTileFlags(pos, TileFlags.None);
        WarningTilemap.SetColor(pos, new Color(1, 0, 0, 0));
    }
    public void TurnWarning(Vector3Int pos, bool flag) {
        if (WarningTilemap.GetTile(pos) == null) return;

        Color color = WarningTilemap.GetColor(pos);
        color.a = (flag) ? 0.5f : 0;
        WarningTilemap.SetTileFlags(pos, TileFlags.None);
        WarningTilemap.SetColor(pos, color);
    }
    public void RemoveWarning(Vector3Int pos) {
        WarningTilemap.SetTile(pos, null);
    }

    public void CreateFire(Vector3Int pos) {
        EnvironmentTilemap.SetTile(pos, FireTile);
    }
    public void CreateElectric(Vector3Int pos) {
        EnvironmentTilemap.SetTile(pos, ElectricTile);
        Electrify(pos);
    }
    public void CreateFireWall(Vector3Int pos) {
        ObjectTilemap.SetTile(pos, FireWallTile);
    }

    public bool ExistObject(Vector3Int pos) {
        return ObjectTilemap.GetTile(pos) != null;
    }
    public bool ExistEnvironment(Vector3Int pos) {
        return EnvironmentTilemap.GetTile(pos) != null;
    }
    public bool ExistFire(Vector3Int pos) {
        return ExistEnvironmentTile(pos, "Fire");
    }
    public bool ExistWater(Vector3Int pos) {
        return ExistEnvironmentTile(pos, "Water");
    }
    public bool ExistElectric(Vector3Int pos) {
        return ExistEnvironmentTile(pos, "Electric");
    }
    public bool ExistTempWall(Vector3Int pos) {
        return ExistObjectTile(pos, "TempWall");
    }
    public bool ExistPlayerSpawn(Vector3Int pos) {
        //return SpawnTilemap.GetTile(pos) != null;
        return false;
    }

    private Water GetWater(Vector3Int pos) {
        return EnvironmentTilemap.GetInstantiatedObject(pos).GetComponent<Water>();
    }

    public void RemoveFire(Vector3Int pos) {
        RemoveEnvironmentTile(pos, "Fire");
    }
    public void RemoveWater(Vector3Int pos) {
        RemoveEnvironmentTile(pos, "Water");
    }
    public void RemoveElectric(Vector3Int pos) {
        Diselectrify(pos);
        RemoveEnvironmentTile(pos, "Electric");
    }
    public void RemoveTempWall(Vector3Int pos) {
        RemoveObjectTile(pos, "TempWall");
    }

    private bool ExistObjectTile(Vector3Int pos, string name) {
        TileBase tile = ObjectTilemap.GetTile(pos);
        if (tile != null && tile.name == name)
            return true;
        return false;
    }
    private bool ExistEnvironmentTile(Vector3Int pos, string name) {
        TileBase tile = EnvironmentTilemap.GetTile(pos);
        if (tile != null && tile.name == name)
            return true;
        return false;
    }
    private void RemoveObjectTile(Vector3Int pos, string name) {
        TileBase tile = ObjectTilemap.GetTile(pos);
        if (tile != null && tile.name == name)
            ObjectTilemap.SetTile(pos, null);
    }
    private void RemoveEnvironmentTile(Vector3Int pos, string name) {
        TileBase tile = EnvironmentTilemap.GetTile(pos);
        if (tile != null && tile.name == name)
            EnvironmentTilemap.SetTile(pos, null);
    }
    private void SwitchFloorTilemap(GameObject obj) {
        BackgroundTilemap = obj.transform.Find("Background").gameObject.GetComponent<Tilemap>();
        ObjectTilemap = obj.transform.Find("Object").gameObject.GetComponent<Tilemap>();
        EnvironmentTilemap = obj.transform.Find("Environment").gameObject.GetComponent<Tilemap>();
        SpawnTilemap = obj.transform.Find("Spawn").gameObject.GetComponent<Tilemap>();
        EffectTilemap = obj.transform.Find("Effect").gameObject.GetComponent<Tilemap>();
        WarningTilemap = obj.transform.Find("Warning").gameObject.GetComponent<Tilemap>();
        _currentFloor = idx + flag;
        obj.SetActive(true);
    }
}
