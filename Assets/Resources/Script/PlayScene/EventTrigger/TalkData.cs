using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;

public class Message
{
    public enum Type { Message, CutScene }
    public Type DataType;

    public enum LookType { Normal, Angry, Sad, Surprise, Happy, None }
    private LookType look;
    private string speaker;
    private string content;

    private Sprite image;

    public Message(string imageName)
    {
        DataType = Type.CutScene;
        image = Resources.Load<Sprite>("Sprite/PlayScene/CutScene/" + imageName) as Sprite;
    }

    public Message(string Speaker, string Content, LookType Look)
    {
        DataType = Type.Message;
        this.speaker = Speaker;
        this.content = Content;
        this.look = Look;
    }

    public void ShowCutScene(Image PopPanel)
    {
        if (DataType != Type.CutScene)
            return;
        PopPanel.sprite = image;
    }

    public void ShowMessage(Text Speaker, Text Content)
    {
        if (DataType != Type.Message)
            return;
        Speaker.text = this.speaker;
        Content.text = this.content;
    }

    public LookType GetLook() { return look; }
    public string GetSpeaker() { return speaker; }
}