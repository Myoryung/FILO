using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class GoalMgr {
    private List<Goal> mainGoals = new List<Goal>();
    private List<Goal> subGoals = new List<Goal>();
    private G_Deadline deadline = null;
    private G_RescueSurvivor rescueSurvivor = null;
    private G_Arrive arrive = null;
    private G_RescueImportantSurvivor rescueImportantSurvivor = null;

    private GameObject GoalTextPrefab = Resources.Load<GameObject>("Prefabs/UI/StageGoal");
    private GameObject StageGoals = GameObject.Find("UICanvas/PlayCanvas/TopLeftUI/StageGoals");

    public GoalMgr(XmlNode goalsNode) {
        // Load Main Goal
        XmlNodeList mainGoalNodes = goalsNode.SelectNodes("Main/Goal");
        foreach (XmlNode goalNode in mainGoalNodes) {
            GameObject textObject = GameObject.Instantiate(GoalTextPrefab, StageGoals.transform);
            textObject.transform.localPosition = new Vector3(0, -17 + -22.5f + -45f*mainGoals.Count, 0);

            Goal goal = CreateGoal(textObject, goalNode);
            
            mainGoals.Add(goal);
        }
		GameObject end = StageGoals.transform.Find("End").gameObject;
        end.transform.localPosition = new Vector3(0, -17 + -45f*mainGoals.Count + -19.5f, 0);

        // Load Sub Goal
        XmlNodeList subGoalNodes = goalsNode.SelectNodes("Sub/Goal");
        GameObject subStart = StageGoals.transform.Find("SubStart").gameObject;
        GameObject subEnd = StageGoals.transform.Find("SubEnd").gameObject;
        float subStartY = -30 + -34 + -17 + -45f*mainGoals.Count;

        if (subGoalNodes.Count == 0) {
            subStart.SetActive(false);
            subEnd.SetActive(false);
        }
        else {
            subStart.transform.localPosition = new Vector3(0, subStartY + -17, 0);

            foreach (XmlNode goalNode in subGoalNodes) {
                GameObject textObject = GameObject.Instantiate(GoalTextPrefab, StageGoals.transform);
                textObject.transform.localPosition = new Vector3(0, subStartY + -34 + -22.5f + -45f*subGoals.Count, 0);

                Goal goal = CreateGoal(textObject, goalNode);

                subGoals.Add(goal);
            }

            subEnd.transform.localPosition = new Vector3(0, subStartY + -34 + -45f*subGoals.Count + -19.5f, 0);
        }
    }
    private Goal CreateGoal(GameObject textObject, XmlNode goalNode) {
        Goal.GoalType type = Goal.StringToType(goalNode.SelectSingleNode("Type").InnerText);

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
        case Goal.GoalType.ARRIVE:
            arrive = new G_Arrive(textObject, goalNode);
            goalObject = arrive;
            break;
        case Goal.GoalType.RESCUE_IMPORTANT_SURVIVOR:
            rescueImportantSurvivor = new G_RescueImportantSurvivor(textObject, goalNode);
            goalObject = rescueSurvivor;
            break;
        }

        return goalObject;
    }

    public List<Goal> GetMainGoals() {
        return mainGoals;
	}

    public bool IsAllSatisfied() {
        bool isSatisfied = true;
        foreach (Goal goal in mainGoals) {
            if (!goal.IsSatisfied()) {
                isSatisfied = false;
                break;
            }
		}
        return isSatisfied;
	}
    public bool IsImpossible() {
        bool isImpossible = false;
        foreach (Goal goal in mainGoals) {
            if (goal.IsImpossible()) {
                isImpossible = true;
                break;
            }
        }
        return isImpossible;
	}

    public void CheckArriveAt(Vector3Int pos) {
        if (arrive != null)
            arrive.CheckArriveAt(pos);
    }
    public void OnTurnEnd(int currTime) {
        if (deadline != null)
            deadline.OnTurnEnd(currTime);
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
