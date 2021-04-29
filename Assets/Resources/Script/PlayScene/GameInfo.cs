using System;
using System.Collections.Generic;

public class GameInfo {
    private int stageNumber;
    private int currTime, deadline;

    private int toolUseCount = 0;
    private int survivorNum = 0;
    private int rescuedSurvivorNum = 0;

    public GameInfo(int stageNumber) {
        this.stageNumber = stageNumber;
    }

    public void OnUseTool() {
        toolUseCount++;
    }
    public void OnAddSurvivor() {
        survivorNum++;
    }
    public void OnRescueSurvivor() {
        rescuedSurvivorNum++;
    }

    public int StageNumber {
        get { return stageNumber; }
    }
    public int CurrTime {
        get { return currTime; }
        set { currTime = value; }
    }
    public int Deadline {
        get { return deadline; }
        set { deadline = value; }
    }

    public int ToolUseCount {
        get { return toolUseCount; }
    }
    public int SurvivorNum {
        get { return survivorNum; }
    }
    public int RescuedSurvivorNum {
        get { return rescuedSurvivorNum; }
    }
    public int UnrescuedSurvivorNum {
        get { return survivorNum - rescuedSurvivorNum; }
    }
}