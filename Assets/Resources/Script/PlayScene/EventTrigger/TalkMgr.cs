using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;

public class TalkMgr
{
    public enum SpecialTalkTrigger { GameStart, SelectDone, GoalSuccess, Rescue, Interact }
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
    private Dictionary<int, bool> isTalkUsed = new Dictionary<int, bool>();
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
            isTalkUsed.Add(id, false);
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
                            case "Surprise":
                                looktype = Message.LookType.Surprise;
                                break;
                            case "Happy":
                                looktype = Message.LookType.Happy;
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
    }

    public IEnumerator StartTalk(int ID)
    {
        if (!isTalkUsed[ID])
        {
            talkUI.GetComponent<Canvas>().enabled = true;
            Debug.Log(talklist[ID].Count);
            for (int i = 0; i < talklist[ID].Count; i++)
            {
                switch (talklist[ID][i].DataType)
                {
                    case Message.Type.Message:
                        ChangeTalkPanel(talklist[ID][i].GetSpeaker(), talklist[ID][i].GetLook());
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
                yield return new WaitForSeconds(0.15f);
            }
            talkUI.GetComponent<Canvas>().enabled = false;
            isTalkUsed[ID] = true;
            ClearPanel();
        }
    }

    private void ChangeTalkPanel(string Speaker, Message.LookType look)
    {
        if(leftPanel.sprite == null)
        {
            leftPanel.sprite = GetPanelLook(Speaker, look);
            leftPanel.color = new Color(1, 1, 1, 1);
            if (rightPanel.sprite != null)
                rightPanel.color = new Color(0.5f, 0.5f, 0.5f, 1);
            leftTalker = Speaker;
        }
        else if(leftPanel.sprite != null && leftTalker == Speaker)
        {
            leftPanel.sprite = GetPanelLook(Speaker, look);
            leftPanel.color = new Color(1, 1, 1, 1);
            if (rightPanel.sprite != null)
                rightPanel.color = new Color(0.5f, 0.5f, 0.5f, 1);
        }
        else if(rightPanel.sprite == null && lastestTalker != Speaker)
        {
            rightPanel.sprite = GetPanelLook(Speaker, look);
            rightPanel.color = new Color(1, 1, 1, 1);
            if (leftPanel.sprite != null)
                leftPanel.color = new Color(0.5f, 0.5f, 0.5f, 1);
            rightTalker = Speaker;
        }
        else if(rightPanel.sprite != null && rightTalker == Speaker)
        {
            rightPanel.sprite = GetPanelLook(Speaker, look);
            rightPanel.color = new Color(1, 1, 1, 1);
            if (leftPanel.sprite != null)
                leftPanel.color = new Color(0.5f, 0.5f, 0.5f, 1);
        }
        else if(leftTalker != Speaker && rightTalker != Speaker)
        {
            if(leftTalker == lastestTalker)
            {
                rightPanel.sprite = GetPanelLook(Speaker, look);
                rightPanel.color = new Color(1, 1, 1, 1);
                leftPanel.color = new Color(0.5f, 0.5f, 0.5f, 1);
                rightTalker = Speaker;
            }
            else if(rightTalker == lastestTalker)
            {
                leftPanel.sprite = GetPanelLook(Speaker, look);
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

    Sprite GetPanelLook(string Speaker, Message.LookType look)
    {
        Debug.Log(Speaker);
        Debug.Log(look);
        switch(Speaker)
        {
            case "신화준":
                switch (look)
                {
                    case Message.LookType.Normal:
                        return Resources.Load<Sprite>("Sprite/Ilust/Captain/Captain_basic");
                    case Message.LookType.Sad:
                        return Resources.Load<Sprite>("Sprite/Ilust/Captain/Captain_sad");
                    case Message.LookType.Angry:
                        return Resources.Load<Sprite>("Sprite/Ilust/Captain/Captain_anger");
                    case Message.LookType.Surprise:
                        return Resources.Load<Sprite>("Sprite/Ilust/Captain/Captain_surprised");
                    case Message.LookType.Happy:
                        return Resources.Load<Sprite>("Sprite/Ilust/Captain/Captain_joy");
                    case Message.LookType.None:
                        return null;
                }
                break;
            case "빅토르":
                switch (look)
                {
                    case Message.LookType.Normal:
                        return Resources.Load<Sprite>("Sprite/Ilust/HammerMan/Hammerman_basic");
                    case Message.LookType.Sad:
                        return Resources.Load<Sprite>("Sprite/Ilust/HammerMan/Hammerman_sadness");
                    case Message.LookType.Angry:
                        return Resources.Load<Sprite>("Sprite/Ilust/HammerMan/Hammerman_anger");
                    case Message.LookType.Surprise:
                        return Resources.Load<Sprite>("Sprite/Ilust/HammerMan/Hammerman_surprised");
                    case Message.LookType.Happy:
                        return Resources.Load<Sprite>("Sprite/Ilust/HammerMan/Hammerman_joy");
                    case Message.LookType.None:
                        return null;
                }
                break;
            case "레오":
                switch (look)
                {
                    case Message.LookType.Normal:
                        return Resources.Load<Sprite>("Sprite/Ilust/Rescuer/Rescueman_basic2");
                    case Message.LookType.Sad:
                        return Resources.Load<Sprite>("Sprite/Ilust/Rescuer/Rescueman_sadness2");
                    case Message.LookType.Angry:
                        return Resources.Load<Sprite>("Sprite/Ilust/Rescuer/Rescueman_anger2");
                    case Message.LookType.Surprise:
                        return Resources.Load<Sprite>("Sprite/Ilust/Rescuer/Rescueman_surprised2");
                    case Message.LookType.Happy:
                        return Resources.Load<Sprite>("Sprite/Ilust/Rescuer/Rescueman_joy2");
                    case Message.LookType.None:
                        return null;
                }
                break;
            case "시노에":
                switch (look)
                {
                    case Message.LookType.Normal:
                        return Resources.Load<Sprite>("Sprite/Ilust/Nurse/Nurse_1_basic");
                    case Message.LookType.Sad:
                        return Resources.Load<Sprite>("Sprite/Ilust/Nurse/Nurse_1_sadness");
                    case Message.LookType.Angry:
                        return Resources.Load<Sprite>("Sprite/Ilust/Nurse/Nurse_1_anger");
                    case Message.LookType.Surprise:
                        return Resources.Load<Sprite>("Sprite/Ilust/Nurse/Nurse_1_surprised");
                    case Message.LookType.Happy:
                        return Resources.Load<Sprite>("Sprite/Ilust/Nurse/Nurse_joy");
                    case Message.LookType.None:
                        return null;
                }
                break;
            case "상황실":
                switch (look)
                {
                    case Message.LookType.Normal:
                        return Resources.Load<Sprite>("Sprite/Ilust/Senior/Senior_basic");
                    case Message.LookType.None:
                        return null;
                }
                break;
        }
        return null;
    }

    void ClearPanel()
    {
        leftPanel.sprite = null;
        leftPanel.color = new Color(1, 1, 1, 0);
        rightPanel.sprite = null;
        rightPanel.color = new Color(1, 1, 1, 0);
        cutScenePopPanel.color = new Color(1, 1, 1, 0);
        leftTalker = null;
        rightTalker = null;
        lastestTalker = null;
    }

    public void SpecialBehaviorTrigger(int stage, SpecialTalkTrigger trigger)
    {
        switch(stage)
        {
            case 0:
                if(trigger == SpecialTalkTrigger.SelectDone)
                    GameMgr.Instance.StartTalk(1);
                break;
            case 1:
                switch(trigger)
                {
                    case SpecialTalkTrigger.GameStart:
                        GameMgr.Instance.StartTalk(1);
                        break;
                    case SpecialTalkTrigger.Rescue:
                        GameMgr.Instance.StartTalk(4);
                        break;
                    case SpecialTalkTrigger.Interact:
                        GameMgr.Instance.StartTalk(7);
                        break;
                    case SpecialTalkTrigger.GoalSuccess:
                        GameMgr.Instance.StartTalk(8);
                        break;
                }
                break;
        }
    }
}