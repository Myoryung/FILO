using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyMgr : MonoBehaviour
{
    public GameObject UICanvas;

    public Text newsContents;

    private GameObject argosSystem;
    private GameObject athenaSystem;
    private GameObject eagisSystem;
    private GameObject futureTechSystem;

    private Image argosBtn;
    private Image athenaBtn;
    private Image messengerBtn;
    private Image techBtn;

    private GameObject debriefing;
    private GameObject operatorInfos;
    private GameObject operatorLeftInfo;
    private GameObject operatorRightInfo;
    private GameObject selectToolUI;
    private GameObject selectAbilUI;
    private GameObject banners;

    private Image operatorProfileImage;

    private Text operatorNameText;
    private Text operatorBodyText;
    private Text operatorSpecialText;

    private Text hpText;
    private Text o2Text;
    private Text useO2Text;
    private Text recorveryO2Text;

    private int currentOperatorNum = 0;

    private GameObject techDevUI;
    private GameObject techTestUI;

    private int currentSystemIndex = 0;
    private GameObject currentSystem;
    private Image currentSystemBtn;

    private Sprite[] btnImages;
    private Sprite[] profileImages;

    private Dictionary<Tool, Sprite> toolSprites;
    private readonly Color CHAING_COLOR = Color.white, NON_SELECTED_COLOR = new Color(217/255f, 54/255f, 21/255f);
    private Image[] info_toolImages;
    private Image[] select_toolImages;
    private Text[] select_toolTexts;
    private int selectToolIndex = 0;

    private GameData gameData = new GameData();

    private void Start()
    {
        argosSystem = UICanvas.transform.Find("ArgosSystem").gameObject;
        athenaSystem = UICanvas.transform.Find("AthenaSystem").gameObject;
        eagisSystem = UICanvas.transform.Find("EagisSystem").gameObject;
        futureTechSystem = UICanvas.transform.Find("FutureTechSystem").gameObject;

        Transform Btns = UICanvas.transform.Find("Btns");

        argosBtn = Btns.Find("ArgosBtn").GetComponent<Image>();
        athenaBtn = Btns.Find("AthenaBtn").GetComponent<Image>();
        messengerBtn = Btns.Find("MessengerBtn").GetComponent<Image>();
        techBtn = Btns.Find("TechBtn").GetComponent<Image>();

        debriefing = athenaSystem.transform.Find("Debriefing").gameObject;
        operatorInfos = athenaSystem.transform.Find("OperatorInfo").gameObject;
        operatorLeftInfo = operatorInfos.transform.Find("LeftInfo").gameObject;
        operatorRightInfo = operatorInfos.transform.Find("RightInfo").gameObject;
        banners = athenaSystem.transform.Find("SelectBanner").gameObject;

        operatorProfileImage = operatorLeftInfo.transform.Find("ProfileImage").GetComponent<Image>();
        operatorNameText = operatorLeftInfo.transform.Find("OperatorName").GetComponent<Text>();
        operatorBodyText = operatorLeftInfo.transform.Find("OperatorBody").GetComponent<Text>();
        operatorSpecialText = operatorLeftInfo.transform.Find("OperatorSpecial").GetComponent<Text>();

        hpText = operatorRightInfo.transform.Find("HP").GetComponent<Text>();
        o2Text = operatorRightInfo.transform.Find("O2").GetComponent<Text>();
        useO2Text = operatorRightInfo.transform.Find("UseO2").GetComponent<Text>();
        recorveryO2Text = operatorRightInfo.transform.Find("RecorveryO2").GetComponent<Text>();

        selectToolUI = operatorInfos.transform.Find("SelectTool").gameObject;
        selectAbilUI = operatorInfos.transform.Find("SelectAbility").gameObject;

        techDevUI = futureTechSystem.transform.Find("TechDev").gameObject;
        techTestUI = futureTechSystem.transform.Find("TechTest").gameObject;


        info_toolImages = new Image[2] {
            operatorRightInfo.transform.Find("Tool0").Find("Tool").GetComponent<Image>(),
            operatorRightInfo.transform.Find("Tool1").Find("Tool").GetComponent<Image>()
        };
        select_toolImages = new Image[2] {
            selectToolUI.transform.Find("ToolImage0").Find("Tool").GetComponent<Image>(),
            selectToolUI.transform.Find("ToolImage1").Find("Tool").GetComponent<Image>()
        };
        select_toolTexts = new Text[2] {
            selectToolUI.transform.Find("ToolText0").GetComponent<Text>(),
            selectToolUI.transform.Find("ToolText1").GetComponent<Text>()
        };
        for (int i = 0; i < 2; i++)
            select_toolTexts[i].color = NON_SELECTED_COLOR;

        btnImages = new Sprite[12];

        btnImages[0] = Resources.Load<Sprite>("Sprite/LobbyScene/Icon/01.Argos_Non select");
        btnImages[1] = Resources.Load<Sprite>("Sprite/LobbyScene/Icon/01.Argos_Non select_notice");
        btnImages[2] = Resources.Load<Sprite>("Sprite/LobbyScene/Icon/01.Argos_select");
        btnImages[3] = Resources.Load<Sprite>("Sprite/LobbyScene/Icon/02.Athena_Non select");
        btnImages[4] = Resources.Load<Sprite>("Sprite/LobbyScene/Icon/02.Athena_Non select_notice");
        btnImages[5] = Resources.Load<Sprite>("Sprite/LobbyScene/Icon/02.Athena_select");
        btnImages[6] = Resources.Load<Sprite>("Sprite/LobbyScene/Icon/03.Messenger_Non select");
        btnImages[7] = Resources.Load<Sprite>("Sprite/LobbyScene/Icon/03.Messenger_Non select_notice");
        btnImages[8] = Resources.Load<Sprite>("Sprite/LobbyScene/Icon/03.Messenger_select");
        btnImages[9] = Resources.Load<Sprite>("Sprite/LobbyScene/Icon/04.Technology_Non select");
        btnImages[10] = Resources.Load<Sprite>("Sprite/LobbyScene/Icon/04.Technology_Non select_notice");
        btnImages[11] = Resources.Load<Sprite>("Sprite/LobbyScene/Icon/04.Technology_select");

        profileImages = new Sprite[4];

        profileImages[0] = Resources.Load<Sprite>("Sprite/LobbyScene/02_Athena_System/02_Operator Information/Athena-02_Operator Information-profile-01");
        profileImages[1] = Resources.Load<Sprite>("Sprite/LobbyScene/02_Athena_System/02_Operator Information/Athena-02_Operator Information-profile-02");
        profileImages[2] = Resources.Load<Sprite>("Sprite/LobbyScene/02_Athena_System/02_Operator Information/Athena-02_Operator Information-profile-03");
        profileImages[3] = Resources.Load<Sprite>("Sprite/LobbyScene/02_Athena_System/02_Operator Information/Athena-02_Operator Information-profile-04");

        toolSprites = new Dictionary<Tool, Sprite>();
        toolSprites.Add(Tool.FIREWALL, Resources.Load<Sprite>("Sprite/LobbyScene/02_Athena_System/02_Operator Information/ToolIcon/Firewall1"));
        toolSprites.Add(Tool.FIRE_EX, Resources.Load<Sprite>("Sprite/LobbyScene/02_Athena_System/02_Operator Information/ToolIcon/FireExtinguisher1"));
        toolSprites.Add(Tool.FLARE, Resources.Load<Sprite>("Sprite/LobbyScene/02_Athena_System/02_Operator Information/ToolIcon/Flare1"));
        toolSprites.Add(Tool.O2_CAN, Resources.Load<Sprite>("Sprite/LobbyScene/02_Athena_System/02_Operator Information/ToolIcon/OxygenRespirator1"));
        toolSprites.Add(Tool.STICKY_BOMB, Resources.Load<Sprite>("Sprite/LobbyScene/02_Athena_System/02_Operator Information/ToolIcon/StickyBomb1"));

        currentSystemIndex = 0;
        currentSystem = argosSystem;
        currentSystemBtn = argosBtn;
    }

    private void Update()
    {
        NewsFlow();
    }

    void NewsFlow()
    {
        newsContents.rectTransform.Translate(-1.0f * Time.deltaTime * 15, 0, 0);
        if (newsContents.rectTransform.position.x < -250.0f)
            newsContents.rectTransform.position = new Vector3(1160.0f, 1053, 0);
    }

    public void SystemSelectBtn(int index)
    {
        if (currentSystemIndex == index)
            return;
        currentSystem.SetActive(false);
        switch(currentSystemIndex)
        {
            case 0:
                currentSystemBtn.sprite = btnImages[0];
                break;
            case 1:
                debriefing.SetActive(false);
                operatorInfos.SetActive(false);
                banners.SetActive(false);
                currentSystemBtn.sprite = btnImages[3];
                break;
            case 2:
                currentSystemBtn.sprite = btnImages[6];
                break;
            case 3:
                currentSystemBtn.sprite = btnImages[9];
                break;
        }
        switch (index)
        {
            case 0:
                currentSystem = argosSystem;
                currentSystemBtn = argosBtn;
                currentSystemBtn.sprite = btnImages[2];
                break;
            case 1:
                currentSystem = athenaSystem;
                currentSystemBtn = athenaBtn;
                banners.SetActive(true);
                currentSystemBtn.sprite = btnImages[5];
                break;
            case 2:
                currentSystem = eagisSystem;
                currentSystemBtn = messengerBtn;
                currentSystemBtn.sprite = btnImages[8];
                break;
            case 3:
                currentSystem = futureTechSystem;
                currentSystemBtn = techBtn;
                currentSystemBtn.sprite = btnImages[11];
                break;
        }
        currentSystemIndex = index;
        currentSystem.SetActive(true);
    }

    public void SwapUI(string btnName)
    {
        switch (btnName)
        {
            case "Debriefing":
                debriefing.SetActive(true);
                banners.SetActive(false);
                break;
            case "OperatorInfo":
                operatorInfos.SetActive(true);
                operatorRightInfo.SetActive(true);
                selectAbilUI.SetActive(false);
                selectToolUI.SetActive(false);
                banners.SetActive(false);
                
                LoadToolIcon();
                break;
            case "SelectTool":
                operatorRightInfo.SetActive(false);
                selectAbilUI.SetActive(false);
                selectToolUI.SetActive(true);

                selectToolIndex = -1;
                break;
            case "SelectAbil":
                operatorRightInfo.SetActive(false);
                selectAbilUI.SetActive(true);
                selectToolUI.SetActive(false);
                break;
            case "TechDev":
                techTestUI.SetActive(false);
                techDevUI.SetActive(true);
                break;
            case "TechTest":
                techDevUI.SetActive(false);
                techTestUI.SetActive(true);
                break;
        }
    }

    public void LoadPlayScene()
    {
        SceneManager.LoadScene("Scenes/PlayScene");
    }

    public void ChangeOperatorInfo(bool isLeft)
    {
        if (isLeft && currentOperatorNum > 0)
        {
            currentOperatorNum--;
        }
        else if(!isLeft && currentOperatorNum < 3)
        {
            currentOperatorNum++;
        }
        switch (currentOperatorNum)
        {
            case 0:
                operatorNameText.text = "01. 신화준";
                break;
            case 1:
                operatorNameText.text = "02. 빅토르";
                break;
            case 2:
                operatorNameText.text = "03. 레    오";
                break;
            case 3:
                operatorNameText.text = "04. 시노에";
                break;
        }
        operatorProfileImage.sprite = profileImages[currentOperatorNum];

        LoadToolIcon();
        selectToolIndex = -1;
    }

    public void LoadToolIcon() {
        for (int i = 0; i < 2; i++) {
            Tool tool = gameData.GetTool(currentOperatorNum, i);
            info_toolImages[i].sprite = toolSprites[tool];
            select_toolImages[i].sprite = toolSprites[tool];
            select_toolTexts[i].text = PlayerToolMgr.ToString(tool);
            select_toolTexts[i].color = NON_SELECTED_COLOR;
        }
    }
    public void ChangeToolIndex(int index) {
        if (selectToolIndex != -1)
            LoadToolIcon();

        if (selectToolIndex == index) {
            selectToolIndex = -1;
            return;
        }

        selectToolIndex = index;
        select_toolTexts[selectToolIndex].text = "선택 중";
        select_toolTexts[selectToolIndex].color = CHAING_COLOR;
    }
    public void ChangeTool(string toolName) {
        if (selectToolIndex == -1) return;

        Tool tool = Tool.FIREWALL;
        switch (toolName) {
        case "FIREWALL":    tool = Tool.FIREWALL;   break;
        case "FIRE_EX":     tool = Tool.FIRE_EX;    break;
        case "FLARE":       tool = Tool.FLARE;      break;
        case "O2_CAN":      tool = Tool.O2_CAN;     break;
        case "STICKY_BOMB": tool = Tool.STICKY_BOMB;break;
        }

        int otherIndex = (selectToolIndex+1) % 2;
        if (gameData.GetTool(currentOperatorNum, otherIndex) == tool)
            gameData.SetTool(currentOperatorNum, otherIndex, gameData.GetTool(currentOperatorNum, selectToolIndex));

        select_toolTexts[selectToolIndex].color = NON_SELECTED_COLOR;

        gameData.SetTool(currentOperatorNum, selectToolIndex, tool);
        gameData.Save();

        LoadToolIcon();
        selectToolIndex = -1;
    }
}