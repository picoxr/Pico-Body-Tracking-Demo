using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DancePadsManager : MonoBehaviour
{
    public GameObject StepOnEffect;
    public DancePadUIManager DancePadUI;

    private int m_DancePadHoleCount = 0;
    private DancePadHole[] m_DancePadHoles;
    private bool[] m_LeftStepOnController;
    private bool[] m_RightStepOnController;


    //LittleMole game related
    private bool m_isDancePadGamePlaying;  //Judge whether the dance pad game is playing
    private int m_TotalScore = 0;
    private int m_OneKickScore = 0;

    // Start is called before the first frame update
    void Start()
    {
        InitDancePad();
        //OnStartDancePadGame();
    }

    // Update is called once per frame
    void Update()
    {
        //if (m_isDancePadGamePlaying)
        //{

        //}
    }

    /// <summary>
    /// Init Dance pad, get the materals of the direction panels
    /// </summary>
    private void InitDancePad()
    {
        m_DancePadHoles = this.GetComponentsInChildren<DancePadHole>();
        Debug.Log("[DragonTest] m_DancePadHoles.length = " + m_DancePadHoles.Length);
        m_DancePadHoleCount = m_DancePadHoles.Length;
        if (m_DancePadHoleCount > 0)
        {
            m_LeftStepOnController = new bool[m_DancePadHoleCount];
            m_RightStepOnController = new bool[m_DancePadHoleCount];

            for (int i = 0; i < m_DancePadHoleCount; i++)
            {
                if (m_DancePadHoles[i] == null) continue;
                m_LeftStepOnController[i] = false;
                m_RightStepOnController[i] = false;
            }
        }

        DancePadUI.GameStart.interactable = true;
        DancePadUI.GamePause.interactable = false;
        DancePadUI.GameContinue.interactable = false;
        DancePadUI.GameStop.interactable = false;
    }

    /// <summary>
    /// Detect the direction panels state in runtime
    /// </summary>
    /// <param name="leftFoot">Avatar left foot position</param>
    /// <param name="rightFoot">Avatar right foot position</param>
    /// <param name="leftStepOn">Left foot touch gournd return value, 1 means step on the ground, 0 means not</param>
    /// <param name="rightSteipOn">Right foot touch gournd return value, 1 means step on the ground, 0 means not</param>
    public void DancePadHoleStepOnDetection(Vector3 leftFoot, Vector3 rightFoot, int leftStepOn = 1, int rightSteipOn = 1)
    {
        for (int i = 0; i < m_DancePadHoleCount; i++)
        {
            //detect left foot step on action
            if (leftStepOn == 1 && m_DancePadHoles[i].GetComponent<BoxCollider>().bounds.Contains(leftFoot))
            {
                if (!m_LeftStepOnController[i])
                {
                    m_DancePadHoles[i].SetHoleColor(true);
                    m_LeftStepOnController[i] = true;
                    if (m_DancePadHoles[i].LittleMole.Kickable)
                    {
                        PlayStepOnEffect(i);
                        m_DancePadHoles[i].LittleMole.OnLittleMoleKicked(ref m_OneKickScore);
                        m_TotalScore += m_OneKickScore;
                        DancePadUI.SetScoreText(m_TotalScore);
                    }
                }
            }
            else
            {
                if (!m_RightStepOnController[i])
                    m_DancePadHoles[i].SetHoleColor(false);
                m_LeftStepOnController[i] = false;
            }

            // detect right foot step on action
            if (rightSteipOn == 1 && m_DancePadHoles[i].GetComponent<BoxCollider>().bounds.Contains(rightFoot))
            {
                if (!m_RightStepOnController[i])
                {
                    m_DancePadHoles[i].SetHoleColor(true);
                    m_RightStepOnController[i] = true;
                    if (m_DancePadHoles[i].LittleMole.Kickable)
                    {
                        PlayStepOnEffect(i);
                        m_DancePadHoles[i].LittleMole.OnLittleMoleKicked(ref m_OneKickScore);
                        m_TotalScore += m_OneKickScore;
                        DancePadUI.SetScoreText(m_TotalScore);
                    }
                }
            }
            else
            {
                if (!m_LeftStepOnController[i])
                    m_DancePadHoles[i].SetHoleColor(false);
                m_RightStepOnController[i] = false;
            }


        }

    }


    private void PlayStepOnEffect(int index)
    {
        index = index % m_DancePadHoleCount;
        GameObject obj = Instantiate(StepOnEffect);
        obj.transform.position = m_DancePadHoles[index].transform.position + new Vector3(0, 0.1f, 0);
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
        if (m_DancePadHoles[active_index].IsActive)
        {
            ActiveDancePadHoleRandomly();
            return;
        }
        m_DancePadHoles[active_index].SetHoleActive();

    }

}
