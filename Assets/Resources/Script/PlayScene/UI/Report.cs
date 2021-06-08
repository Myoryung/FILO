using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

public class Report {
    private int REWARD_MAIN_GOAL, REWARD_SUB_GOAL;
    private int REWARD_SURVIVOR_RESCUE, REWARD_SURVIVOR_DEAD, REWARD_TOOL_USE;

    private readonly Sprite RANK_BAR_WHITE = Resources.Load<Sprite>("Sprite/PlayScene/Report_UI/RankBar_White");
    private readonly Sprite RANK_BAR_RED = Resources.Load<Sprite>("Sprite/PlayScene/Report_UI/RankBar_Red");
    private GameObject rankObj;
    private Text rankText;

    private readonly GameObject GOAL_TEXT_PREFAB = Resources.Load<GameObject>("Prefabs/UI/Report/GoalText");
    private GameObject mainGoalsObj, subGoalsObj;

    private readonly GameObject REWARD_ITEM_PREFAB = Resources.Load<GameObject>("Prefabs/UI/Report/RewardItem");
    private GameObject rewardsObj;
    private int rewardItemCount = 0;
    private int totalReward = 0;

    public Report(GameObject reportObj, List<Goal> mainGoals, List<Goal> subGoals, GameInfo info) {
        // Load XML
        TextAsset textAsset = (TextAsset)Resources.Load("Reward");
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(textAsset.text);

        XmlNodeList stageRewards = doc.SelectNodes("Rewards/StageRewards/Stage");
        foreach (XmlNode stageReward in stageRewards) {
            int number = int.Parse(stageReward.SelectSingleNode("Number").InnerText);
            if (number == info.StageNumber) {
                REWARD_MAIN_GOAL = int.Parse(stageReward.SelectSingleNode("MainGoal").InnerText);
                REWARD_SUB_GOAL = int.Parse(stageReward.SelectSingleNode("SubGoal").InnerText);
                break;
            }
        }

        REWARD_SURVIVOR_RESCUE = int.Parse(doc.SelectSingleNode("Rewards/CommonRewards/SurvivorRescue").InnerText);
        REWARD_SURVIVOR_DEAD = int.Parse(doc.SelectSingleNode("Rewards/CommonRewards/SurvivorDead").InnerText);
        REWARD_TOOL_USE = int.Parse(doc.SelectSingleNode("Rewards/CommonRewards/ToolUse").InnerText);

        // Load Object
        Transform reportTF = reportObj.transform;
        rankObj = reportTF.Find("Rank").gameObject;
        rankText = rankObj.transform.Find("RankText").GetComponent<Text>();

        mainGoalsObj = reportTF.Find("MainGoals").gameObject;
        subGoalsObj = reportTF.Find("SubGoals").gameObject;

        rewardsObj = reportTF.Find("Rewards").gameObject;

        // Rank Bar
        int rank = GetRank(info);
        Transform rankBars = rankObj.transform.Find("RankBars");

        for (int i = 0; i < rank; i++)
            rankBars.GetChild(i).GetComponent<Image>().sprite = RANK_BAR_RED;
        for (int i = rank; i < rankBars.childCount; i++)
            rankBars.GetChild(i).GetComponent<Image>().sprite = RANK_BAR_WHITE;

        // Rank Text
        char rankChar = GetRankChar(rank);
        rankText.text = string.Format("{0} rank", rankChar);


        // Create Main Goal Text
        for (int i = 0; i < mainGoals.Count; i++) {
            GameObject goalTextObj = GameObject.Instantiate(GOAL_TEXT_PREFAB, mainGoalsObj.transform);
            goalTextObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(175, -80*i);
            goalTextObj.GetComponent<Text>().text = mainGoals[i].GetExplanationText();
        }

        // Create Sub Goal Text
        for (int i = 0; i < subGoals.Count; i++) {
            GameObject goalTextObj = GameObject.Instantiate(GOAL_TEXT_PREFAB, subGoalsObj.transform);
            goalTextObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(175, -80*i);
            goalTextObj.GetComponent<Text>().text = subGoals[i].GetExplanationText();
        }

        // Set SubGoals Object
        if (subGoals.Count == 0)
            subGoalsObj.SetActive(false);
        else
            subGoalsObj.transform.localPosition = mainGoalsObj.transform.localPosition + new Vector3(0, -80*mainGoals.Count);

        // Create Reward Item
        for (int i = 0; i < mainGoals.Count; i++) {
            string name = mainGoals[i].GetExplanationText();
            string reward = string.Format("{0}$", REWARD_MAIN_GOAL);

            totalReward += REWARD_MAIN_GOAL;
            AddRewardItem(name, reward);
        }

        for (int i = 0; i < subGoals.Count; i++) {
            string name = subGoals[i].GetExplanationText();
            string reward = "0$";

            if (subGoals[i].IsSatisfied()) {
                reward = string.Format("{0}$", REWARD_SUB_GOAL);
                totalReward += REWARD_SUB_GOAL;
            }

            AddRewardItem(name, reward);
        }

        totalReward += info.RescuedSurvivorNum*REWARD_SURVIVOR_RESCUE;
        AddRewardItem("구출 생존자", string.Format("{0}×{1}$", info.RescuedSurvivorNum, REWARD_SURVIVOR_RESCUE));

        totalReward += info.UnrescuedSurvivorNum*REWARD_SURVIVOR_DEAD;
        AddRewardItem("구조하지 못한 생존자", string.Format("{0}×{1}$", info.UnrescuedSurvivorNum, REWARD_SURVIVOR_DEAD));

        totalReward += info.ToolUseCount*REWARD_TOOL_USE;
        AddRewardItem("장비 사용", string.Format("{0}×{1}$", info.ToolUseCount, REWARD_TOOL_USE));

        AddRewardItem("합계", string.Format("{0}$", totalReward));
    }

    private int GetRank(GameInfo info) {
        float rescueRate = info.RescuedSurvivorNum / info.SurvivorNum;
        if (rescueRate >= 1) {
            if (info.Deadline - info.CurrTime >= 20)
                return 5;
            else if (info.Deadline - info.CurrTime >= 10)
                return 4;
            else
                return 3;
        }
        else if (rescueRate >= 0.5)
            return 2;
        else
            return 1;
    }
    private char GetRankChar(int rank) {
        switch (rank) {
        case 5: return 'S';
        case 4: return 'A';
        case 3: return 'B';
        case 2: return 'C';
        case 1: return 'D';
        }
        return 'D';
    }
    private void AddRewardItem(string name, string reward) {
        GameObject rewardItemObj = GameObject.Instantiate(REWARD_ITEM_PREFAB, rewardsObj.transform);
        rewardItemObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -50*rewardItemCount++);

        rewardItemObj.transform.Find("Name").GetComponent<Text>().text = name;
        rewardItemObj.transform.Find("Reward").GetComponent<Text>().text = reward;
    }

    public int Reward {
        get { return totalReward; }
    }
}
