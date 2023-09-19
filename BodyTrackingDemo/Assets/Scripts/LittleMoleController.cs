using UnityEngine;

public class LittleMoleController : MonoBehaviour
{
    //public const float SHOW_TIME = 0.3f;
    //public const float HIDE_TIME = 0.2f;
    public const float WAIT_TIME = 1.5f;
    public const float WAIT_POINT_HEIGHT = -7f;
    public const float SHOW_SPEED = 60f;
    public const float HIDE_SPEED = 90f;

    public delegate void ON_STATE_CHANGE_TO_IDLE();
    public ON_STATE_CHANGE_TO_IDLE OnStateIdle;

    private const float IDLE_POINT_HEIGHT = -25f;
    private MoleState m_MoleState;
    private float m_TimeCount;
    private bool m_Kickable;
    public bool Kickable
    {
        get { return m_Kickable; }
        //set { m_Kickable = value; }
    }

    private enum MoleState
    {
        IDLE,
        SHOW,
        WAIT,
        HIDE
    }

    // Start is called before the first frame update
    void Start()
    {
        this.transform.localPosition = new Vector3(0, IDLE_POINT_HEIGHT, 0);
        m_MoleState = MoleState.IDLE;
        m_TimeCount = 0;
        m_Kickable = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_MoleState == MoleState.IDLE)
        {
            return;
        }
        else if (m_MoleState == MoleState.SHOW)
        {
            this.transform.localPosition += new Vector3(0, SHOW_SPEED * Time.deltaTime, 0);
            if (this.transform.localPosition.y >= WAIT_POINT_HEIGHT)
            {
                m_MoleState = MoleState.WAIT;
                m_TimeCount = 0;
            }
        }
        else if (m_MoleState == MoleState.WAIT)
        {
            m_TimeCount += Time.deltaTime;
            if (m_TimeCount >= WAIT_TIME)
            {
                m_MoleState = MoleState.HIDE;
            }
        }
        else if (m_MoleState == MoleState.HIDE)
        {
            this.transform.localPosition -= new Vector3(0, HIDE_SPEED * Time.deltaTime, 0);
            if (this.transform.localPosition.y <= IDLE_POINT_HEIGHT)
            {
                m_MoleState = MoleState.IDLE;
                m_Kickable = false;
                if (OnStateIdle != null)
                {
                    OnStateIdle();
                }
            }
        }
    }

    public void Show()
    {
        m_MoleState = MoleState.SHOW;
        m_TimeCount = 0;
        m_Kickable = true;
        this.transform.localPosition = new Vector3(0, IDLE_POINT_HEIGHT, 0);
        //this.transform.LookAt(/*Camera.main.transform.position*/);
    }

    public void Hide()
    {
        m_MoleState = MoleState.HIDE;
    }

    public int OnLittleMoleKicked()
    {
        int score = 0;
        if (m_MoleState == MoleState.SHOW)
            score = 3;
        else if (m_MoleState == MoleState.WAIT)
            score = 2;
        else if (m_MoleState == MoleState.HIDE)
            score = 1;

        m_Kickable = false;
        m_MoleState = MoleState.HIDE;

        return score;
    }

}
