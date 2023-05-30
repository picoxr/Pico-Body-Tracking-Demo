using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.UI;


public class DemoProcessController : MonoBehaviour
{
    public static DemoProcessController Instance;
    public DemoUIManager DemoUIManagerInstance;

    public GameObject RenderCameraObj;
    public GameObject MirrorObj;
    public GameObject ControllerLeftObj;
    public GameObject ControllerRightObj;
    public GameObject SkeletonRoot;
    public GameObject PPText;
    public GameObject SkeletonText;
    public GameObject effectText2;
    public static GameObject effectText;


    
    public Transform[] SkeletonNodes = new Transform[22];
    public LineRenderer[] Lines = new LineRenderer[21];
    [HideInInspector]
    public int PlayerHeight = 160;          //unit: cm
    [HideInInspector]
    public int PlayerGender = 0;            //0-Female, 1-Male, lady first
    [HideInInspector]
    public DemoProcess DemoProcessState = DemoProcess.START;

    public enum DemoProcess
    {
        START,
        CALIBRATING,
        CALIBRATED,
        PLAYING,
    }

    public Vector3 DiffPosition
    {
        get { return m_DiffPosition; }
        set { m_DiffPosition = value; }
    }

    public GameObject AvatarObj { get { return m_AvatarObj; } }
    public bool SwiftCalibrated { get { return m_SwiftCalibratedState; } }

    private const int LAYER_DEFAULT = 0;
    private const int LAYER_PLAYER_MIRROR = 9;
    private const int LAYER_PLAYER_PLAYER = 12;
    private GameObject m_AvatarObj;
    private float m_ViewRadius = 2.8f;
    private float m_CameraHeight = 1.27f;
    private List<Vector3> m_ViewPositionList = new List<Vector3>();
    private int m_ViewIndex = 0;
    private bool m_PrimaryButtonDown;
    private bool m_SecondaryButtonDown;
    private bool m_GripButtonDown;
    private bool m_TriggerButtonDown;
    private bool m_EnablePP = false;
    private bool m_LoadAvatar = false;
    private bool m_SwiftCalibratedState = false;    //Is swift calibrated

    private Vector3 m_DiffPosition;
    

    private void Awake()
    {
        Instance = this;
        string logPath = GetLogPath();
        if (System.IO.File.Exists(logPath))
        {
            System.IO.File.Delete(logPath);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Swift sample start");
        DemoProcessState = DemoProcess.START;
        
        int calibrated = -1;
        PXR_Input.GetFitnessBandCalibState(ref calibrated);
        m_SwiftCalibratedState = calibrated == 1;
        
        SetControllersActive(true);        
        DemoUIManagerInstance.InitUI(m_SwiftCalibratedState);

        for (int i = 0; i < SkeletonNodes.Length; i++)
        {
            SkeletonNodes[i] = GameObject.Find("SkeletonRoot/SphereNode" + i).transform;
            if (i < 21)
            {
                Lines[i] = GameObject.Find("SkeletonRoot/Line" + i).GetComponent<LineRenderer>();
                Lines[i].startColor = Color.red;
                Lines[i].endColor = Color.red;
                Lines[i].startWidth = 0.01f;
                Lines[i].endWidth = 0.01f;
            }
        }
        SkeletonRoot.SetActive(false);
        effectText = effectText2;

        //For testing Load Avatar
        //LoadAvatar();
        //MirrorObj.SetActive(true);
        //UpdateAlgorithmXML();
        //EnablePPAlgorithm(true);
    }

    // Update is called once per frame
    void Update()
    {
        DrawLineBetweenNodes();
        if (DemoProcessState == DemoProcess.PLAYING)
        {
            if (InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool right_pressed) && right_pressed ||
                InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool left_pressed) && left_pressed)
            {
                CalibrateMotionTracker();
            }




            if (m_AvatarObj != null)
            {
                if(InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.secondaryButton, out bool right_B) && right_B ||
                InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.secondaryButton, out bool left_B) && left_B)
                {
                    if (m_SecondaryButtonDown)
                    {
                        m_SecondaryButtonDown = false;
                        m_EnablePP = !m_EnablePP;
                        CalibrateMotionTracker();
                        if(m_EnablePP)
                            PPText.GetComponent<Text>().text = "Click B/Y button to Disable/Enable PP algorithm, which will not lock the feet height but achieve more accurate position data. CurrentPP:Enabled";
                        else
                            PPText.GetComponent<Text>().text = "Click B/Y button to Disable/Enable PP algorithm, which will not lock the feet height but achieve more accurate position data. CurrentPP:Disabled";
                    }
                }
                else
                {
                    m_SecondaryButtonDown = true;   
                }

