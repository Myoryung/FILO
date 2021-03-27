using UnityEngine;
using UnityEngine.UI;
using System.Xml;

public class G_Deadline : Goal {

    private int startTime, endTime;
    private int currTime;

    private Text timerText = null;

    public G_Deadline(GameObject textObject, XmlNode goalNode) : base(GoalType.DEADLINE, textObject) {
        // Load XML
        string startTimeStr = goalNode.SelectSingleNode("StartTime").InnerText;
        string endTimeStr = goalNode.SelectSingleNode("EndTime").InnerText;

        string[] startTimeTokens = startTimeStr.Split(':');
        string[] endTimeTokens = endTimeStr.Split(':');

        startTime = int.Parse(startTimeTokens[0])*60 + int.Parse(startTimeTokens[1]);
        endTime = int.Parse(endTimeTokens[0])*60 + int.Parse(endTimeTokens[1]);

        currTime = startTime;

        // Set Text
        timerText = GameObject.Find("UICanvas/PlayCanvas/TopUI/TurnEndBtn/TimerText").GetComponent<Text>();
        RefreshText();

        ExplanationText.text = "제한시간";
        StatusText.text = string.Format("~ {0}", endTimeStr);
    }

    public void TurnEnd() {
        currTime += 5;
        RefreshText();
    }

    public int StartTime {
        get { return startTime; }
	}

    public override bool IsSatisfied() {
        return startTime <= currTime && currTime <= endTime;
	}
    public override bool IsImpossible() {
        return !IsSatisfied();
    }
    protected override void RefreshText() {
        timerText.text = string.Format("{0,2}:{1:00}", (currTime / 60), (currTime % 60));
        if (currTime > endTime) {
            StatusText.text = "실패";
		}
    }
}