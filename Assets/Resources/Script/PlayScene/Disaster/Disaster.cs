using UnityEngine;

public class Disaster {
    public enum DisasterType {
        FALLING_ROCK, SHORT_CIRCUIT, FLASHOVER, SMOKE
    }

    public readonly DisasterType type;
    public readonly Vector3Int position;
    public readonly int floor;
    private int leftTurn;
    private bool isHaveTalk;
    private int talkID;

    public Disaster(DisasterType type, Vector3Int position, int floor, int turn, bool talkFlag, int talkID) {
        this.type = type;
        this.position = position;
        this.leftTurn = turn;
        this.floor = floor;
        this.isHaveTalk = talkFlag;
        this.talkID = talkID;
	}

    public virtual void Update() {
        leftTurn--;
    }

    public int LeftTurn {
        get { return leftTurn; }
	}
    public bool IsSatisfied {
        get { return LeftTurn <= 0; }
    }
    public bool IsSatisfiedWhenNextTurn {
        get { return LeftTurn <= 1; }
	}
    public bool IsHaveTalk{
        get { return isHaveTalk; }
    }
    public int TalkID{
        get { return talkID; }
    }

    public static DisasterType StringToType(string text) {
        switch (text) {
        case "FALLING_ROCK": return DisasterType.FALLING_ROCK;
        case "SHORT_CIRCUIT": return DisasterType.SHORT_CIRCUIT;
        case "FLASHOVER": return DisasterType.FLASHOVER;
        case "SMOKE": return DisasterType.SMOKE;
        }
        return DisasterType.FALLING_ROCK;
    }
}