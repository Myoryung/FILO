using UnityEngine;
using UnityEngine.UI;
using System.Xml;
using System.Collections.Generic;

public class G_Arrive : Goal {

    private Dictionary<KeyValuePair<Vector3Int, int>, bool> points = new Dictionary<KeyValuePair<Vector3Int, int>, bool>();
    int arriveCount = 0;

    public G_Arrive(GameObject textObject, XmlNode goalNode) : base(GoalType.ARRIVE, textObject) {
        // Load XML
        XmlNodeList targetNodes = goalNode.SelectNodes("Target");
        foreach (XmlNode targetNode in targetNodes) {
            string[] pointTokens = targetNode.SelectSingleNode("Point").InnerText.Split(',');
            int x = int.Parse(pointTokens[0]), y = int.Parse(pointTokens[1]);
            int floor = int.Parse(targetNode.SelectSingleNode("Floor").InnerText);

            Vector3Int point = new Vector3Int(x, y, 0);
            points.Add(new KeyValuePair<Vector3Int, int>(point, floor), false);
        }

        // Set Text
        ExplanationText.text = "위치 도달";
        RefreshText();
    }

    public void CheckArriveAt(Vector3Int point, int floor) {
        KeyValuePair<Vector3Int, int> key = new KeyValuePair<Vector3Int, int>(point, floor);
        if (points.ContainsKey(key) && !points[key]) {
            points[key] = true;
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