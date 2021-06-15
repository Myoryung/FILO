using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class GameData {
    [Serializable]
    private class Data {
        public int money = 0;
        public List<char> stageRanks = new List<char>();
    }

    private const string FILE_PATH = "Assets/GameData.ini";
    private const char NOT_CLEAR_RANK = 'N';
    private Data data = null;


    public GameData() {
        Load();

        if (data == null)
            data = new Data();
    }
    public void Load() {
        try { 
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(FILE_PATH, FileMode.Open);

            data = formatter.Deserialize(stream) as Data;

            stream.Close();
        } catch (Exception e) {
            Debug.Log(e.Message);
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

    public int Money {
        get { return data.money; }
        set { data.money = value; }
    }
}
