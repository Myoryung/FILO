using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DisasterMgr {
    private readonly List<Disaster> disasters = new List<Disaster>();

    public static readonly Object FallingRock   = Resources.Load("Prefabs/Disaster/Disaster_FallingRock");
    public static readonly Object ShortCircuit  = Resources.Load("Prefabs/Disaster/Disaster_ShortCircuit");
    public static readonly Object Flashover     = Resources.Load("Prefabs/Disaster/Disaster_Flashover");
    public static readonly Object Smoke         = Resources.Load("Prefabs/Disaster/Disaster_Smoke");

    public DisasterMgr(int stage) {
        LoadStage(stage);
	}
    private void LoadStage(int stage) {
        // XML Load
        XmlDocument doc = new XmlDocument();
        TextAsset textAsset = (TextAsset)Resources.Load("Stage/Stage" + stage);
        doc.LoadXml(textAsset.text);

        XmlNodeList disasterNodes = doc.SelectNodes("Stage/Disasters/Disaster");
        foreach (XmlNode disasterNode in disasterNodes) {
            Disaster.DisasterType type = Disaster.StringToType(disasterNode.SelectSingleNode("Type").InnerText);

			string[] pointStr = disasterNode.SelectSingleNode("Point").InnerText.Replace(" ", "").Split(',');
            int x = int.Parse(pointStr[0]);
            int y = int.Parse(pointStr[1]);
            Vector3Int point = new Vector3Int(x, y, 0);

            int turn = int.Parse(disasterNode.SelectSingleNode("Turn").InnerText);

            disasters.Add(new Disaster(type, point, turn));
        }

        disasters.Sort(delegate (Disaster e1, Disaster e2) {
            return e1.LeftTurn.CompareTo(e2.LeftTurn);
        });
    }

    public DisasterObject TurnUpdate() {
        DisasterObject disasterObject = null;

		for (int i = 0; i < disasters.Count;) {
            disasters[i].Update();

            if (disasters[i].IsSatisfied) {
                disasterObject = CreateDisasterObject(disasters[i]);
                disasters.RemoveAt(i);
            }
            else
                i++;
		}

        return disasterObject;
	}

    public Disaster GetWillActiveDisaster() {
        foreach (Disaster disaster in disasters) {
            if (disaster.IsSatisfiedWhenNextTurn)
                return disaster;
		}
        return null;
	}

    private DisasterObject CreateDisasterObject(Disaster disaster) {
        Object obj = null;
        Vector3 pos = TileMgr.Instance.CellToWorld(disaster.position);

        switch (disaster.type) {
        case Disaster.DisasterType.FALLING_ROCK:  obj = FallingRock;   break;
        case Disaster.DisasterType.SHORT_CIRCUIT: obj = ShortCircuit;  break;
        case Disaster.DisasterType.FLASHOVER:     obj = Flashover;     break;
        case Disaster.DisasterType.SMOKE:         obj = Smoke;         break;
        }

        GameObject gameObj = (GameObject)Object.Instantiate(obj, pos, Quaternion.identity, GameMgr.Instance.transform);
        DisasterObject disasterObject = gameObj.GetComponent<DisasterObject>();
        disasterObject.Pos = disaster.position;

        return disasterObject;
    }

	public IEnumerator UpdateWillActiveDisasterArea() // 다음 재난 일어날 위치 Tilemap에 생성(1턴 후 자동삭제)
	{
		Disaster nextDis = GetWillActiveDisaster();
		if (nextDis != null) // 다음 턴 재난이 없으면 실행되지 않음
		{
			Vector3Int minRange = Vector3Int.zero; // 좌측상단 xy측
			Vector3Int maxRange = Vector3Int.zero; // 우측하단 xy측
			switch (nextDis.type) {
			case Disaster.DisasterType.FALLING_ROCK:
				minRange = new Vector3Int(-1, -1, 0);
				maxRange = new Vector3Int(1, 1, 0);
				break;
			case Disaster.DisasterType.FLASHOVER:
				minRange = new Vector3Int(0, 0, 0);
				maxRange = new Vector3Int(1, 1, 0);
				break;
			case Disaster.DisasterType.SHORT_CIRCUIT:
				minRange = new Vector3Int(0, 0, 0);
				maxRange = new Vector3Int(0, 0, 0);
				break;
			case Disaster.DisasterType.SMOKE:
				minRange = new Vector3Int(-1, -1, 0);
				maxRange = new Vector3Int(1, 1, 0);
				break;
			}

            // 재난 범위에 타일 생성
            for (int i = minRange.x; i <= maxRange.x; i++) {
				for (int j = minRange.y; j <= maxRange.y; j++)
                    TileMgr.Instance.SetWarning(nextDis.position + new Vector3Int(i, j, 0)); 
			}

			int checkTurn = GameMgr.Instance.GameTurn;
			yield return new WaitUntil(() => GameMgr.Instance.CurrGameState == GameMgr.GameState.DISASTER_ALARM);

            // 타일 삭제 부분
            for (int i = minRange.x; i <= maxRange.x; i++) {
				for (int j = minRange.y; j <= maxRange.y; j++)
                    TileMgr.Instance.RemoveWarning(nextDis.position + new Vector3Int(i, j, 0));
			}
		}
	}
}
