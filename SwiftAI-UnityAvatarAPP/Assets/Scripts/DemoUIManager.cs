using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.XR.PXR;

/// <summary>
/// Used to control demo scene UI logic
/// </summary>
public class DemoUIManager : MonoBehaviour
{
    public GameObject StartMenu;

    private Button m_ContinueButton;


    // Start is called before the first frame update    
    void Start()
    {
        //InitUI();
    }

    // Update is called once per frame
    void Update()
    {
        

    }

    public void InitUI(bool calibrated)
    {
        if (StartMenu == null )
        {
            Debug.LogError("[SwiftDemoDebug] Please check the Demo menu reference!");
            return;
        }

        DemoProcessController.Instance.MirrorObj.SetActive(false);
        if (DemoProcessController.Instance.AvatarObj != null)
            DemoProcessController.Instance.AvatarObj.SetActive(false);
        StartMenu.SetActive(true);
        
        m_ContinueButton = StartMenu.transform.Find("ContinueButton").GetComponent<Button>();
        
        if (m_ContinueButton == null)
        {
            Debug.LogError("[SwiftDemoDebug] Please check the Scene Hierarchy!");
        }


        // if the Demo process is START or the Swift is not calibrated, the player cannot continue to play, must restart to calibrate
        if (DemoProcessController.Instance.DemoProcessState == DemoProcessController.DemoProcess.START || !calibrated)
            m_ContinueButton.interactable = false;
        else
            m_ContinueButton.interactable = true;

    }


    /// <summary>
    /// On click Start button
    /// </summary>
    public void OnDemoStart()
    {
        StartMenu.SetActive(false);

        DemoProcessController.Instance.DemoProcessState = DemoProcessController.DemoProcess.START;
    }
    /// <summary>
    /// On click Continue button
    /// </summary>
    public void OnDemoContinue()
    {
        StartMenu.SetActive(false);
        DemoProcessController.Instance.DemoProcessState = DemoProcessController.DemoProcess.PLAYING;
        DemoProcessController.Instance.MirrorObj.SetActive(true);
        DemoProcessController.Instance.SetControllersActive(false);
        DemoProcessController.Instance.AvatarObj.SetActive(true);
    }


    

}
