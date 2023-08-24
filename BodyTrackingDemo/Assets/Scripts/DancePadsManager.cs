using UnityEngine;

public class DancePadsManager : MonoBehaviour
{
    [SerializeField] private GameObject StepOnEffect;
    [SerializeField] private DancePadUIManager DancePadUI;
    [SerializeField] private DancePadHole[] dancePadHoles;
    
    private int m_DancePadHoleCount = 0;
    
    private bool[] m_LeftStepOnController;
    private bool[] m_RightStepOnController;

    public int LeftLegAction { get; private set; }
    public int RightLegAction{ get; private set; }

    //LittleMole game related
    private bool m_isDancePadGamePlaying;  //Judge whether the dance pad game is playing
    private int m_TotalScore = 0;

    // Start is called before the first frame update
    void Start()
    {
        InitDancePad();
        //OnStartDancePadGame();
    }

    /// <summary>
    /// Init Dance pad, get the materals of the directionType panels
    /// </summary>
    private void InitDancePad()
    {
        dancePadHoles ??= GetComponentsInChildren<DancePadHole>();
        
        m_DancePadHoleCount = dancePadHoles.Length;
        if (m_DancePadHoleCount > 0)
        {
            m_LeftStepOnController = new bool[m_DancePadHoleCount];
            m_RightStepOnController = new bool[m_DancePadHoleCount];

            for (int i = 0; i < m_DancePadHoleCount; i++)
            {
                if (dancePadHoles[i] == null) continue;
                m_LeftStepOnController[i] = false;
                m_RightStepOnController[i] = false;

                dancePadHoles[i].DancePadsManager = this;
                dancePadHoles[i].onTrigger += OnPadTrigger;
            }
        }

        DancePadUI.GameStart.interactable = true;
        DancePadUI.GamePause.interactable = false;
        DancePadUI.GameContinue.interactable = false;
        DancePadUI.GameStop.interactable = false;
    }

    private void OnPadTrigger(DancePadHole dancePadHole)
    {
        int score = dancePadHole.LittleMole.OnLittleMoleKicked();
        if (score > 0)
        {
            m_TotalScore += score;
            DancePadUI.SetScoreText(m_TotalScore);    
        }
    }

    /// <summary>
    /// Detect the directionType panels state in runtime
    /// </summary>
    /// <param name="leftFoot">Avatar left foot position</param>
    /// <param name="rightFoot">Avatar right foot position</param>
    /// <param name="leftStepOn">Left foot touch gournd return value, 1 means step on the ground, 0 means not</param>
    /// <param name="rightStepOn">Right foot touch gournd return value, 1 means step on the ground, 0 means not</param>
    public void DancePadHoleStepOnDetection(Vector3 leftFoot, Vector3 rightFoot, int leftStepOn = 1, int rightStepOn = 1)
    {
        LeftLegAction = leftStepOn;
        RightLegAction = rightStepOn;
        // for (int i = 0; i < m_DancePadHoleCount; i++)
        // {
        //     //detect left foot step on action
        //     if (leftStepOn == 1 && dancePadHoles[i].GetComponent<BoxCollider>().bounds.Contains(leftFoot))
        //     {
        //         if (!m_LeftStepOnController[i])
        //         {
        //             dancePadHoles[i].SetHoleColor(true);
        //             m_LeftStepOnController[i] = true;
        //             if (dancePadHoles[i].LittleMole.Kickable)
        //             {
        //                 PlayStepOnEffect(i);
        //                 int score = dancePadHoles[i].LittleMole.OnLittleMoleKicked();
        //                 m_TotalScore += score;
        //                 DancePadUI.SetScoreText(m_TotalScore);
        //             }
        //         }
        //     }
        //     else
        //     {
        //         if (!m_RightStepOnController[i])
        //             dancePadHoles[i].SetHoleColor(false);
        //         m_LeftStepOnController[i] = false;
        //     }
        //
        //     // detect right foot step on action
        //     if (rightStepOn == 1 && dancePadHoles[i].GetComponent<BoxCollider>().bounds.Contains(rightFoot))
        //     {
        //         if (!m_RightStepOnController[i])
        //         {
        //             dancePadHoles[i].SetHoleColor(true);
        //             m_RightStepOnController[i] = true;
        //             if (dancePadHoles[i].LittleMole.Kickable)
        //             {
        //                 PlayStepOnEffect(i);
        //                 int score = dancePadHoles[i].LittleMole.OnLittleMoleKicked();
        //                 m_TotalScore += score;
        //                 DancePadUI.SetScoreText(m_TotalScore);
        //             }
        //         }
        //     }
        //     else
        //     {
        //         if (!m_LeftStepOnController[i])
        //             dancePadHoles[i].SetHoleColor(false);
        //         m_RightStepOnController[i] = false;
        //     }
        // }
    }


    private void PlayStepOnEffect(int index)
    {
        index = index % m_DancePadHoleCount;
        GameObject obj = Instantiate(StepOnEffect);
        obj.transform.position = dancePadHoles[index].transform.position + new Vector3(0, 0.1f, 0);
        obj.GetComponent<ParticleSystem>().Play();
    }


    public void OnStartDancePadGame()
    {
        m_isDancePadGamePlaying = true;
        m_TotalScore = 0;
        CancelInvoke();
        InvokeRepeating("ActiveDancePadHoleRandomly", 1.0f, 1.0f);
        DancePadUI.GameStart.interactable = false;
        DancePadUI.GamePause.interactable = true;
        DancePadUI.GameContinue.interactable = false;
        DancePadUI.GameStop.interactable = true;
    }

    public void OnPauseDancePadGame()
    {
        m_isDancePadGamePlaying = false;
        CancelInvoke();
        DancePadUI.GameStart.interactable = false;
        DancePadUI.GamePause.interactable = false;
        DancePadUI.GameContinue.interactable = true;
        DancePadUI.GameStop.interactable = true;
    }

    public void OnContinueDancePadGame()
    {
        m_isDancePadGamePlaying = true;
        CancelInvoke();
        InvokeRepeating("ActiveDancePadHoleRandomly", 1.0f, 1.0f);
        DancePadUI.GameStart.interactable = false;
        DancePadUI.GamePause.interactable = true;
        DancePadUI.GameContinue.interactable = false;
        DancePadUI.GameStop.interactable = true;
    }

    public void OnStopDancePadGame()
    {
        m_isDancePadGamePlaying = false;
        CancelInvoke();
        DancePadUI.GameStart.interactable = true;
        DancePadUI.GamePause.interactable = false;
        DancePadUI.GameContinue.interactable = false;
        DancePadUI.GameStop.interactable = false;
    }

    private void ActiveDancePadHoleRandomly()
    {
        int active_index = Random.Range(0, m_DancePadHoleCount);
        //Debug.Log("[DragonTest] active_index = " + active_index);
        if (dancePadHoles[active_index].IsActive)
        {
            ActiveDancePadHoleRandomly();
            return;
        }
        dancePadHoles[active_index].SetHoleActive();

    }
}
