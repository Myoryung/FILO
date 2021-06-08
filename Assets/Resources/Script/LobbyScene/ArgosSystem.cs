using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ArgosSystem : MonoBehaviour
{
    public Text newsContents;
    private Vector2 uvOffset = Vector2.zero;
    public void LoadPlayScene()
    {
        SceneManager.LoadScene("Scenes/PlayScene");
    }

    public void Update()
    {
        newsContents.rectTransform.Translate(-1.0f * Time.deltaTime * 15, 0, 0);
        if (newsContents.rectTransform.position.x < -250.0f)
            newsContents.rectTransform.position = new Vector3(1160.0f, 1053, 0);
    }
}