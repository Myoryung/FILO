using System.Collections.Generic;
using System.Xml;
using UnityEngine;

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
        TextAsset textAsset = (TextAsset)Resources.Load("Disaster/Stage" + stage);
        doc.LoadXml(textAsset.text);

        XmlNodeList disasterNodes = doc.SelectNodes("Disasters/Disaster");
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
}
