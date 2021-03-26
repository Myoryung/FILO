using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class GoalMgr {
    private List<Goal> goals = new List<Goal>();
    private G_Deadline deadline;

    private GameObject GoalTextPrefab = Resources.Load<GameObject>("Prefabs/GoalText");
    private GameObject StageGoals = GameObject.Find("UICanvas/PlayCanvas/TopLeftUI/StageGoals");

    public GoalMgr(XmlNode goalsNode) {
        Vector3 relativePos = StageGoals.transform.position - StageGoals.transform.parent.position;

        // Load XML
        XmlNodeList goalNodes = goalsNode.SelectNodes("Goal");
        foreach (XmlNode goalNode in goalNodes) {
            Goal.GoalType type = Goal.StringToType(goalNode.SelectSingleNode("Type").InnerText);

            GameObject textObject = GameObject.Instantiate(GoalTextPrefab, StageGoals.transform.position, Quaternion.identity, StageGoals.transform.parent);
			textObject.transform.Translate(0, -22.5f*goals.Count, 0);

			Goal goalObject = null;
			switch (type) {
            case Goal.GoalType.DEADLINE:
                deadline = new G_Deadline(textObject, goalNode);
                goalObject = deadline;
                break;
            case Goal.GoalType.RESCUE_SURVIVOR:
                break;
            case Goal.GoalType.EXTINGUISH:
                break;
            case Goal.GoalType.ARRIVE:
                break;
            case Goal.GoalType.RESCUE_SPECIFIC_SURVIVOR:
                break;
            }

            goals.Add(goalObject);
        }

		float oh = StageGoals.GetComponent<RectTransform>().rect.height/2;
		StageGoals.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 45*(goals.Count+1));
		float ch = StageGoals.GetComponent<RectTransform>().rect.height/4;
		StageGoals.transform.Translate(0, oh - ch, 0);
	}

    public bool IsAllSatisfied() {
        return false;
	}
    public bool IsImpossible() {
        return false;
	}

    public int GetStartTime() {
        return deadline.StartTime;
    }
    public void TurnEnd() {
        deadline.TurnEnd();
    }

    // 시간 지남
    // 구조함
    // 불 없앰, 불 생김
    // 대원 이동
}
