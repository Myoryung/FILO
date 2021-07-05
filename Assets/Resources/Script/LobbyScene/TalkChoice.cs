using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using UnityEngine.UI;

public class TalkChoice : MonoBehaviour
{
    [SerializeField]
    private int talkerIndex = 0;
    [SerializeField]
    private int Index = 0;
    public Text Question;
    public Text FirstAnswer;
    public Text SecondAnswer;
    public Text ThirdAnswer;
    public Text Result;
    private Animator _anim;
    private string[] resultTexts;

    private void Start()
    {
        XmlDocument doc = new XmlDocument();
        TextAsset textAsset = (TextAsset)Resources.Load("Data/EagisTalkData");
        doc.LoadXml(textAsset.text);
        XmlNodeList talks = doc.SelectNodes("EagisTalkData/Message");

        foreach(XmlNode talk in talks)
        {
            if (talk.SelectSingleNode("TalkerIndex").InnerText.Equals(talkerIndex.ToString()))
            {
                if(talk.SelectSingleNode("Index").InnerText.Equals(Index.ToString()))
                {
                    Question.text = talk.SelectSingleNode("Question").InnerText;
                    FirstAnswer.text = talk.SelectSingleNode("FirstAnswer").InnerText;
                    SecondAnswer.text = talk.SelectSingleNode("SecondAnswer").InnerText;
                    ThirdAnswer.text = talk.SelectSingleNode("ThirdAnswer").InnerText;
                    resultTexts = new string[3];
                    resultTexts[0] = talk.SelectSingleNode("FirstResult").InnerText;
                    resultTexts[1] = talk.SelectSingleNode("SecondResult").InnerText;
                    resultTexts[2] = talk.SelectSingleNode("ThirdResult").InnerText;
                }
            }
        }
        _anim = GetComponent<Animator>();
    }

    public void ChoiceBtn(string trigger)
    {
        _anim.SetTrigger(trigger);
        int resultIndex = 0;
        switch(trigger)
        {
            case "FirstAnswer":
                resultIndex = 0;
                break;
            case "SecondAnswer":
                resultIndex = 1;
                break;
            case "ThirdAnswer":
                resultIndex = 2;
                break;
        }
        Result.text = resultTexts[resultIndex];
    }

    public void EnableTalkChoice()
    {

    }
}
