using UnityEngine;
using UnityEngine.UI;
using System.Xml;

public class G_RescueSurvivor : Goal {
    private int survivorNum = 0;
    private int goalRescueCount, rescueCount = 0;

    public G_RescueSurvivor(GameObject textObject, XmlNode goalNode) : base(GoalType.RESCUE_SURVIVOR, textObject) {
        // Load XML
        string countStr = goalNode.SelectSingleNode("Count").InnerText;

        goalRescueCount = int.Parse(countStr);
        survivorNum = goalRescueCount;

        // Set Text
        ExplanationText.text = "생존자 구조";
        StatusText.text = string.Format("{0} / {1}", rescueCount, goalRescueCount);
    }

    public void SetSurvivorNum(int num) {
        survivorNum = num;
	}
    public void Rescue() {
        survivorNum--;
        rescueCount++;
        RefreshText();
    }

    public override bool IsSatisfied() {
        return rescueCount >= goalRescueCount;
	}
    public override bool IsImpossible() {
        return (goalRescueCount - rescueCount) < survivorNum;
    }
    protected override void RefreshText() {
        StatusText.text = string.Format("{0} / {1}", rescueCount, goalRescueCount);
        if (IsSatisfied())
            StatusText.text = "성공";
        else if (IsImpossible())
            StatusText.text = "실패";
    }
}