using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class GameData {
    [Serializable]
    private class Data {
        public static readonly int LATEST_VERSION = 2;
        public int version = LATEST_VERSION;

        public int money = 0;
        public List<char> stageRanks = new List<char>();
        public Tool[,] tools = new Tool[4,2];
        public int[,] abilityIndices = new int[4,4];

        public int stageNumber;

        public Data() {
            // 기본 도구 설정
            for (int i = 0; i < 4; i++) {
                tools[i, 0] = Tool.FIRE_EX;
                tools[i, 1] = Tool.O2_CAN;
            }
            tools[1, 0] = Tool.FLARE; // 해머맨은 소화기 대신 조명탄 사용

            // 특성 기본값 설정 (-1: 미선택, 0: 첫번째 특성, 1: 두번째 특성)
            for (int i = 0; i < 4; i++) {
                for (int j = 0; j < 4; j++)
                    abilityIndices[i, j] = -1;
            }
        }
    }


    private const string FILE_PATH = "Assets/GameData.ini";
    private const char NOT_CLEAR_RANK = 'N';
    private Data data = null;


    public GameData() {
        try {
            Load();
        } catch (Exception except) {
            Debug.Log(except.Message);
        }

        if (data == null)
            data = new Data();
    }
    public void Load() {
        try { 
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(FILE_PATH, FileMode.Open);

            data = formatter.Deserialize(stream) as Data;
            if (data.version != Data.LATEST_VERSION) {
                Debug.Log("이전 버전의 세이브 파일입니다.");
                data = null;
            }

            stream.Close();
        } catch (Exception e) {
            Debug.Log(e.Message);
            data = null;
        }
    }
    public void Save() {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream;
        
        if (File.Exists(FILE_PATH))
            stream = new FileStream(FILE_PATH, FileMode.Create);
        else
            stream = new FileStream(FILE_PATH, FileMode.Create);

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public void SetRank(int stage, char rank) {
        while (data.stageRanks.Count <= stage)
            data.stageRanks.Add(NOT_CLEAR_RANK);

        data.stageRanks[stage] = rank;
    }
    public char GetRank(int stage) {
        if (data.stageRanks.Count <= stage)
            return NOT_CLEAR_RANK;
        return data.stageRanks[stage];
    }

    public bool IsCleared(int stage) {
        return GetRank(stage) != NOT_CLEAR_RANK;
    }

    public Tool GetTool(int operatorNumber, int index) {
        return data.tools[operatorNumber, index];
    }
    public void SetTool(int operatorNumber, int index, Tool tool) {
        data.tools[operatorNumber, index] = tool;
    }
    public int GetAbilityIndex(int operatorNumber, int level) {
        return data.abilityIndices[operatorNumber, level];
    }
    public void SetAbilityIndex(int operatorNumber, int level, int index) {
        data.abilityIndices[operatorNumber, level] = index;
    }

    public int Money {
        get { return data.money; }
        set { data.money = value; }
    }
    public void SetStageNumber(int value) { data.stageNumber = value; }
    public int GetStageNumber() { return data.stageNumber; }
}
