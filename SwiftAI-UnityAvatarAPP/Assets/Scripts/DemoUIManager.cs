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
    public GameObject HeightGenderMenu;

    private const int MIN_HEIGHT = 140;     //Minimum Avatar height supported
    private const int MAX_HEIGHT = 200;
    private Slider m_HeightSlider;
    private Text m_SliderValueText;
    private Button m_MaleButton;
    private Button m_FemaleButton;
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
        if (StartMenu == null || HeightGenderMenu == null)
        {
            Debug.LogError("[SwiftDemoDebug] Please check the Demo menu reference!");
            return;
        }

        DemoProcessController.Instance.MirrorObj.SetActive(false);
        if (DemoProcessController.Instance.AvatarObj != null)
            DemoProcessController.Instance.AvatarObj.SetActive(false);
        StartMenu.SetActive(true);
        HeightGenderMenu.SetActive(false); 
        m_HeightSlider = HeightGenderMenu.transform.Find("HeightSlider").GetComponent<Slider>();
        if (m_HeightSlider != null)
            m_SliderValueText = m_HeightSlider.transform.Find("Value Text").GetComponent<Text>();

        m_ContinueButton = StartMenu.transform.Find("ContinueButton").GetComponent<Button>();
        m_MaleButton = HeightGenderMenu.transform.Find("MaleButton").GetComponent<Button>();
        m_FemaleButton = HeightGenderMenu.transform.Find("FemaleButton").GetComponent<Button>();

        if (m_HeightSlider == null || m_SliderValueText == null || m_ContinueButton == null || m_MaleButton == null || m_FemaleButton == null)
        {
            Debug.LogError("[SwiftDemoDebug] Please check the Scene Hierarchy!");
        }


        // if the Demo process is START or the Swift is not calibrated, the player cannot continue to play, must restart to calibrate
        if (DemoProcessController.Instance.DemoProcessState == DemoProcessController.DemoProcess.START || !calibrated)
            m_ContinueButton.interactable = false;
        else
            m_ContinueButton.interactable = true;

        m_MaleButton.interactable = DemoProcessController.Instance.PlayerGender == 0;
        m_FemaleButton.interactable = DemoProcessController.Instance.PlayerGender == 1;

        m_HeightSlider.value = 30f;
    }

    /// <summary>
    /// Calculate the Height based on current slider value
    /// </summary>
    public void OnHeightSliderValueChanged()
    {
        DemoProcessController.Instance.PlayerHeight = MIN_HEIGHT + (int)m_HeightSlider.value;
        m_SliderValueText.text = DemoProcessController.Instance.PlayerHeight + "cm";
    }

    /// <summary>
    /// On click Start button
    /// </summary>
    public void OnDemoStart()
    {
        StartMenu.SetActive(false);
        HeightGenderMenu.SetActive(true);
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


    /// <summary>
    /// On click gender buttons, control the logic of male and female buttons.
    /// </summary>
    /// <param name="gender"></param>
    public void OnGenderButtons(int gender)
    {
        if (DemoProcessController.Instance.PlayerGender == gender) return;
        DemoProcessController.Instance.PlayerGender = gender;
        Debug.Log("[SwiftDemoDebug] Gender = " + DemoProcessController.Instance.PlayerGender);

        var colors = m_MaleButton.colors;
        m_MaleButton.colors = m_FemaleButton.colors;
        m_FemaleButton.colors = colors;
        m_MaleButton.interactable = gender == 0;
        m_FemaleButton.interactable = gender == 1;
    }

    /// <summary>
    /// On click Confirm button
    /// </summary>
    public void OnConfirmHeightAndGender()
    {
        HeightGenderMenu.SetActive(false);
        DemoProcessController.Instance.CalibrateMotionTracker();
    }

    

}
