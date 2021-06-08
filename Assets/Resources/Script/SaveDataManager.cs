using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveDataManager : MonoBehaviour
{
    private static SaveDataManager instance;
    public static SaveDataManager Instance
    {
        get
        {
            if (!instance) return null;
            return instance;
        }
    }

    private int currentLevel = 0;

    private void Awake()
    {
        if(!instance)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void Save()
    {

    }

    public void Load()
    {
        
    }
}
