using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class GameData {
    [Serializable]
    private class Data {
        public int money;
        public List<bool> IsStageCleared;
        public List<char> StageRanks;

        public Data() {
            money = 0;
        }
    }

    private const string FILE_PATH = "Assets/GameData.ini";
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

    public int Money {
        get { return data.money; }
        set { data.money = value; }
    }
}
