using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Xml;

public class TileMgr {   
    private static TileMgr m_instance; // Singleton

    private List<Tilemap> BackgroundTilemaps = new List<Tilemap>();
    private List<Tilemap> ObjectTilemaps = new List<Tilemap>();
    private List<Tilemap> EnvironmentTilemaps = new List<Tilemap>();
    private List<Tilemap> SpawnTilemaps = new List<Tilemap>();
    private List<Tilemap> EffectTilemaps = new List<Tilemap>();
    private List<Tilemap> WarningTilemaps = new List<Tilemap>();

    [SerializeField]
    private TileBase FireTile = null, FireWallTile = null, ElectricTile = null, EffectTile = null;

    private float EmberMoveTime = 0.0f;
    private int _currentFloor;

    private List<GameObject> Floors;

    private readonly int FloorSize, MinFloor, MaxFloor, StartFloor;

    private Dictionary<Vector3Int, Vector3Int> doorPairs = new Dictionary<Vector3Int, Vector3Int>(){
        {new Vector3Int(-2, -4, 2), new Vector3Int(0, -4, 2)},
    };
    private Dictionary<Vector3Int, Vector3Int> socketPairs = new Dictionary<Vector3Int, Vector3Int>(){
        {new Vector3Int(2, -2, 2), new Vector3Int(4, -3, 2)},
    };

    public static TileMgr Instance {
        get {
            if (m_instance == null)
                m_instance = new TileMgr();
            return m_instance;
        }
    }

    public TileMgr() {
        Floors = new List<GameObject>();

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
        for(int i=MinFloor;i<=MaxFloor;i++)
        {
            Object floorPath = Resources.Load("Stage/Stage" + GameMgr.Instance.stage + "/Floor" + i);
            GameObject floor = (GameObject)Object.Instantiate(floorPath, ParentFloor.transform);
            floor.SetActive(true);
            Floors.Add(floor);

            floor.name = "Floor" + i;
            BackgroundTilemaps.Add(floor.transform.Find("Background").gameObject.GetComponent<Tilemap>());
            ObjectTilemaps.Add(floor.transform.Find("Object").gameObject.GetComponent<Tilemap>());
            EnvironmentTilemaps.Add(floor.transform.Find("Environment").gameObject.GetComponent<Tilemap>());
            SpawnTilemaps.Add(floor.transform.Find("Spawn").gameObject.GetComponent<Tilemap>());
            EffectTilemaps.Add(floor.transform.Find("Effect").gameObject.GetComponent<Tilemap>());
            WarningTilemaps.Add(floor.transform.Find("Warning").gameObject.GetComponent<Tilemap>());
        }
<<<<<<< HEAD
        //  Load Tilemap Object
        SwitchFloorTilemap(StartFloor, 0);
=======
>>>>>>> 3f9abde... Add: 계단 타일 추가

        // Load Prefab
        FireTile = Resources.Load<TileBase>("Tilemap/Enviroment/Fire");
        FireWallTile = Resources.Load<TileBase>("Tilemap/Object/FireWall");
        ElectricTile = Resources.Load<TileBase>("Tilemap/Enviroment/Electric");
        EffectTile = Resources.Load<TileBase>("Tilemap/Effect/Effect");
    }