                if (InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.primaryButton, out bool left_A) && left_A)
                {
                    if (m_PrimaryButtonDown && !m_EnablePP)
                    {
                        m_PrimaryButtonDown = false;
                        SkeletonRoot.SetActive(!SkeletonRoot.activeSelf);
                        if(SkeletonRoot.activeSelf)
                        {
                            SetModelLayer(m_AvatarObj, LAYER_PLAYER_PLAYER);
                            SkeletonText.GetComponent<Text>().text = "Click X button to Disable/Enable skeleton debug nodes. Current Skeleton:Enabled.";
                        }
                        else
                        {
                            SetModelLayer(m_AvatarObj, LAYER_DEFAULT);
                            SkeletonText.GetComponent<Text>().text = "Click X button to Disable/Enable skeleton debug nodes. Current Skeleton:Disabled.";
                        }
                    }
                }
                else
                {
                    m_PrimaryButtonDown = true;
                }

                if (InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.primaryButton, out bool right_A) && right_A )
                {
                    if(m_PrimaryButtonDown)
                    {
                        string filter  = effectText2.GetComponent<Text>().text;
                        if(filter == "Click A button to Disable/Enable stomping effects. Current stomping: Disabled.")
                        {
                            filter = "Click A button to Disable/Enable stomping effects. Current stomping: Enabled.";
                        }
                        else
                        {
                            filter = "Click A button to Disable/Enable stomping effects. Current stomping: Disabled.";
                        }
                        effectText2.GetComponent<Text>().text = filter;
                    }
                }
                else
                {
                    m_PrimaryButtonDown = true;
                }


                // Click Trigger button to adjust Hips height dynamically
                if (InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.triggerButton, out bool right_trigger) && right_trigger ||
                InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.triggerButton, out bool left_trigger) && left_trigger)
                {
                    if (m_TriggerButtonDown)
                    {
                        m_TriggerButtonDown = false;
                        UpdateAdjustment();
                    }
                }
                else
                {
                    m_TriggerButtonDown = true;
                }
            }


            if (m_AvatarObj != null && SkeletonRoot.activeSelf)
                DrawLineBetweenNodes();
        }

    }

    private void OnApplicationPause(bool pause)
    {
        Debug.Log("application pause: " + pause);

    }

    private void OnApplicationFocus(bool focus)
    {
        Debug.Log("[SwiftDemoTest] Application focus: " + focus);
        //if (focus && m_LoadAvatar)
        //{
        //    m_LoadAvatar = false;
        //    DemoSceneInit();
        //}

        if (focus)
        {
            if (DemoProcessState == DemoProcess.START) return;

            //Update Swift calibration state after resuming
            int calibrated = -1;
            PXR_Input.GetFitnessBandCalibState(ref calibrated);
            m_SwiftCalibratedState = calibrated == 1;
            Debug.Log("[SwiftDemoTest] Swift calibrated: " + calibrated);
            if (DemoProcessState == DemoProcess.CALIBRATING && m_SwiftCalibratedState)
            {
                DemoProcessState = DemoProcess.CALIBRATED;

                //load avatar
                LoadAvatar();
                MirrorObj.SetActive(true);
                //SetControllersActive(false);
            }
            else
            {
                DemoUIManagerInstance.InitUI(m_SwiftCalibratedState);
                SetControllersActive(true);
            }

        }


    }

    private void OnApplicationQuit()
    {
        Debug.Log("simple sample quit");
    }


    public string GetLogPath()
    {
        return System.IO.Path.Combine(Application.persistentDataPath, "SwiftAI.txt");

    }


    private void DemoSceneInit()
    {
        // set view position
        if (m_ViewPositionList.Count == 0)
        {
            m_ViewPositionList.Add(GetViewPosition(ViewDirection.Back, m_ViewRadius));
            m_ViewPositionList.Add(GetViewPosition(ViewDirection.Right, m_ViewRadius));
            m_ViewPositionList.Add(GetViewPosition(ViewDirection.Right_Forward, m_ViewRadius));
            m_ViewPositionList.Add(GetViewPosition(ViewDirection.Forward, m_ViewRadius));
            m_ViewPositionList.Add(GetViewPosition(ViewDirection.Left_Forward, m_ViewRadius));
            m_ViewPositionList.Add(GetViewPosition(ViewDirection.Left, m_ViewRadius));
            Debug.Log("view positions: " + string.Join(",", m_ViewPositionList));

            if (mModelsArray == null || mModelsArray.Length == 0)
            {
                mModelsArray = Resources.LoadAll<GameObject>("Models");
                Debug.Log("all models >  count: " + mModelsArray.Length + " items: " + string.Join<GameObject>(",", mModelsArray));
            }

            m_ViewIndex = 0;
            RenderCameraObj.transform.position = m_ViewPositionList[m_ViewIndex];
            RenderCameraObj.transform.rotation = Quaternion.identity;
            // show mirror
            if (MirrorObj != null)
            {
                MirrorObj.SetActive(true);
            }
            mModelIndex = 0;
            if (m_AvatarObj == null)
            {
                m_AvatarObj = LoadModel(mModelIndex);
            }
            // set model layer
            SetModelLayer(m_AvatarObj, LAYER_PLAYER_MIRROR);

            SetControllersActive(true);
        }
    }

    public void SetControllersActive(bool active)
    {
        if (ControllerLeftObj != null)
        {
            ControllerLeftObj.SetActive(PXR_Input.IsControllerConnected(PXR_Input.Controller.LeftController) && active);
        }
        if (ControllerRightObj != null)
        {
            ControllerRightObj.SetActive(PXR_Input.IsControllerConnected(PXR_Input.Controller.RightController) && active);
        }
    }


    private void Calibrate()
    {
#if !UNITY_EDITOR
        int calibrated = -1;
        PXR_Input.GetFitnessBandCalibState(ref calibrated);
        Debug.Log("calibrated: " + calibrated);
        if (calibrated != 1)
        {
            Debug.Log("start calibrate");
            PXR_Input.OpenFitnessBandCalibrationAPP();
        }
#endif
    }

    #region Change model

    private Vector3 GetViewPosition(ViewDirection dir, float radius)
    {
        if (dir == ViewDirection.Back)
        {
            return new Vector3(0, m_CameraHeight, 0);
            //return Vector3.zero;
        }

        double radian = ((int)dir - 1) * Math.PI * 0.25f;
        double x = Math.Cos(radian) * radius;
        double z = Math.Sin(radian) * radius;
        Vector3 position;
        position.x = (float)x;
        position.y = m_CameraHeight;
        position.z = (float)z;
        return position;
    }

    private enum ViewDirection
    {
        Back,
        Right,
        Right_Forward,
        Forward,
        Left_Forward,
        Left,
    }

    

    private void ChangeModel(int index)
    {
        if (m_AvatarObj != null)
        {
            m_AvatarObj.SetActive(false);
        }
        m_AvatarObj = LoadModel(index);
        m_AvatarObj.SetActive(true);
    }

    #endregion



    #region Load Avatar

    private GameObject[] mModelsArray;
    private Dictionary<int, GameObject> mModelDic = new Dictionary<int, GameObject>();
    private int mModelIndex = 0;

    private GameObject LoadModel(int modelIndex)
    {
        Debug.Log("load model: " + modelIndex);
        if (mModelDic.ContainsKey(modelIndex))
        {
            return mModelDic[modelIndex];
        }
        else
        {
            GameObject go = Instantiate(mModelsArray[modelIndex], Vector3.zero, Quaternion.identity);
            mModelDic.Add(modelIndex, go);
            return go;
        }
    }

    /// <summary>
    /// Load Avatar based on player height and gender
    /// </summary>
    public static bool loadavatar = false;
    private void LoadAvatar()
    {
        Debug.Log("[SwiftDemoTest] LoadAvatar");
        if (m_AvatarObj != null)
        {
            GameObject temp = m_AvatarObj;
            m_AvatarObj = null;
            GameObject.Destroy(temp);
        }

        // current logic use only one Avatar model for each gender.
        string avatar_name = (PlayerGender == 0 ? "ClothRun_" : "ClothStripe_") + "170 Variant";
        Debug.Log("[SwiftDemoTest] Models/" + avatar_name);
        GameObject avatarObj = Resources.Load<GameObject>("Models/" + avatar_name);
        m_AvatarObj = Instantiate(avatarObj, Vector3.zero, Quaternion.identity);
        float scale = PlayerHeight / 170.0f;
        m_AvatarObj.transform.localScale = new Vector3(scale, scale, scale);
        Debug.Log("[SwiftDemoTest] m_AvatarObj" + m_AvatarObj);
        m_AvatarObj.SetActive(true);

        if (m_EnablePP)
            SetModelLayer(m_AvatarObj, m_AvatarObj.layer == 0 ? LAYER_PLAYER_PLAYER : LAYER_DEFAULT);

        DemoProcessState = DemoProcess.PLAYING;
        loadavatar = true;
    }

    /// <summary>
    /// Set Avatar layer to hide/show the avatar from camera's view.
    /// </summary>
    /// <param name="modelObj"></param>
    /// <param name="layer"></param>
    private void SetModelLayer(GameObject modelObj, int layer)
    {
        if (modelObj != null)
        {
            modelObj.layer = layer;
            
            if (modelObj.transform.childCount > 0)
            {
                for (int i = 0; i < modelObj.transform.childCount; i++)
                {
                    SetModelLayer(modelObj.transform.GetChild(i).gameObject, layer);
                }
            }
        }
    }
    

    /// <summary>
    /// Enable/Disable Swift Algorithm post processing dynamically.
    /// No permission, give up
    /// </summary>
    /// <param name="active"></param>
    private void EnablePPAlgorithm(bool active)
    {
        using (AndroidJavaClass cls = new AndroidJavaClass("android.os.SystemProperties"))
        {
            string value = active ? "1" : "0";
            cls.CallStatic("set", " persist.pxr.picoSwiftTracking.EnablePP", value);
        }
    }
    #endregion

    public void CalibrateMotionTracker()
    {
        DemoProcessState = DemoProcessController.DemoProcess.CALIBRATING;
        //Launch Swift Calibration App
        if (PlayerGender == 0)
        {
            UpdateAlgorithmAvatarSkeletonLenthXMLFemale();
        }
        else
        {
            UpdateAlgorithmAvatarSkeletonLenthXML();
        }
        UpdateAlgorithmHeightXML();
        UpdateAlgorithmPPXML();
        UpdateAdjustment();
        PXR_Input.OpenFitnessBandCalibrationAPP();
    }

    private void UpdateAlgorithmHeightXML()
    {
        string path = "/sdcard/AlgSwift/human_skeleton_model.xml";
        if (File.Exists(path))
        {
            Debug.Log("Update Algorithm XML");
            XmlDocument xml = new XmlDocument();
            xml.Load(path);
            XmlNodeList xmlNodeList = xml.SelectSingleNode("HumanSkeleton").ChildNodes;
            foreach (XmlElement xmlNode in xmlNodeList)
            {
                if (xmlNode.Name == "TotalHeight")
                {
                    //float height = float.Parse(InputHeight.GetComponent<TMP_Text>().text) / 100.0f;
                    xmlNode.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 100f).ToString());
                }
                if (xmlNode.Name == "AdjustmentHeight")
                {
                    xmlNode.SetAttribute("value", (0.03
                     * (DemoProcessController.Instance.PlayerHeight / 100f)).ToString());
                }
            }
            xml.Save(path);
        }
        else
        {
            Debug.Log("[SwiftDemoTest] Create XML script");
            //If xml script doesn't exist, create a new one
            XmlDocument xml = new XmlDocument();
            xml.AppendChild(xml.CreateXmlDeclaration("1.0", "UTF-8", null));
            xml.AppendChild(xml.CreateElement("HumanSkeleton"));
            XmlNode rootNode = xml.SelectSingleNode("HumanSkeleton");
            XmlElement element = xml.CreateElement("TotalHeight"); // unit: M
            element.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 100f).ToString());
            rootNode.AppendChild(element);

            element = xml.CreateElement("HeadToChin");
            element.SetAttribute("value", "0.2");
            rootNode.AppendChild(element);

            element = xml.CreateElement("AdjustmentHeight"); //unit: M
            element.SetAttribute("value", (0.03 * (DemoProcessController.Instance.PlayerHeight / 100f)).ToString());
            rootNode.AppendChild(element);

            xml.Save(path);

        }
    }

    private void UpdateAlgorithmAvatarSkeletonLenthXML()
    {
        string path = "/sdcard/AlgSwift/avatar_skeleton_model.xml";
        if (File.Exists(path))
        {
            Debug.Log("Update Algorithm XML");
            XmlDocument xml = new XmlDocument();
            xml.Load(path);
            XmlNodeList xmlNodeList = xml.SelectSingleNode("AvatarSkeleton").ChildNodes;
            foreach (XmlElement xmlNode in xmlNodeList)
            {
                if (xmlNode.Name == "HeadLen")
                {
                    //float height = float.Parse(InputHeight.GetComponent<TMP_Text>().text) / 100.0f;
                    xmlNode.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.2f).ToString());
                }
                if (xmlNode.Name == "NeckLen")
                {
                    xmlNode.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.0730167f).ToString());
                }
                if (xmlNode.Name == "TorsoLen")
                {
                    xmlNode.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.4004313f).ToString());
                }                
                if (xmlNode.Name == "HipLen")
                {
                    xmlNode.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.05436245f).ToString());
                }                
                if (xmlNode.Name == "UpperLegLen")
                {
                    xmlNode.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.4150999f).ToString());
                }                
                if (xmlNode.Name == "LowerLegLen")
                {
                    xmlNode.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.4741815f).ToString());
                }                
                if (xmlNode.Name == "FootLen")
                {
                    xmlNode.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.129468f).ToString());
                }                
                if (xmlNode.Name == "ShoulderLen")
                {
                    xmlNode.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.241329f).ToString());
                }
                if (xmlNode.Name == "UpperArmLen")
                {
                    xmlNode.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.213491f).ToString());
                }
                if (xmlNode.Name == "LowerArmLen")
                {
                    xmlNode.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.2100582f).ToString());
                }
                if (xmlNode.Name == "HandLen")
                {
                    xmlNode.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.169f).ToString());
                }
            }
            xml.Save(path);
        }
        else
        {
            Debug.Log("[SwiftDemoTest] Create XML script");
            //If xml script doesn't exist, create a new one
            XmlDocument xml = new XmlDocument();
            xml.AppendChild(xml.CreateXmlDeclaration("1.0", "UTF-8", null));
            xml.AppendChild(xml.CreateElement("AvatarSkeleton"));
            XmlNode rootNode = xml.SelectSingleNode("AvatarSkeleton");
            XmlElement element = xml.CreateElement("HeadLen"); // unit: M
            element.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.2f).ToString());
            rootNode.AppendChild(element);

            element = xml.CreateElement("NeckLen"); // unit: M
            element.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.0730167f).ToString());
            rootNode.AppendChild(element);

            element = xml.CreateElement("TorsoLen"); // unit: M
            element.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.4004313f).ToString());
            rootNode.AppendChild(element);

            element = xml.CreateElement("HipLen"); // unit: M
            element.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.05436245f).ToString());
            rootNode.AppendChild(element);

            element = xml.CreateElement("UpperLegLen"); // unit: M
            element.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.4150999f).ToString());
            rootNode.AppendChild(element);

            element = xml.CreateElement("LowerLegLen"); // unit: M
            element.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.4741815f).ToString());
            rootNode.AppendChild(element);

            element = xml.CreateElement("FootLen"); // unit: M
            element.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.129468f).ToString());
            rootNode.AppendChild(element);

            element = xml.CreateElement("ShoulderLen"); // unit: M
            element.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.241329f).ToString());
            rootNode.AppendChild(element);

            element = xml.CreateElement("UpperArmLen"); // unit: M
            element.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.213491f).ToString());
            rootNode.AppendChild(element);

            element = xml.CreateElement("LowerArmLen"); // unit: M
            element.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.2100582f).ToString());
            rootNode.AppendChild(element);

            element = xml.CreateElement("HandLen"); // unit: M
            element.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.169f).ToString());
            rootNode.AppendChild(element);



            xml.Save(path);

        }
    }


        private void UpdateAlgorithmAvatarSkeletonLenthXMLFemale()
    {
        string path = "/sdcard/AlgSwift/avatar_skeleton_model.xml";
        if (File.Exists(path))
        {
            Debug.Log("Update Algorithm XML");
            XmlDocument xml = new XmlDocument();
            xml.Load(path);
            XmlNodeList xmlNodeList = xml.SelectSingleNode("AvatarSkeleton").ChildNodes;
            foreach (XmlElement xmlNode in xmlNodeList)
            {
                if (xmlNode.Name == "HeadLen")
                {
                    //float height = float.Parse(InputHeight.GetComponent<TMP_Text>().text) / 100.0f;
                    xmlNode.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.2f).ToString());
                }
                if (xmlNode.Name == "NeckLen")
                {
                    xmlNode.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.09392913f).ToString());
                }
                if (xmlNode.Name == "TorsoLen")
                {
                    xmlNode.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.4084449f).ToString());
                }                
                if (xmlNode.Name == "HipLen")
                {
                    xmlNode.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.06820124f).ToString());
                }                
                if (xmlNode.Name == "UpperLegLen")
                {
                    xmlNode.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.4027063f).ToString());
                }                
                if (xmlNode.Name == "LowerLegLen")
                {
                    xmlNode.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.4592273f).ToString());
                }                
                if (xmlNode.Name == "FootLen")
                {
                    xmlNode.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.1380634f).ToString());
                }                
                if (xmlNode.Name == "ShoulderLen")
                {
                    xmlNode.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.2925628f).ToString());
                }
                if (xmlNode.Name == "UpperArmLen")
                {
                    xmlNode.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.2448786f).ToString());
                }
                if (xmlNode.Name == "LowerArmLen")
                {
                    xmlNode.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.2062476f).ToString());
                }
                if (xmlNode.Name == "HandLen")
                {
                    xmlNode.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.169f).ToString());
                }
            }
            xml.Save(path);
        }
        else
        {
            Debug.Log("[SwiftDemoTest] Create XML script");
            //If xml script doesn't exist, create a new one
            XmlDocument xml = new XmlDocument();
            xml.AppendChild(xml.CreateXmlDeclaration("1.0", "UTF-8", null));
            xml.AppendChild(xml.CreateElement("AvatarSkeleton"));
            XmlNode rootNode = xml.SelectSingleNode("AvatarSkeleton");
            XmlElement element = xml.CreateElement("HeadLen"); // unit: M
            element.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.2f).ToString());
            rootNode.AppendChild(element);

            element = xml.CreateElement("NeckLen"); // unit: M
            element.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.09392913f).ToString());
            rootNode.AppendChild(element);

            element = xml.CreateElement("TorsoLen"); // unit: M
            element.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.4084449f).ToString());
            rootNode.AppendChild(element);

            element = xml.CreateElement("HipLen"); // unit: M
            element.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.06820124f).ToString());
            rootNode.AppendChild(element);

            element = xml.CreateElement("UpperLegLen"); // unit: M
            element.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.4027063f).ToString());
            rootNode.AppendChild(element);

            element = xml.CreateElement("LowerLegLen"); // unit: M
            element.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.4592273f).ToString());
            rootNode.AppendChild(element);

            element = xml.CreateElement("FootLen"); // unit: M
            element.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.1380634f).ToString());
            rootNode.AppendChild(element);

            element = xml.CreateElement("ShoulderLen"); // unit: M
            element.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.2925628f).ToString());
            rootNode.AppendChild(element);

            element = xml.CreateElement("UpperArmLen"); // unit: M
            element.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.2448786f).ToString());
            rootNode.AppendChild(element);

            element = xml.CreateElement("LowerArmLen"); // unit: M
            element.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.2062476f).ToString());
            rootNode.AppendChild(element);

            element = xml.CreateElement("HandLen"); // unit: M
            element.SetAttribute("value", (DemoProcessController.Instance.PlayerHeight / 170f * 0.169f).ToString());
            rootNode.AppendChild(element);



            xml.Save(path);

        }
    }

    /// <summary>
    /// Temp method for debugging Avatar height adjustment value
    /// </summary>
    private void UpdateAdjustment()
    {
        string path = "/sdcard/AlgSwift/human_skeleton_model.xml";
        if (File.Exists(path))
        {
            Debug.Log("Update Algorithm XML");
            XmlDocument xml = new XmlDocument();
            xml.Load(path);
            XmlNodeList xmlNodeList = xml.SelectSingleNode("HumanSkeleton").ChildNodes;
            foreach (XmlElement xmlNode in xmlNodeList)
            {
                if (xmlNode.Name == "AdjustmentHeight")
                {
                    m_DiffPosition = new Vector3(0, float.Parse(xmlNode.GetAttribute("value")), 0);
                }
            }
        }
    }

    private void UpdateAlgorithmPPXML()
    {
        string path = "/sdcard/AlgSwift/PPStatus.xml";
        string value = m_EnablePP ? "1":"0";
        if (File.Exists(path))
        {
            Debug.Log("Update Algorithm PP XML");
            XmlDocument xml = new XmlDocument();
            xml.Load(path);
            XmlNodeList xmlNodeList = xml.SelectSingleNode("SwiftAlgConfig").ChildNodes;
            foreach (XmlElement xmlNode in xmlNodeList)
            {
                if (xmlNode.Name == "PPStatus")
                {   
                    xmlNode.SetAttribute("value", value);
                }
                if (xmlNode.Name == "SwiftMode")
                {   
                    xmlNode.SetAttribute("value", "0");
                }
            }
            xml.Save(path);
        }
        else
        {
            Debug.Log("[SwiftDemoTest] Create PP XML script");
            //If xml script doesn't exist, create a new one
            XmlDocument xml = new XmlDocument();
            xml.AppendChild(xml.CreateXmlDeclaration("1.0", "UTF-8", null));
            xml.AppendChild(xml.CreateElement("SwiftAlgConfig"));
            XmlNode rootNode = xml.SelectSingleNode("SwiftAlgConfig");
            XmlElement element = xml.CreateElement("PPStatus"); // unit: M
            element.SetAttribute("value", value);
            rootNode.AppendChild(element);

            element = xml.CreateElement("SwiftMode"); // unit: M
            element.SetAttribute("value", "0");
            rootNode.AppendChild(element);

            xml.Save(path);

        }
    }

    private int[,] m_Betweens = { { 0, 1 },
                                  { 0, 2 },
                                  { 1,4 },
                                  { 4,7 },
                                  { 7,10 },
                                  { 2,5 },
                                  { 5,8 },
                                  { 8,11 },
                                  { 0,3 },
                                  { 3,6 },
                                  { 6,9 },
                                  { 9,12 },
                                  { 12,15 },
                                  { 9, 13 },
                                  { 13,16 },
                                  { 16,18 },
                                  { 18,20},
                                  { 9,14},
                                  { 14,17},
                                  { 17,19},
                                  { 19,21} };

    private void DrawLineBetweenNodes()
    {
        for (int i = 0; i < Lines.Length; i++)
        {
            Lines[i].SetPosition(0, SkeletonNodes[m_Betweens[i, 0]].position);
            Lines[i].SetPosition(1, SkeletonNodes[m_Betweens[i, 1]].position);
        }
    }

}
