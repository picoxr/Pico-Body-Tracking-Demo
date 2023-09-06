using System;
using BodyTrackingDemo;
using Unity.XR.PXR;
using UnityEngine;

public class DancePadHole : MonoBehaviour
{
    [Flags]
    public enum DirectionType
    {
        Left = 1 << 0,
        Up = 1 << 2,
        Right = 1 << 3,
        Down = 1 << 4,
    }
    
    [SerializeField] private DirectionType direction;
    [SerializeField] private GameObject stepOnEffect;

    public Action<DancePadHole> onTrigger;
    
    public bool IsActive => _isActive;
    public LittleMoleController LittleMole => m_LittleMole;
    public DirectionType Direction => direction;
    public DancePadsManager DancePadsManager { get; set; }


    // Start is called before the first frame update    
    private bool _isActive;
    private Material m_Material;
    private LittleMoleController m_LittleMole;
    private int _lastScore;
    private int _triggerState;

    void Start()
    {
        _isActive = false;
        if (m_LittleMole == null)
        {
            m_LittleMole = GetComponentInChildren<LittleMoleController>();
        }
        m_LittleMole.OnStateIdle = SetHoleInactive;
        m_Material = GetComponent<MeshRenderer>().material;
    }

    private void OnTriggerStay(Collider other)
    {
        if (_triggerState > 0)
        {
            return;
        }

        var curBodyTrackerJoints = other.GetComponent<BodyTrackerJoint>();
        if (curBodyTrackerJoints == null)
        {
            return;
        }
        
        if (curBodyTrackerJoints.TrackingData.Action * 0.001f >= PlayerPrefManager.Instance.PlayerPrefData.steppingSensitivity)
        {
            int actionValue = 0;
            switch (curBodyTrackerJoints.bodyTrackerRole)
            {
                case BodyTrackerRole.LEFT_FOOT:
                    actionValue = DancePadsManager.LeftLegAction;
                    break;
                case BodyTrackerRole.RIGHT_FOOT:
                    actionValue = DancePadsManager.RightLegAction;
                    break;
            }

            if (actionValue >= (int)BodyActionList.PxrTouchGround)
            {
                _triggerState = 1;
                _lastScore = 0;

                SetHoleColor(actionValue);
                if (LittleMole.Kickable)
                {
                    PlayStepOnEffect();
                    _lastScore = LittleMole.OnLittleMoleKicked();
                }

                onTrigger?.Invoke(this);
                    
                Debug.Log($"DancePadHole.OnTriggerStay: other = {other.name}, LeftLegAction = {DancePadsManager.LeftLegAction}, RightLegAction = {DancePadsManager.RightLegAction}, FootAction = {curBodyTrackerJoints.TrackingData.Action}");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _triggerState = 0;
        SetHoleColor(0);
        
        Debug.Log($"DancePadHole.OnTriggerExit: other = {other.name}");
    }

    public void SetHoleActive()
    {
        _isActive = true;
        //Sliding window open, TBD
        m_LittleMole.Show();

    }

    public void SetHoleInactive()
    {
        _isActive = false;
    }


    public void SetHoleColor(int action)
    {
        if (action == 0)
        {
            m_Material.SetColor("_Color", new Color(0,0.425f, 0.165f));
        }
        else if ((action & (int) BodyActionList.PxrTouchGroundToe) != 0 && (action & (int) BodyActionList.PxrTouchGround) != 0)
        {
            m_Material.SetColor("_Color", (Color.red + Color.blue) * .5f);
        }
        else if ((action & (int) BodyActionList.PxrTouchGroundToe) != 0)
        {
            m_Material.SetColor("_Color", Color.blue);
        }
        else if ((action & (int) BodyActionList.PxrTouchGround) != 0)
        {
            m_Material.SetColor("_Color", Color.red);
        }
    }

    private void PlayStepOnEffect()
    {
        GameObject obj = Instantiate(stepOnEffect);
        obj.SetActive(true);
        obj.transform.position = transform.position + new Vector3(0, 0.1f, 0);
        obj.GetComponent<ParticleSystem>().Play();
    }
}
