using UnityEngine;
using UnityEngine.UI;
using System.Xml;
using System.Collections.Generic;

public class G_Arrive : Goal {

    private Dictionary<Vector3Int, bool> points = new Dictionary<Vector3Int, bool>();
    int arriveCount = 0;

    public G_Arrive(GameObject textObject, XmlNode goalNode) : base(GoalType.ARRIVE, textObject) {
        // Load XML
        XmlNodeList pointNodes = goalNode.SelectNodes("Point");
        foreach (XmlNode pointNode in pointNodes) {
            string[] pointTokens = pointNode.InnerText.Split(',');

            Vector3Int point = new Vector3Int(int.Parse(pointTokens[0]), int.Parse(pointTokens[1]), 0);
            points.Add(point, false);
        }

        // Set Text
        ExplanationText.text = "위치 도달";
        RefreshText();
    }

    public void CheckArriveAt(Vector3Int point) {
        if (points.ContainsKey(point) && !points[point]) {
            points[point] = true;
            arriveCount++;
            RefreshText();
        }
    }

    public override bool IsSatisfied() {
        return arriveCount == points.Count;
	}
    public override bool IsImpossible() {
        return false;
    }
    protected override void RefreshText() {
        if (arriveCount == points.Count)
            StatusText.text = "성공";
        else
            StatusText.text = string.Format("{0} / {1}", arriveCount, points.Count);
    }
}