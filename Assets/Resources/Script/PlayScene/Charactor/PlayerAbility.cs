using UnityEngine;
using UnityEngine.UI;
using System;
using System.Xml;

public enum Ability {
    None,
    Cardio, DoubleHeart, EquipMini, Fitness, IntervalTrain, 
    MacGyver, Nightingale, OxygenTank, PartsLightweight, RedundancyOxygen,
    Refuel, ReinforceParts, SteelBody, SurvivalPriority
};

class PlayerAbilityMgr {
    private static Ability[,,] abilities = null;

    public static void AddAbility(Player player, Ability ability) {
        // TODO: 특성 적용
    }


    public static Ability GetAbility(int operatorNumber, int level, int index) {
        if (abilities == null) LoadInfo();

        return abilities[operatorNumber, level, index];
    }
    private static void LoadInfo() {
        abilities = new Ability[4, 4, 2];

        XmlDocument doc = new XmlDocument();
        TextAsset textAsset = (TextAsset)Resources.Load("Ability");
        doc.LoadXml(textAsset.text);

        XmlNodeList operatorList = doc.SelectNodes("Abilities/Operator");
        for (int i = 0; i < 4; i++) {
            XmlNodeList levelList = operatorList[i].SelectNodes("Level");
            for (int j = 0; j < 2; j++) {
                XmlNodeList abilityList = levelList[j].SelectNodes("Ability");
                for (int k = 0; k < 2; k++)
                    abilities[i, j, k] = ToAbility(abilityList[k].InnerText.Trim());
            }
        }
    }

    public static string ToString(Ability ability) {
        switch (ability) {
        case Ability.None:              return "특성 미선택";
        case Ability.Cardio:            return "유산소 훈련";
        case Ability.DoubleHeart:       return "두개의 심장";
        case Ability.EquipMini:         return "장비 소형화";
        case Ability.Fitness:           return "체력 단련";
        case Ability.IntervalTrain:     return "인터벌 훈련";
        case Ability.MacGyver:          return "멕 가이버";
        case Ability.Nightingale:       return "나이팅 게일";
        case Ability.OxygenTank:        return "산소 탱크";
        case Ability.PartsLightweight:  return "파츠 경량화";
        case Ability.RedundancyOxygen:  return "여분 산소캔";
        case Ability.Refuel:            return "연료 보급";
        case Ability.ReinforceParts:    return "강화 파츠";
        case Ability.SteelBody:         return "강철의 육체";
        case Ability.SurvivalPriority:  return "생존우선순위";
        }

        return "";
    }
    public static string GetInfo(Ability ability) {
        switch (ability) {
        case Ability.Cardio:            return "체력 +5, 산소 +10";
        case Ability.DoubleHeart:       return "산소 회복량 +3";
        case Ability.EquipMini:         return "핵심 능력 산소 소비량 -5";
        case Ability.Fitness:           return "체력 +10";
        case Ability.IntervalTrain:     return "체력 +5, 산소 +5";
        case Ability.MacGyver:          return "도구 사용횟수 +1";
        case Ability.Nightingale:       return "궁극기 사용 횟수 -1\n핵심 능력 체력, 산소 회복량 +10";
        case Ability.OxygenTank:        return "체력 -5, 산소 +15";
        case Ability.PartsLightweight:  return "체력 -5, 산소 +15";
        case Ability.RedundancyOxygen:  return "산소 +10";
        case Ability.Refuel:            return "로봇 개 산소 +30";
        case Ability.ReinforceParts:    return "체력 +10, 산소 +5";
        case Ability.SteelBody:         return "체력 +30";
        case Ability.SurvivalPriority:  return "산소 회복량 +5\n핵심 능력 체력, 산소 회복량 -5";
        }

        return "";
    }
    public static Ability ToAbility(string name) {
        switch (name) {
        case "None":            return Ability.None;
        case "Cardio":          return Ability.Cardio;
        case "DoubleHeart":     return Ability.DoubleHeart;
        case "EquipMini":       return Ability.EquipMini;
        case "Fitness":         return Ability.Fitness;
        case "IntervalTrain":   return Ability.IntervalTrain;
        case "MacGyver":        return Ability.MacGyver;
        case "Nightingale":     return Ability.Nightingale;
        case "OxygenTank":      return Ability.OxygenTank;
        case "PartsLightweight":return Ability.PartsLightweight;
        case "RedundancyOxygen":return Ability.RedundancyOxygen;
        case "Refuel":          return Ability.Refuel;
        case "ReinforceParts":  return Ability.ReinforceParts;
        case "SteelBody":       return Ability.SteelBody;
        case "SurvivalPriority":return Ability.SurvivalPriority;
        }

        return Ability.None;
    }
}
