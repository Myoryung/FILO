using UnityEngine;
using UnityEngine.UI;
using System.Xml;
using System.Collections.Generic;

public class G_Extinguish : Goal {

    private List<RectInt> areas = new List<RectInt>();

    public G_Extinguish(GameObject textObject, XmlNode goalNode) : base(GoalType.EXTINGUISH, textObject) {
        // Load XML
        XmlNodeList areaNodes = goalNode.SelectNodes("Area");
        foreach (XmlNode areaNode in areaNodes) {
            string[] LT_Tokens = areaNode.SelectSingleNode("LT").InnerText.Split(',');
            string[] RB_Tokens = areaNode.SelectSingleNode("RB").InnerText.Split(',');

            Vector3Int LT = new Vector3Int(int.Parse(LT_Tokens[0]), int.Parse(LT_Tokens[1]), 0);
            Vector3Int RB = new Vector3Int(int.Parse(RB_Tokens[0]), int.Parse(RB_Tokens[1]), 0);
            Vector3Int size = (RB-LT);

            RectInt area = new RectInt(LT.x, LT.y, size.x, size.y);
            areas.Add(area);
        }

        // Set Text
        ExplanationText.text = "화재 진압";
        RefreshText();
    }

    public void CheckFireInArea() {
        RefreshText();
    }

    private int GetFireExistCount() {
        int fireExistCount = 0;

        foreach (RectInt area in areas) {
            if (ExistFireInArea(area))
                fireExistCount++;
        }

        return fireExistCount;
	}
    private bool ExistFireInArea(RectInt area) {
        for (int y = area.yMin; y <= area.yMax; y++) {
            for (int x = area.xMin; x <= area.xMax; x++) {
                if (TileMgr.Instance.ExistFire(new Vector3Int(x, y, 0)))
                    return true;
            }
        }
        return false;
	}

    public override bool IsSatisfied() {
        return GetFireExistCount() == 0;
	}
    public override bool IsImpossible() {
        return false;
    }
    protected override void RefreshText() {
        int fireExistCount = GetFireExistCount();
        StatusText.text = string.Format("{0} / {1}", areas.Count-fireExistCount, areas.Count);
        if (fireExistCount == 0)
            StatusText.text = "성공";
    }
}