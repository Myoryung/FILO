using UnityEngine;
using UnityEngine.UI;
using System.Xml;

public class G_Deadline : Goal {

    private int endTime;
    private int currTime;

    public G_Deadline(GameObject textObject, XmlNode goalNode) : base(GoalType.DEADLINE, textObject) {
        // Load XML
        string endTimeStr = goalNode.SelectSingleNode("EndTime").InnerText;
        string[] endTimeTokens = endTimeStr.Split(':');
        endTime = int.Parse(endTimeTokens[0])*60 + int.Parse(endTimeTokens[1]);

        ExplanationText.text = "제한시간";
        StatusText.text = string.Format("~ {0}", endTimeStr);
    }

    public void OnTurnEnd(int currTime) {
        this.currTime = currTime;
        RefreshText();
    }

    public override bool IsSatisfied() {
        return currTime <= endTime;
	}
    public override bool IsImpossible() {
        return !IsSatisfied();
    }
    protected override void RefreshText() {
        if (IsImpossible())
            StatusText.text = "실패";
    }

    public int GetDeadline() {
        return endTime;
    }
}