using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;

public class TalkMgr
{
    private static TalkMgr m_instance;

    public static TalkMgr Instance
    {
        get { return m_instance; }
    }
    public static void CreateInstance(int stage)
    {
        if (m_instance != null)
        {
            Debug.Log("Destroy TalkMgr");
            m_instance = null;
        }
        m_instance = new TalkMgr(stage);
    }
    private GameObject talkUI;
    private Image leftPanel;
    private Image rightPanel;
    private Image cutScenePopPanel;
    private Text speakerText;
    private Text contentText;

    private string leftTalker;
    private string rightTalker;
    private string lastestTalker;
    private Dictionary<string, Sprite> talkerPanel = new Dictionary<string, Sprite>();
    private Dictionary<int, List<Message>> talklist = new Dictionary<int, List<Message>>();
    private TalkMgr(int stage)
    {
        //XML 생성
        XmlDocument doc = new XmlDocument();
        TextAsset textAsset = (TextAsset)Resources.Load("Data/TalkData" + stage);
        doc.LoadXml(textAsset.text);

        //대화 데이터 불러오기
        XmlNodeList talks = doc.SelectNodes("TalkData/Talk");
        foreach(XmlNode talk in talks)
        {
            XmlNodeList datas = talk.SelectNodes("Message");
            List<Message> messages = new List<Message>();
            int id = int.Parse(talk.SelectSingleNode("ID").InnerText);
            talklist.Add(id, messages);
            for(int i=0;i<datas.Count; i++)
            {
                switch (datas[i].SelectSingleNode("Type").InnerText)
                {
                    case "Message":
                        string speaker = datas[i].SelectSingleNode("Speaker").InnerText;
                        string content = datas[i].SelectSingleNode("Content").InnerText;
                        string look = datas[i].SelectSingleNode("Look").InnerText;
                        Message.LookType looktype = Message.LookType.None;
                        switch (look)
                        {
                            case "Normal":
                                looktype = Message.LookType.Normal;
                                break;
                            case "Angry":
                                looktype = Message.LookType.Angry;
                                break;
                            case "Sad":
                                looktype = Message.LookType.Sad;
                                break;
                            default:
                                looktype = Message.LookType.None;
                                break;
                        }
                        Message message = new Message(speaker, content, looktype);
                        talklist[id].Add(message);
                        break;
                    case "CutScene":
                        string imageName = datas[i].SelectSingleNode("ImageName").InnerText;
                        Message cutScene = new Message(imageName);
                        talklist[id].Add(cutScene);
                        break;
                    default:
                        break;
                        
                }
            }
        }

        //대화 UI 오브젝트 찾기
        talkUI = GameObject.Find("UICanvas/TalkCanvas");
        if (talkUI)
        {
            leftPanel = talkUI.transform.Find("LeftPanel").GetComponent<Image>();
            rightPanel = talkUI.transform.Find("RightPanel").GetComponent<Image>();
            cutScenePopPanel = talkUI.transform.Find("CutScenePopPanel").GetComponent<Image>();
            speakerText = talkUI.transform.Find("SpeakerText").GetComponent<Text>();
            contentText = talkUI.transform.Find("ContentText").GetComponent<Text>();
        }
        //대화에 사용될 일러스트 할당
        Sprite captainBasic = Resources.Load<Sprite>("Sprite/Ilust/Captain/Captain_basic");
        Sprite hammerBasic = Resources.Load<Sprite>("Sprite/Ilust/HammerMan/Hammerman_basic");
        talkerPanel.Add("신화준", captainBasic);
        talkerPanel.Add("빅토르", hammerBasic);
    }

    public IEnumerator StartTalk(int ID)
    {
        talkUI.GetComponent<Canvas>().enabled = true;
        Debug.Log(talklist[ID].Count);
        for(int i=0; i<talklist[ID].Count; i++)
        {
            switch(talklist[ID][i].DataType)
            {
                case Message.Type.Message:
                    ChangeTalkPanel(talklist[ID][i].GetSpeaker());
                    talklist[ID][i].ShowMessage(speakerText, contentText);
                    break;
                case Message.Type.CutScene:
                    talklist[ID][i].ShowCutScene(cutScenePopPanel);
                    if (leftPanel.sprite != null)
                        leftPanel.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
                    if (rightPanel.sprite != null)
                        rightPanel.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
                    cutScenePopPanel.color = new Color(1, 1, 1, 1);
                    break;
            }
            yield return new WaitUntil(() => Input.GetKeyUp(KeyCode.Space));
            yield return new WaitForSeconds(1.0f);
        }
        talkUI.GetComponent<Canvas>().enabled = false;
    }

    private void ChangeTalkPanel(string Speaker)
    {
        if(!talkerPanel.ContainsKey(Speaker))
        {
            if (leftPanel.sprite != null)
                leftPanel.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
            if (rightPanel.sprite != null)
                rightPanel.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
            lastestTalker = Speaker;
            return;
        }
        if(leftPanel.sprite == null)
        {
            leftPanel.sprite = talkerPanel[Speaker];
            leftPanel.color = new Color(1, 1, 1, 1);
            if (rightPanel.sprite != null)
                rightPanel.color = new Color(0.5f, 0.5f, 0.5f, 1);
            leftTalker = Speaker;
        }
        else if(rightPanel.sprite == null)
        {
            rightPanel.sprite = talkerPanel[Speaker];
            rightPanel.color = new Color(1, 1, 1, 1);
            if (leftPanel.sprite != null)
                leftPanel.color = new Color(0.5f, 0.5f, 0.5f, 1);
            rightTalker = Speaker;
        }
        else if(leftTalker != Speaker && rightTalker != Speaker)
        {
            if(leftTalker == lastestTalker)
            {
                rightPanel.sprite = talkerPanel[Speaker];
                rightPanel.color = new Color(1, 1, 1, 1);
                leftPanel.color = new Color(0.5f, 0.5f, 0.5f, 1);
                rightTalker = Speaker;
            }
            else if(rightTalker == lastestTalker)
            {
                leftPanel.sprite = talkerPanel[Speaker];
                leftPanel.color = new Color(1, 1, 1, 1);
                rightPanel.color = new Color(0.5f, 0.5f, 0.5f, 1);
                leftTalker = Speaker;
            }
        }
        else
        {
            return;
        }
        lastestTalker = Speaker;
    }
}
