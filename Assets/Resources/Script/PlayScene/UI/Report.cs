using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

public class Report {
    private int REWARD_MAIN_GOAL, REWARD_SUB_GOAL;
    private int REWARD_SURVIVOR_RESCUE, REWARD_SURVIVOR_DEAD, REWARD_TOOL_USE;

    private const string RANK_SPRITE_PATH = "Sprite/Report_UI/Rank/";
    private Image rankImage;

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
        rankImage = reportTF.Find("Rank").Find("RankChar").GetComponent<Image>();

        mainGoalsObj = reportTF.Find("MainGoals").gameObject;
        subGoalsObj = reportTF.Find("SubGoals").gameObject;

        rewardsObj = reportTF.Find("Rewards").gameObject;

        // Set Rank
        char rank = GetRank(info);
        rankImage.sprite = Resources.Load<Sprite>(RANK_SPRITE_PATH + rank);

        // Create Main Goal Text
        for (int i = 0; i < mainGoals.Count; i++) {
            GameObject goalTextObj = GameObject.Instantiate(GOAL_TEXT_PREFAB, mainGoalsObj.transform);
            goalTextObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -10*(i+1) + -40*i);

            string text = string.Format("{0}. {1}", i+1, mainGoals[i].GetExplanationText());
            goalTextObj.GetComponent<Text>().text = text;
        }

        // Create Sub Goal Text
        for (int i = 0; i < subGoals.Count; i++) {
            GameObject goalTextObj = GameObject.Instantiate(GOAL_TEXT_PREFAB, subGoalsObj.transform);
            goalTextObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -10*(i+1) + -40*i);

            string text = string.Format("{0}. {1}", i+1, subGoals[i].GetExplanationText());
            goalTextObj.GetComponent<Text>().text = text;
        }

        // Set SubGoals Object
        if (subGoals.Count == 0)
            subGoalsObj.SetActive(false);
        else
            subGoalsObj.transform.localPosition = mainGoalsObj.transform.localPosition + new Vector3(0, -50*(mainGoals.Count+1));

        // Create Reward Item
        for (int i = 0; i < mainGoals.Count; i++) {
            string name = string.Format("메인 목표 [{0}]", i+1);
            string reward = string.Format("{0}$", REWARD_MAIN_GOAL);

            totalReward += REWARD_MAIN_GOAL;
            AddRewardItem(name, reward);
        }

        for (int i = 0; i < subGoals.Count; i++) {
            string name = string.Format("서브 목표 [{0}]", i+1);
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

    private char GetRank(GameInfo info) {
        float rescueRate = info.RescuedSurvivorNum / info.SurvivorNum;
        if (rescueRate >= 1) {
            if (info.Deadline - info.CurrTime >= 20)
                return 'S';
            else if (info.Deadline - info.CurrTime >= 10)
                return 'A';
            else
                return 'B';
        }
        else if (rescueRate >= 0.5)
            return 'C';
        else
            return 'D';
    }
    private void AddRewardItem(string name, string reward) {
        GameObject rewardItemObj = GameObject.Instantiate(REWARD_ITEM_PREFAB, rewardsObj.transform);
        rewardItemObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -40*rewardItemCount++);

        rewardItemObj.transform.Find("Name").GetComponent<Text>().text = name;
        rewardItemObj.transform.Find("Reward").GetComponent<Text>().text = reward;
    }
}
