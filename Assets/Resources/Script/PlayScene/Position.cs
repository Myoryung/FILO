using System.Xml;
using UnityEngine;

class Position {
    public int x, y;
    public int floor;

    public Position() {

    }
    public Position(int x, int y, int floor) {
        this.x = x;
        this.y = y;
        this.floor = floor;
    }
    public Position(XmlNode node) {
        string[] pointToken = node.SelectSingleNode("Point").InnerText.Split(',');
        string floorStr = node.SelectSingleNode("Floor").InnerText;

        x = int.Parse(pointToken[0]);
        y = int.Parse(pointToken[1]);
        floor = int.Parse(floorStr);
    }

    public Vector3Int Point {
        get { return new Vector3Int(x, y, 0); }
    }
}