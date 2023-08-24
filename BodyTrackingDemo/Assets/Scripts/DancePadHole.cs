using System;
using System.Collections.Generic;
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
    private readonly List<BodyTrackerJoint> _curBodyTrackerJoints = new(2);

    void Start()
    {
        _isActive = false;
        if (m_LittleMole == null)
        {
            m_LittleMole = this.GetComponentInChildren<LittleMoleController>();
        }
        m_LittleMole.OnStateIdle = SetHoleInactive;
        m_Material = this.GetComponent<MeshRenderer>().material;
    }

    private void OnTriggerEnter(Collider other)
    {
        var curBodyTrackerJoints = other.GetComponent<BodyTrackerJoint>();
        if (curBodyTrackerJoints != null)
        {
            _curBodyTrackerJoints.Add(curBodyTrackerJoints);    
        }

        Debug.Log($"DancePadHole.OnTriggerEnter: other = {other.name}, JointCount = {_curBodyTrackerJoints.Count}");
    }

    private void OnTriggerStay(Collider other)
    {
        if (_triggerState > 0)
        {
            return;
        }
        
        if (_curBodyTrackerJoints.Count == 0)
        {
            return;
        }

        foreach (var item in _curBodyTrackerJoints)
        {
            if ((item.bodyTrackerRole == BodyTrackerRole.LEFT_FOOT && DancePadsManager.LeftLegAction == 1) ||
                (item.bodyTrackerRole == BodyTrackerRole.RIGHT_FOOT && DancePadsManager.RightLegAction == 1))
            {
                if (item.TrackingData.Action * 0.001f > .8f)
                {
                    _triggerState = 1;
                    _lastScore = 0;
                    SetHoleColor(true);
                    if (LittleMole.Kickable)
                    {
                        PlayStepOnEffect();
                        _lastScore = LittleMole.OnLittleMoleKicked();
                    }

                    onTrigger?.Invoke(this);
                    
                    Debug.Log($"DancePadHole.OnTriggerStay: other = {other.name}, FootAction = {item.TrackingData.Action}");
                    break;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _triggerState = 0;
        _curBodyTrackerJoints.Remove(other.GetComponent<BodyTrackerJoint>());
        SetHoleColor(false);
        
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


    public void SetHoleColor(bool active)
    {
        if (active)
            m_Material.SetColor("_Color", new Color(0.726f, 0.3f, 0));
        else
            m_Material.SetColor("_Color", new Color(0,0.425f, 0.165f));
    }
    
    private void PlayStepOnEffect()
    {
        GameObject obj = Instantiate(stepOnEffect);
        obj.SetActive(true);
        obj.transform.position = transform.position + new Vector3(0, 0.1f, 0);
        obj.GetComponent<ParticleSystem>().Play();
    }
}
