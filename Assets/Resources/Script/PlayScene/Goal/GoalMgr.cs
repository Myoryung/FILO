using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class GoalMgr {
    private List<Goal> goals = new List<Goal>();
    private G_Deadline deadline = null;
    private G_RescueSurvivor rescueSurvivor = null;
    private G_Extinguish extinguish = null;
    private G_Arrive arrive = null;
    private G_RescueImportantSurvivor rescueImportantSurvivor = null;

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
                rescueSurvivor = new G_RescueSurvivor(textObject, goalNode);
                goalObject = rescueSurvivor;
                break;
            case Goal.GoalType.EXTINGUISH:
                extinguish = new G_Extinguish(textObject, goalNode);
                goalObject = extinguish;
                break;
            case Goal.GoalType.ARRIVE:
                arrive = new G_Arrive(textObject, goalNode);
                goalObject = arrive;
                break;
            case Goal.GoalType.RESCUE_IMPORTANT_SURVIVOR:
                rescueImportantSurvivor = new G_RescueImportantSurvivor(textObject, goalNode);
                goalObject = rescueSurvivor;
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
        bool isSatisfied = true;
        foreach (Goal goal in goals) {
            if (!goal.IsSatisfied()) {
                isSatisfied = false;
                break;
            }
		}
        return isSatisfied;
	}
    public bool IsImpossible() {
        bool isImpossible = false;
        foreach (Goal goal in goals) {
            if (goal.IsImpossible()) {
                isImpossible = true;
                break;
            }
        }
        return isImpossible;
	}

    public void CheckFireInArea() {
        if (extinguish != null)
            extinguish.CheckFireInArea();
    }
    public void CheckArriveAt(Vector3Int pos) {
        if (arrive != null)
            arrive.CheckArriveAt(pos);
    }
    public void OnTurnEnd() {
        if (deadline != null)
            deadline.OnTurnEnd();
    }
    public void OnRescueSurvivor() {
        if (rescueSurvivor != null)
            rescueSurvivor.OnRescueSurvivor();
    }
    public void OnRescueImportantSurvivor() {
        OnRescueSurvivor();
        if (rescueImportantSurvivor != null)
            rescueImportantSurvivor.OnRescueSurvivor();
    }
    public void OnDieSurvivor() {
        if (rescueSurvivor != null)
            rescueSurvivor.OnDieSurvivor();
    }
    public void OnDieImportantSurvivor() {
        OnDieSurvivor();
        if (rescueImportantSurvivor != null)
            rescueImportantSurvivor.OnDieSurvivor();
    }

    public void OnAddSurvivor() {
        if (rescueSurvivor != null)
            rescueSurvivor.IncreaseSurvivorNum();
    }
    public void OnAddImportantSurvivor() {
        OnAddSurvivor();
        if (rescueImportantSurvivor != null)
            rescueImportantSurvivor.IncreaseSurvivorNum();
    }
}
