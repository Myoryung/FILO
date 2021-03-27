using UnityEngine;
using UnityEngine.UI;

public abstract class Goal {
    public enum GoalType {
        DEADLINE, RESCUE_SURVIVOR, EXTINGUISH, ARRIVE, RESCUE_IMPORTANT_SURVIVOR
	}

    public readonly GoalType type;

	protected Text ExplanationText = null, StatusText = null;

	public Goal(GoalType type, GameObject textObject) {
        this.type = type;

		ExplanationText = textObject.transform.Find("Explanation").GetComponent<Text>();
		StatusText = textObject.transform.Find("Status").GetComponent<Text>();
	}

	public static GoalType StringToType(string text) {
		switch (text) {
		case "DEADLINE":					return GoalType.DEADLINE;
		case "RESCUE_SURVIVOR":				return GoalType.RESCUE_SURVIVOR;
		case "EXTINGUISH":					return GoalType.EXTINGUISH;
		case "ARRIVE":						return GoalType.ARRIVE;
		case "RESCUE_IMPORTANT_SURVIVOR":	return GoalType.RESCUE_IMPORTANT_SURVIVOR;
		}
		return GoalType.DEADLINE;
	}

	public abstract bool IsSatisfied();
	public abstract bool IsImpossible();
	protected abstract void RefreshText();
}