    public void SpreadFire() {
        foreach (Tilemap environmentTilemap in EnvironmentTilemaps) {
            Fire[] fires = environmentTilemap.GetComponentsInChildren<Fire>();
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

        foreach (Tilemap environmentTilemap in EnvironmentTilemaps) {
            Fire[] fires = environmentTilemap.GetComponentsInChildren<Fire>();
            foreach (Fire fire in fires)
                fire.MoveEmber();
        }
    }
    public void MoveGas() {
        foreach (Tilemap environmentTilemap in EnvironmentTilemaps) {
            Gas[] gasArr = environmentTilemap.GetComponentsInChildren<Gas>();
            foreach (Gas gas in gasArr)
                gas.Move();
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
        Vector3Int cellPos = BackgroundTilemaps[(int)pos.z - MinFloor].WorldToCell(pos);
        cellPos.z = (int)pos.z;
        return cellPos;
    }
    public Vector3 CellToWorld(Vector3Int pos) {
        Debug.Log(pos.z);
        Tilemap floor = BackgroundTilemaps[pos.z - MinFloor];

        Vector3 worldPos = floor.CellToWorld(pos) + floor.cellSize/2.0f;
        worldPos.z = pos.z;
        Debug.Log(worldPos);

        return worldPos;
    }

    public void SetEffect(Vector3Int pos, Color color) {
        int floorIndex = pos.z - MinFloor;
        Vector3Int basePos = pos;
        basePos.z = 0;

        EffectTilemaps[floorIndex].SetTile(pos, EffectTile);
        EffectTilemaps[floorIndex].SetTileFlags(pos, TileFlags.None);
        EffectTilemaps[floorIndex].SetColor(pos, color);
    }
    public void RemoveEffect(Vector3Int pos) {
        int floorIndex = pos.z - MinFloor;
        Vector3Int basePos = pos;
        basePos.z = 0;

        EffectTilemaps[floorIndex].SetTile(basePos, null);
    }

    public void SetWarning(Vector3Int pos) {
        int floorIndex = pos.z - MinFloor;
        Vector3Int basePos = pos;
        basePos.z = 0;

        WarningTilemaps[floorIndex].SetTile(basePos, EffectTile);
        WarningTilemaps[floorIndex].SetTileFlags(basePos, TileFlags.None);
        WarningTilemaps[floorIndex].SetColor(basePos, new Color(1, 0, 0, 0));
    }
    public void TurnWarning(Vector3Int pos, bool flag) {
        int floorIndex = pos.z - MinFloor;
        Vector3Int basePos = pos;
        basePos.z = 0;

        if (WarningTilemaps[floorIndex].GetTile(basePos) == null) return;

        Color color = WarningTilemaps[floorIndex].GetColor(basePos);
        color.a = (flag) ? 0.5f : 0;
        WarningTilemaps[floorIndex].SetTileFlags(basePos, TileFlags.None);
        WarningTilemaps[floorIndex].SetColor(basePos, color);
    }
    public void RemoveWarning(Vector3Int pos) {
        int floorIndex = pos.z - MinFloor;
        Vector3Int basePos = pos;
        basePos.z = 0;

        WarningTilemaps[floorIndex].SetTile(basePos, null);
    }

    public void CreateFire(Vector3Int pos) {
        int floorIndex = pos.z - MinFloor;
        Vector3Int basePos = pos;
        basePos.z = 0;

        EnvironmentTilemaps[floorIndex].SetTile(basePos, FireTile);
    }
    public void CreateElectric(Vector3Int pos) {
        int floorIndex = pos.z - MinFloor;
        Vector3Int basePos = pos;
        basePos.z = 0;

        EnvironmentTilemaps[floorIndex].SetTile(basePos, ElectricTile);
        Electrify(pos);
    }
    public void CreateFireWall(Vector3Int pos) {
        int floorIndex = pos.z - MinFloor;
        Vector3Int basePos = pos;
        basePos.z = 0;

        ObjectTilemaps[floorIndex].SetTile(basePos, FireWallTile);
	}

    public bool ExistObject(Vector3Int pos) {
        int floorIndex = pos.z - MinFloor;
        Vector3Int basePos = pos;
        basePos.z = 0;

        return ObjectTilemaps[floorIndex].GetTile(basePos) != null;
	}
    public bool ExistEnvironment(Vector3Int pos) {
        int floorIndex = pos.z - MinFloor;
        Vector3Int basePos = pos;
        basePos.z = 0;

        return EnvironmentTilemaps[floorIndex].GetTile(basePos) != null;
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
        int floorIndex = pos.z - MinFloor;
        Vector3Int basePos = pos;
        basePos.z = 0;

        return EnvironmentTilemaps[floorIndex].GetInstantiatedObject(basePos).GetComponent<Water>();
    }
    public InteractiveObject GetInteractiveObject(Vector3Int pos) {
        int floorIndex = pos.z - MinFloor;
        Vector3Int basePos = pos;
        basePos.z = 0;

        GameObject obj = ObjectTilemaps[floorIndex].GetInstantiatedObject(basePos);
        if (obj != null)
            return obj.GetComponent<InteractiveObject>();
        return null;
    }
    public INO_Door GetMatchedDoor(Vector3Int pos) {
        Vector3Int doorPos = doorPairs[pos];
        int floorIndex = doorPos.z - MinFloor;
        doorPos.z = 0;

        return ObjectTilemaps[floorIndex].GetInstantiatedObject(doorPos).GetComponent<INO_Door>();
    }
    public INO_Socket GetMatchedSocket(Vector3Int pos) {
        Vector3Int socketPos = socketPairs[pos];
        int floorIndex = socketPos.z - MinFloor;
        socketPos.z = 0;

        return ObjectTilemaps[floorIndex].GetInstantiatedObject(socketPos).GetComponent<INO_Socket>();
    }

    public void RemoveFire(Vector3Int pos) {
        RemoveEnvironmentTile(pos, "Fire");
    }
    public void RemoveElectric(Vector3Int pos) {
        Diselectrify(pos);
        RemoveEnvironmentTile(pos, "Electric");
    }
    public void RemoveTempWall(Vector3Int pos) {
        RemoveObjectTile(pos, "TempWall");
    }
    public void RemoveDoor(Vector3Int pos) {
		RemoveObjectTile(pos, "Door");
	}

    private bool ExistObjectTile(Vector3Int pos, string name) {
        int floorIndex = pos.z - MinFloor;
        Vector3Int basePos = pos;
        basePos.z = 0;

        TileBase tile = ObjectTilemaps[floorIndex].GetTile(basePos);
        if (tile != null && tile.name == name)
            return true;
        return false;
    }
    private bool ExistEnvironmentTile(Vector3Int pos, string name) {
        int floorIndex = pos.z - MinFloor;
        Vector3Int basePos = pos;
        basePos.z = 0;

        TileBase tile = EnvironmentTilemaps[floorIndex].GetTile(basePos);
        if (tile != null && tile.name == name)
            return true;
        return false;
    }
    private void RemoveObjectTile(Vector3Int pos, string name) {
        int floorIndex = pos.z - MinFloor;
        Vector3Int basePos = pos;
        basePos.z = 0;

        TileBase tile = ObjectTilemaps[floorIndex].GetTile(basePos);
        if (tile != null && tile.name == name)
            ObjectTilemaps[floorIndex].SetTile(basePos, null);
    }
    private void RemoveEnvironmentTile(Vector3Int pos, string name) {
        int floorIndex = pos.z - MinFloor;
        Vector3Int basePos = pos;
        basePos.z = 0;

        TileBase tile = EnvironmentTilemaps[floorIndex].GetTile(basePos);
        if (tile != null && tile.name == name)
            EnvironmentTilemaps[floorIndex].SetTile(basePos, null);
    }
    private void SwitchFloorTilemap(int floorNumber) {
        int nextFloorIdx = floorNumber - MinFloor;

        BackgroundTilemaps[_currentFloor].GetComponent<SpriteRenderer>().enabled = false;
        EffectTilemaps[_currentFloor].GetComponent<SpriteRenderer>().enabled = false;
        WarningTilemaps[_currentFloor].GetComponent<SpriteRenderer>().enabled = false;
        
        BackgroundTilemaps[nextFloorIdx].GetComponent<SpriteRenderer>().enabled = true;
        EffectTilemaps[nextFloorIdx].GetComponent<SpriteRenderer>().enabled = true;
        WarningTilemaps[nextFloorIdx].GetComponent<SpriteRenderer>().enabled = true;
        
        _currentFloor = nextFloorIdx;
    }
}
