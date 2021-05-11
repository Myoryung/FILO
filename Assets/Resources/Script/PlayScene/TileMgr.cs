﻿using System.Collections.Generic;
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

    private TileBase FireTile = null, FireWallTile = null, ElectricTile = null, EffectTile = null;

    private float EmberMoveTime = 0.0f;
    private int _currentFloor;

    private List<GameObject> Floors;

    public readonly int FloorSize, MinFloor, MaxFloor, StartFloor;

    private Dictionary<Vector3Int, Vector3Int> doorPairs = new Dictionary<Vector3Int, Vector3Int>(){
        {new Vector3Int(2, 1, 4), new Vector3Int(2, -1, 4)},
    };
    private Dictionary<Vector3Int, Vector3Int[]> socketPairs = new Dictionary<Vector3Int, Vector3Int[]>(){
        {new Vector3Int(-18, -13, 3), new Vector3Int[2] { new Vector3Int(-12, -9, 3), new Vector3Int(11, 7, 3)} },
    };
    private Dictionary<Vector3Int, Vector3Int[]> elevatorPairs = new Dictionary<Vector3Int, Vector3Int[]>(){
        {new Vector3Int(10, 7, 3), new Vector3Int[6] { new Vector3Int(-16, 9, 2), new Vector3Int(-13, 9, 2)
        , new Vector3Int(-16, 9, 3), new Vector3Int(-13, 9, 3), new Vector3Int(-13, 9, 4), new Vector3Int(-16, 9, 4)} }
    };

    public static TileMgr Instance {
        get { return m_instance; }
    }
    public static void CreateInstance(int stage) {
        if (m_instance == null)
            m_instance = new TileMgr(stage);
    }

    private TileMgr(int stage) {
        Floors = new List<GameObject>();

        // Load XML
        XmlDocument doc = new XmlDocument();
        TextAsset textAsset = (TextAsset)Resources.Load("Stage/Stage" + stage);
        doc.LoadXml(textAsset.text);

        XmlNode floorNode = doc.SelectSingleNode("Stage/Floors");
        FloorSize = int.Parse(floorNode.SelectSingleNode("FloorSize").InnerText);
        MinFloor = int.Parse(floorNode.SelectSingleNode("MinFloor").InnerText);
        MaxFloor = int.Parse(floorNode.SelectSingleNode("MaxFloor").InnerText);
        StartFloor = int.Parse(floorNode.SelectSingleNode("StartFloor").InnerText);
        _currentFloor = StartFloor - MinFloor;

        GameObject ParentFloor = GameObject.Find("Floor");
        for (int i = MinFloor; i <= MaxFloor; i++) {
            Object floorPath = Resources.Load("Stage/Stage" + stage + "/Floor" + i);
            GameObject floor = (GameObject)Object.Instantiate(floorPath, ParentFloor.transform);
            floor.SetActive(true);
            Floors.Add(floor);

            floor.name = "Floor" + i;
            Tilemap backgroundTilemap = floor.transform.Find("Background").gameObject.GetComponent<Tilemap>();

            backgroundTilemap.CompressBounds();
            BackgroundTilemaps.Add(backgroundTilemap);
            ObjectTilemaps.Add(floor.transform.Find("Object").gameObject.GetComponent<Tilemap>());
            EnvironmentTilemaps.Add(floor.transform.Find("Environment").gameObject.GetComponent<Tilemap>());
            SpawnTilemaps.Add(floor.transform.Find("Spawn").gameObject.GetComponent<Tilemap>());
            EffectTilemaps.Add(floor.transform.Find("Effect").gameObject.GetComponent<Tilemap>());
            WarningTilemaps.Add(floor.transform.Find("Warning").gameObject.GetComponent<Tilemap>());
        }
        SwitchFloorTilemap(StartFloor);

        // Load Prefab
        FireTile = Resources.Load<TileBase>("Tilemap/Environment/Fire");
        FireWallTile = Resources.Load<TileBase>("Tilemap/Object/FireWall");
        ElectricTile = Resources.Load<TileBase>("Tilemap/Environment/Electric");
        EffectTile = Resources.Load<TileBase>("Tilemap/Effect/Effect");
    }

    public void SpreadFire() {
        for (int floor = MinFloor; floor <= MaxFloor; floor++) {
            Fire[] fires = EnvironmentTilemaps[floor].GetComponentsInChildren<Fire>();
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
                float prob = Random.Range(0.0f, 1.0f);
                if (prob > createProb[pos]) continue;

                if (ExistFlammable(pos, floor))
                    SpreadFireFlammable(pos, floor);
                else if (!ExistObject(pos, floor) && !ExistEnvironment(pos, floor))
                    CreateFire(pos, floor);
            }
        }
    }

    public void Flaming() {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Flaming");
        foreach (GameObject obj in objects)
            obj.GetComponent<Flammable>().Flaming();
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
    public void UpdateFloorView() {
        GameObject[] floorViewObjs = GameObject.FindGameObjectsWithTag("FloorView");
        if (floorViewObjs.Length <= 0) return;
        foreach (GameObject floorViewObj in floorViewObjs) {
            INO_DroneFloorView floorView = floorViewObj.GetComponent<INO_DroneFloorView>();
            floorView.TurnUpdate();
        }
    }

    private void Electrify(Vector3Int electricPos, int floor) {
        HashSet<Vector3Int> searchHistory = new HashSet<Vector3Int>();
        Queue<Vector3Int> searchQueue = new Queue<Vector3Int>();

        for (int y = -1; y <= 1; y++) {
            for (int x = -1; x <= 1; x++) {
                Vector3Int wPos = electricPos + new Vector3Int(x, y, 0);
                if (ExistWater(wPos, floor)) {
                    searchQueue.Enqueue(wPos);
                    searchHistory.Add(wPos);
                }
            }
        }

        while (searchQueue.Count > 0) {
            Vector3Int currPos = searchQueue.Dequeue();
            Water water = GetWater(currPos, floor);
            water.Electrify(electricPos);

            for (int y = -1; y <= 1; y++) {
                for (int x = -1; x <= 1; x++) {
                    Vector3Int nextPos = currPos + new Vector3Int(x, y, 0);
                    if (!searchHistory.Contains(nextPos) && ExistWater(nextPos, floor)) {
                        searchQueue.Enqueue(nextPos);
                        searchHistory.Add(nextPos);
                    }
                }
            }
        }
    }
    private void Diselectrify(Vector3Int electricPos, int floor) {
        HashSet<Vector3Int> searchHistory = new HashSet<Vector3Int>();
        Queue<Vector3Int> searchQueue = new Queue<Vector3Int>();

        for (int y = -1; y <= 1; y++) {
            for (int x = -1; x <= 1; x++) {
                Vector3Int wPos = electricPos + new Vector3Int(x, y, 0);
                if (ExistWater(wPos, floor)) {
                    searchQueue.Enqueue(wPos);
                    searchHistory.Add(wPos);
                }
            }
        }

        while (searchQueue.Count > 0) {
            Vector3Int currPos = searchQueue.Dequeue();
            Water water = GetWater(currPos, floor);
            water.RemoveElectric(electricPos);

            for (int y = -1; y <= 1; y++) {
                for (int x = -1; x <= 1; x++) {
                    Vector3Int nextPos = currPos + new Vector3Int(x, y, 0);
                    if (!searchHistory.Contains(nextPos) && ExistWater(nextPos, floor)) {
                        searchQueue.Enqueue(nextPos);
                        searchHistory.Add(nextPos);
                    }
                }
            }
        }
    }

    private void SpreadFireFlammable(Vector3Int startPos, int floor) {
        Queue<Vector3Int> searchQueue = new Queue<Vector3Int>();
        HashSet<Vector3Int> searchHistory = new HashSet<Vector3Int>();

        searchQueue.Enqueue(startPos);
        searchHistory.Add(startPos);

        while (searchQueue.Count > 0) {
            Vector3Int pos = searchQueue.Dequeue();
            Vector3Int nPos;

            Flammable flammable = GetFlammable(pos, floor);
            flammable.CatchFire();
            CreateFire(pos, floor);

            nPos = pos + Vector3Int.up;
            if (flammable.isConnectedUp && !searchHistory.Contains(nPos)) {
                searchQueue.Enqueue(nPos);
                searchHistory.Add(nPos);
            }

            nPos = pos + Vector3Int.down;
            if (flammable.isConnectedDown && !searchHistory.Contains(nPos)) {
                searchQueue.Enqueue(nPos);
                searchHistory.Add(nPos);
            }

            nPos = pos + Vector3Int.left;
            if (flammable.isConnectedLeft && !searchHistory.Contains(nPos)) {
                searchQueue.Enqueue(nPos);
                searchHistory.Add(nPos);
            }

            nPos = pos + Vector3Int.right;
            if (flammable.isConnectedRight && !searchHistory.Contains(nPos)) {
                searchQueue.Enqueue(nPos);
                searchHistory.Add(nPos);
            }
        }
    }
    private void ExtinguishFireFlammable(Vector3Int startPos, int floor) {
        Queue<Vector3Int> searchQueue = new Queue<Vector3Int>();
        HashSet<Vector3Int> searchHistory = new HashSet<Vector3Int>();

        searchQueue.Enqueue(startPos);
        searchHistory.Add(startPos);

        while (searchQueue.Count > 0) {
            Vector3Int pos = searchQueue.Dequeue();
            Vector3Int nPos;

            Flammable flammable = GetFlammable(pos, floor);
            flammable.Extinguish();

            nPos = pos + Vector3Int.up;
            if (flammable.isConnectedUp && !searchHistory.Contains(nPos)) {
                searchQueue.Enqueue(nPos);
                searchHistory.Add(nPos);
            }

            nPos = pos + Vector3Int.down;
            if (flammable.isConnectedDown && !searchHistory.Contains(nPos)) {
                searchQueue.Enqueue(nPos);
                searchHistory.Add(nPos);
            }

            nPos = pos + Vector3Int.left;
            if (flammable.isConnectedLeft && !searchHistory.Contains(nPos)) {
                searchQueue.Enqueue(nPos);
                searchHistory.Add(nPos);
            }

            nPos = pos + Vector3Int.right;
            if (flammable.isConnectedRight && !searchHistory.Contains(nPos)) {
                searchQueue.Enqueue(nPos);
                searchHistory.Add(nPos);
            }
        }
    }

    public Vector3Int WorldToCell(Vector3 pos, int floor) {
        return BackgroundTilemaps[floor - MinFloor].WorldToCell(pos);
    }
    public Vector3 CellToWorld(Vector3Int pos, int floor) {
        Tilemap floorMap = BackgroundTilemaps[floor - MinFloor];
        return floorMap.CellToWorld(pos) + floorMap.cellSize/2.0f;
    }

    public void SetEffect(Vector3Int pos, int floor, Color color) {
        int floorIndex = floor - MinFloor;

        EffectTilemaps[floorIndex].SetTile(pos, EffectTile);
        EffectTilemaps[floorIndex].SetTileFlags(pos, TileFlags.None);
        EffectTilemaps[floorIndex].SetColor(pos, color);
    }
    public void RemoveEffect(Vector3Int pos, int floor) {
        EffectTilemaps[floor - MinFloor].SetTile(pos, null);
    }

    public void SetWarning(Vector3Int pos, int floor) {
        int floorIndex = floor - MinFloor;

        WarningTilemaps[floorIndex].SetTile(pos, EffectTile);
        WarningTilemaps[floorIndex].SetTileFlags(pos, TileFlags.None);
        WarningTilemaps[floorIndex].SetColor(pos, new Color(1, 0, 0, 0));
    }
    public void TurnWarning(Vector3Int pos, int floor, bool flag) {
        int floorIndex = floor - MinFloor;

        if (WarningTilemaps[floorIndex].GetTile(pos) == null) return;

        Color color = WarningTilemaps[floorIndex].GetColor(pos);
        color.a = (flag) ? 0.5f : 0;
        WarningTilemaps[floorIndex].SetTileFlags(pos, TileFlags.None);
        WarningTilemaps[floorIndex].SetColor(pos, color);
    }
    public void RemoveWarning(Vector3Int pos, int floor) {
        WarningTilemaps[floor - MinFloor].SetTile(pos, null);
    }

    public void CreateFire(Vector3Int pos, int floor) {
        int floorIndex = floor - MinFloor;
        EnvironmentTilemaps[floorIndex].SetTile(pos, FireTile);
    }
    public void CreateElectric(Vector3Int pos, int floor) {
        EnvironmentTilemaps[floor - MinFloor].SetTile(pos, ElectricTile);
        Electrify(pos, floor);
    }
    public void CreateFireWall(Vector3Int pos, int floor) {
        ObjectTilemaps[floor - MinFloor].SetTile(pos, FireWallTile);
	}

    public bool ExistObject(Vector3Int pos, int floor) {
        return ObjectTilemaps[floor - MinFloor].GetTile(pos) != null;
	}
    public bool ExistEnvironment(Vector3Int pos, int floor) {
        return EnvironmentTilemaps[floor - MinFloor].GetTile(pos) != null;
	}
    public bool ExistFire(Vector3Int pos, int floor) {
        return ExistEnvironmentTile(pos, floor, "Fire");
    }
    public bool ExistWater(Vector3Int pos, int floor) {
        return ExistEnvironmentTile(pos, floor, "Water") || ExistEnvironmentTile(pos, floor, "Water(Electric)");
    }
    public bool ExistElectric(Vector3Int pos, int floor) {
        return ExistEnvironmentTile(pos, floor, "Electric") || ExistEnvironmentTile(pos, floor, "Water(Electric)");
    }
    public bool ExistTempWall(Vector3Int pos, int floor) {
        return ExistObjectTile(pos, floor, "TempWall");
    }
    public bool ExistPlayerSpawn(Vector3Int pos, int floor) {
        //return SpawnTilemap.GetTile(pos) != null;
        return false;
    }
    public bool ExistFlammable(Vector3Int pos, int floor) {
        return ExistObjectTile(pos, floor, "Flammable");
    }
    public bool ExistFlaming(Vector3Int pos, int floor) {
        return ExistObjectTile(pos, floor, "Flaming");
    }

    private Water GetWater(Vector3Int pos, int floor) {
        return EnvironmentTilemaps[floor - MinFloor].GetInstantiatedObject(pos).GetComponent<Water>();
    }
    private Flammable GetFlammable(Vector3Int pos, int floor) {
        return ObjectTilemaps[floor - MinFloor].GetInstantiatedObject(pos).GetComponent<Flammable>();
    }
    public InteractiveObject GetInteractiveObject(Vector3Int pos, int floor) {
        GameObject obj = ObjectTilemaps[floor - MinFloor].GetInstantiatedObject(pos);
        if (obj != null)
            return obj.GetComponent<InteractiveObject>();
        return null;
    }
	public OperatorSpawn[] GetOperatorSpawns(int floor) {
        return SpawnTilemaps[floor - MinFloor].transform.GetComponentsInChildren<OperatorSpawn>();
    }
	public INO_Door GetMatchedDoor(Vector3Int pos, int floor) {
        Vector3Int doorPos = doorPairs[pos];
        int floorIndex = doorPos.z - MinFloor;
        doorPos.z = 0;

        return ObjectTilemaps[floorIndex].GetInstantiatedObject(doorPos).GetComponent<INO_Door>();
    }
    public INO_Socket[] GetMatchedSocket(Vector3Int pos, int floor) {
        INO_Socket[] sockets= new INO_Socket[socketPairs[pos].Length];
        if (sockets.Length <= 0) return null;
        for (int i = 0; i < socketPairs[pos].Length; i++)
        {
            int floorIndex = socketPairs[pos][i].z - MinFloor;
            socketPairs[pos][i].z = 0;
            sockets[i] = ObjectTilemaps[floorIndex].GetInstantiatedObject(socketPairs[pos][i]).GetComponent<INO_Socket>();
        }

        return sockets;
    }
    public INO_Elevator[] GetMatchedElevators(Vector3Int pos, int floor) {

        INO_Elevator[] elevators = new INO_Elevator[elevatorPairs[pos].Length];
        if (elevators.Length <= 0) return null;
        for(int i=0;i < elevatorPairs[pos].Length; i++)
        {
            int floorIndex = elevatorPairs[pos][i].z - MinFloor;
            elevatorPairs[pos][i].z = 0;
            elevators[i] = ObjectTilemaps[floorIndex].GetInstantiatedObject(elevatorPairs[pos][i]).GetComponent<INO_Elevator>();
            Debug.Log(elevators[i]);
        }

        return elevators;
    }

    public void RemoveFire(Vector3Int pos, int floor) {
        RemoveEnvironmentTile(pos, floor, "Fire");
		if (ExistFlaming(pos, floor))
            ExtinguishFireFlammable(pos, floor);
	}
    public void RemoveElectric(Vector3Int pos, int floor) {
        Diselectrify(pos, floor);
        RemoveEnvironmentTile(pos, floor, "Electric");
    }
    public void RemoveTempWall(Vector3Int pos, int floor) {
        RemoveObjectTile(pos, floor, "TempWall");
    }
    public void RemoveDoor(Vector3Int pos, int floor) {
		RemoveObjectTile(pos, floor, "Door");
	}
    public void RemoveDrone(Vector3Int pos, int floor) {
        RemoveObjectTile(pos, floor);
    }

    private bool ExistObjectTile(Vector3Int pos, int floor, string tag) {
        GameObject obj = ObjectTilemaps[floor - MinFloor].GetInstantiatedObject(pos);
        if (obj != null && obj.CompareTag(tag))
            return true;
        return false;
    }
    private bool ExistEnvironmentTile(Vector3Int pos, int floor, string tag) {
        GameObject obj = EnvironmentTilemaps[floor - MinFloor].GetInstantiatedObject(pos);
        if (obj != null && obj.CompareTag(tag))
            return true;
        return false;
    }
    private void RemoveObjectTile(Vector3Int pos, int floor, string tag = null) {
        int floorIndex = floor - MinFloor;

        GameObject obj = ObjectTilemaps[floorIndex].GetInstantiatedObject(pos);
        if (obj != null && (tag == null || obj.CompareTag(tag)))
            ObjectTilemaps[floorIndex].SetTile(pos, null);
    }
    private void RemoveEnvironmentTile(Vector3Int pos, int floor, string tag) {
        int floorIndex = floor - MinFloor;

        GameObject obj = EnvironmentTilemaps[floorIndex].GetInstantiatedObject(pos);
        if (obj != null && obj.CompareTag(tag))
            EnvironmentTilemaps[floorIndex].SetTile(pos, null);
    }

    public void SwitchFloorTilemap(int floorNumber) {
        int nextFloorIdx = floorNumber - MinFloor;

        BackgroundTilemaps[_currentFloor].GetComponent<TilemapRenderer>().enabled = false;
        EffectTilemaps[_currentFloor].GetComponent<TilemapRenderer>().enabled = false;
        WarningTilemaps[_currentFloor].GetComponent<TilemapRenderer>().enabled = false;
        
        BackgroundTilemaps[nextFloorIdx].GetComponent<TilemapRenderer>().enabled = true;
        EffectTilemaps[nextFloorIdx].GetComponent<TilemapRenderer>().enabled = true;
        WarningTilemaps[nextFloorIdx].GetComponent<TilemapRenderer>().enabled = true;
        
        _currentFloor = nextFloorIdx;
    }
    public Vector2 GetFloorSize(int floor) {
        Tilemap tilemap = BackgroundTilemaps[floor - MinFloor];
        BoundsInt bounds = tilemap.cellBounds;

        Vector2 size = new Vector2(tilemap.cellSize.x * bounds.size.x, tilemap.cellSize.y * bounds.size.y);
        return size;
    }
}
