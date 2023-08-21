using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DancePadHole : MonoBehaviour
{
    // Start is called before the first frame update    
    private bool m_isActive;
    public bool IsActive
    {
        get { return m_isActive; }
    }
    private Material m_Material;
    private LittleMoleController m_LittleMole;
    public LittleMoleController LittleMole
    {
        get { return m_LittleMole; }
    }

    void Start()
    {
        m_isActive = false;
        if (m_LittleMole == null)
        {
            m_LittleMole = this.GetComponentInChildren<LittleMoleController>();
        }
        m_LittleMole.OnStateIdle = SetHoleInactive;
        m_Material = this.GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetHoleActive()
    {
        m_isActive = true;
        //Sliding window open, TBD
        m_LittleMole.Show();

    }

    public void SetHoleInactive()
    {
        m_isActive = false;
    }


    public void SetHoleColor(bool active)
    {
        if (active)
            m_Material.SetColor("_Color", new Color(0.726f, 0.3f, 0));
        else
            m_Material.SetColor("_Color", new Color(0,0.425f, 0.165f));
    }


}
