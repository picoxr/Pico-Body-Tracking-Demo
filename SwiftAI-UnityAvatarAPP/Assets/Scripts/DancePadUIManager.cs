using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DancePadUIManager : MonoBehaviour
{
    public Button GameStart;
    public Button GamePause;
    public Button GameContinue;    
    public Button GameStop;

    [SerializeField]
    private Text m_TextScore;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetScoreText(int score)
    {
        m_TextScore.text = "Score: " + score;
        //Debug.Log("[DragonTest] Total Score = " + score);
    }
}
