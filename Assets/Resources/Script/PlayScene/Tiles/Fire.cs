using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps; // Tilemap용

public class Fire : MonoBehaviour
{
    private Tilemap obstacles; // 불 타일이 위치할 타일맵
    public TileBase TFire; // 해당 오브젝트를 포함하는 TileBase
    private Vector3Int _pos; // 타일맵 상의 좌표
    public Vector3Int Pos // 좌표 Property
    {
        set { _pos = value; }
        get { return _pos;  }
    }

    private List<int> _SearchArea; // 5x5 랜덤 탐색을 위한 Container
    
    private void Start()
    {
        obstacles = GameObject.Find("Grid/Obstacle").GetComponent<Tilemap>();
        transform.Translate(0, 0, -1);
        _SearchArea = new List<int>();
        for(int i=0; i<25;i++)
        {
            _SearchArea.Add(i);
        }
    }

    public void SpreadFire() // 턴이 끝날 때 큰 불 번지는 함수
    {
        for(int x=-1; x<=1; x++)
        {
            for(int y=-1; y<=1; y++)
            {
                Vector3Int nPos = new Vector3Int(_pos.x + x, _pos.y + y, _pos.z);
                Debug.Log(gameObject.name + x.ToString() + "," + y.ToString());
                if (obstacles.GetTile(nPos) == null)
                {
                    obstacles.SetTile(nPos, TFire);
                }
            }
        }
    }

    private void Shufflelist() // SearchArea의 값을 랜덤하게 변경
    {
        int random1;
        int random2;

        int tmp;

        for (int index = 0; index < _SearchArea.Count; ++index) // 탐색범위만큼 반복
        {
            random1 = UnityEngine.Random.Range(0, _SearchArea.Count);
            random2 = UnityEngine.Random.Range(0, _SearchArea.Count);

            tmp = _SearchArea[random1];
            _SearchArea[random1] = _SearchArea[random2];
            _SearchArea[random2] = tmp;
        }
    }

    public void FindEmberArea()
    {
        if(GameMgr.Instance.UsedEmberCount >= GameMgr.Instance.Embers.childCount) // Object pool의 최대값을 넘긴다면 탐색 정지
        {
            return;
        }
        Shufflelist();
        for(int i=0; i<_SearchArea.Count; i++)
        {
            Vector3Int nPos = new Vector3Int(_pos.x + (_SearchArea[i] % 5 -2), _pos.y + (_SearchArea[i] / 5 - 2), _pos.z);  // 랜덤으로 타일맵의 좌표 할당
            if(obstacles.GetTile(nPos) == null) // 해당 위치에 아무것도 없으면 작은 불 생성
            {
                //좌표값의 변화에 따른 코드 수정 필요함
                GameMgr.Instance.Embers.GetChild(GameMgr.Instance.UsedEmberCount).position = transform.position + new Vector3(1000 * (_SearchArea[i] % 5) - 2000, 1000 * (_SearchArea[i] / 5) - 2000, 0);
                GameMgr.Instance.Embers.GetChild(GameMgr.Instance.UsedEmberCount).gameObject.SetActive(true);
                GameMgr.Instance.UsedEmberCount++;
                break;
            }
        }
    }
}
