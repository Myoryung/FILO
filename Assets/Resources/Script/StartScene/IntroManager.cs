using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class IntroManager : MonoBehaviour //인트로 화면 Mgr 화면 연출용
{
    public Image rightRedBG;
    public Image LoadingBG;
    public Image LoadingBar;

    public Image btn;
    public Sprite[] btn_Images;
    public Image[] borderBuildings;
    private Sprite loadingDoneBG;
    public Image glitchEffect;
    private int btn_index = 0;
    private void Awake()
    {
        loadingDoneBG = Resources.Load<Sprite>("Sprite/StartScene/Loading/Loading_2");
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ButtonMove();
        GameStart();
    }

    void ButtonMove() // 인트로 화면 선택된 버튼 변경
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (++btn_index >= 4)
            {
                btn_index = 0;
            }
            btn.sprite = btn_Images[btn_index];
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (--btn_index <= -1)
            {
                btn_index = 3;
            }
            btn.sprite = btn_Images[btn_index];
        }
    }

    void GameStart() // 게임 시작 상태에서 스페이스바 누를 시 호출
    {
        if(btn_index == 0 && Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(FadeoutScene());
        }
    }

    IEnumerator FadeoutScene() // 빨간 배경 애니메이션
    {
        while(true)
        {
            rightRedBG.rectTransform.Translate(600 * Time.deltaTime, 0, 0);
            for (int i = 0; i < 3; i++)
            {
                borderBuildings[i].fillAmount = 1 +(rightRedBG.rectTransform.localPosition.x + 40) / 1920;
            }
            if (rightRedBG.rectTransform.localPosition.x > 0)
            {
                break;
            }
            yield return null;
        }
        StartCoroutine(Loading());
    }

    IEnumerator Loading()
    {
        LoadingBG.enabled = true;
        while(LoadingBar.fillAmount < 1.0f)
        {
            LoadingBar.fillAmount += Time.deltaTime / 2;
            yield return null;
        }
        LoadingBG.sprite = loadingDoneBG;
        float count = 0;
        while(count < 0.15f)
        {
            glitchEffect.enabled = true;
            yield return null;
            glitchEffect.enabled = false;
            yield return null;
            count += Time.deltaTime;
        }
        glitchEffect.enabled = false;
        GameData gameData = new GameData();
        gameData.SetStageNumber(0);
        gameData.Save();
        SceneManager.LoadScene("Scenes/PlayScene");
        // 컴플리트 및 노이즈
    }

    //IEnumerator Fadeout()
    //{
    //    float alpha = 0;
    //    while(true)
    //    {
    //        alpha += Time.deltaTime;
    //        blackBG.color = new Color(0, 0, 0, alpha);
    //        if (blackBG.color.a >= 1.0f)
    //        {
    //            break;
    //        }
    //        yield return null;
    //    }
    //    alpha = 0;
    //    while (true)
    //    {
    //        alpha += Time.deltaTime;
    //        loading.color = new Color(1, 1, 1, alpha);
    //        loadingCircle.color = new Color(1, 1, 1, alpha);
    //        if (loading.color.a >= 1.0f)
    //        {
    //            break;
    //        }
    //        yield return null;
    //    }
    //}

    //IEnumerator CircleSpin()
    //{
    //    while(true)
    //    {
    //        loadingCircle.rectTransform.Rotate(0, 0, -32 * Time.deltaTime);
    //        yield return null;
    //    }
    //}
}