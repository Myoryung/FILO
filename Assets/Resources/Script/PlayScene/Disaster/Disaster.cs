using UnityEngine;

public class Disaster {
    public enum DisasterType {
        FALLING_ROCK, SHORT_CIRCUIT, FLASHOVER, SMOKE
    }

    public readonly DisasterType type;
    public readonly Vector3Int position;
    private int leftTurn;

    public Disaster(DisasterType type, Vector3Int position, int turn) {
        this.type = type;
        this.position = position;
        this.leftTurn = turn;
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