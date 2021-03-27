using UnityEngine;
using UnityEngine.UI;
using System.Xml;

public class G_RescueImportantSurvivor : Goal {
    private int survivorNum = 0;
    private int goalRescueCount, rescueCount = 0;

    public G_RescueImportantSurvivor(GameObject textObject, XmlNode goalNode) : base(GoalType.RESCUE_IMPORTANT_SURVIVOR, textObject) {
        // Load XML
        string countStr = goalNode.SelectSingleNode("Count").InnerText;

        goalRescueCount = int.Parse(countStr);

        // Set Text
        ExplanationText.text = "중요 생존자 구조";
        StatusText.text = string.Format("{0} / {1}", rescueCount, goalRescueCount);
    }

    public void IncreaseSurvivorNum() {
        survivorNum++;
	}
    public void OnRescueSurvivor() {
        survivorNum--;
        rescueCount++;
        RefreshText();
    }
    public void OnDieSurvivor() {
        survivorNum--;
        RefreshText();
    }

    public override bool IsSatisfied() {
        return rescueCount >= goalRescueCount;
	}
    public override bool IsImpossible() {
        return (goalRescueCount - rescueCount) > survivorNum;
    }
    protected override void RefreshText() {
        StatusText.text = string.Format("{0} / {1}", rescueCount, goalRescueCount);
        if (IsSatisfied())
            StatusText.text = "성공";
        else if (IsImpossible())
            StatusText.text = "실패";
    }
}