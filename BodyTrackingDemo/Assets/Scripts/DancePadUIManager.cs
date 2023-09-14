using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DancePadUIManager : MonoBehaviour
{
    public Button GameStart;
    public Button GamePause;
    public Button GameContinue;    
    public Button GameStop;

    [SerializeField]
    private TextMeshProUGUI m_TextScore;


    public void SetScoreText(int score)
    {
        m_TextScore.text = "" + score;
        //Debug.Log("[DragonTest] Total Score = " + score);
    }
}
