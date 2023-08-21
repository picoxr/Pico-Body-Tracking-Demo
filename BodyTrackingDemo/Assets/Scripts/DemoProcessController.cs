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
    public GameObject SkeletonText;


    
    public Transform[] SkeletonNodes = new Transform[22];
    public LineRenderer[] Lines = new LineRenderer[21];

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
        Debug.Log("[SwiftDemoTest]Swift sample start");
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

        //For testing Load Avatar
        //LoadAvatar();
        //MirrorObj.SetActive(true);
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
                        CalibrateMotionTracker();
                    }
                }
                else
                {
                    m_SecondaryButtonDown = true;   
                }

                if (InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.primaryButton, out bool left_A) && left_A)
                {
                    if (m_PrimaryButtonDown)
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

    //private void DemoSceneInit()
    //{
    //    // set view position
    //    if (m_ViewPositionList.Count == 0)
    //    {
    //        m_ViewPositionList.Add(GetViewPosition(ViewDirection.Back, m_ViewRadius));
    //        m_ViewPositionList.Add(GetViewPosition(ViewDirection.Right, m_ViewRadius));
    //        m_ViewPositionList.Add(GetViewPosition(ViewDirection.Right_Forward, m_ViewRadius));
    //        m_ViewPositionList.Add(GetViewPosition(ViewDirection.Forward, m_ViewRadius));
    //        m_ViewPositionList.Add(GetViewPosition(ViewDirection.Left_Forward, m_ViewRadius));
    //        m_ViewPositionList.Add(GetViewPosition(ViewDirection.Left, m_ViewRadius));
    //        Debug.Log("view positions: " + string.Join(",", m_ViewPositionList));

    //        if (mModelsArray == null || mModelsArray.Length == 0)
    //        {
    //            mModelsArray = Resources.LoadAll<GameObject>("Models");
    //            Debug.Log("all models >  count: " + mModelsArray.Length + " items: " + string.Join<GameObject>(",", mModelsArray));
    //        }

    //        m_ViewIndex = 0;
    //        RenderCameraObj.transform.position = m_ViewPositionList[m_ViewIndex];
    //        RenderCameraObj.transform.rotation = Quaternion.identity;
    //        // show mirror
    //        if (MirrorObj != null)
    //        {
    //            MirrorObj.SetActive(true);
    //        }
    //        mModelIndex = 0;
    //        if (m_AvatarObj == null)
    //        {
    //            m_AvatarObj = LoadModel(mModelIndex);
    //        }
    //        // set model layer
    //        SetModelLayer(m_AvatarObj, LAYER_PLAYER_MIRROR);

    //        SetControllersActive(true);
    //    }
    //}

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

    //#region Change model

    //private Vector3 GetViewPosition(ViewDirection dir, float radius)
    //{
    //    if (dir == ViewDirection.Back)
    //    {
    //        return new Vector3(0, m_CameraHeight, 0);
    //        //return Vector3.zero;
    //    }

    //    double radian = ((int)dir - 1) * Math.PI * 0.25f;
    //    double x = Math.Cos(radian) * radius;
    //    double z = Math.Sin(radian) * radius;
    //    Vector3 position;
    //    position.x = (float)x;
    //    position.y = m_CameraHeight;
    //    position.z = (float)z;
    //    return position;
    //}

    //private enum ViewDirection
    //{
    //    Back,
    //    Right,
    //    Right_Forward,
    //    Forward,
    //    Left_Forward,
    //    Left,
    //}

    

    //private void ChangeModel(int index)
    //{
    //    if (m_AvatarObj != null)
    //    {
    //        m_AvatarObj.SetActive(false);
    //    }
    //    m_AvatarObj = LoadModel(index);
    //    m_AvatarObj.SetActive(true);
    //}

    //#endregion



    #region Load Avatar

    //private GameObject[] mModelsArray;
    //private Dictionary<int, GameObject> mModelDic = new Dictionary<int, GameObject>();
    //private int mModelIndex = 0;

    //private GameObject LoadModel(int modelIndex)
    //{
    //    Debug.Log("load model: " + modelIndex);
    //    if (mModelDic.ContainsKey(modelIndex))
    //    {
    //        return mModelDic[modelIndex];
    //    }
    //    else
    //    {
    //        GameObject go = Instantiate(mModelsArray[modelIndex], Vector3.zero, Quaternion.identity);
    //        mModelDic.Add(modelIndex, go);
    //        return go;
    //    }
    //}

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
        string avatar_name = "ClothRun_175 Variant";
        Debug.Log("[SwiftDemoTest] Models/" + avatar_name);
        GameObject avatarObj = Resources.Load<GameObject>("Models/" + avatar_name);
        m_AvatarObj = Instantiate(avatarObj, Vector3.zero, Quaternion.identity);
        m_AvatarObj.transform.localScale = Vector3.one;
        Debug.Log("[SwiftDemoTest] m_AvatarObj" + m_AvatarObj);
        m_AvatarObj.SetActive(true);

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

    #endregion

    public void CalibrateMotionTracker()
    {
        DemoProcessState = DemoProcessController.DemoProcess.CALIBRATING;
        // Enable FULL BODY TRACKING MODE
        PXR_Input.SetSwiftMode(1);
        //Launch Swift Calibration App        
        PXR_Input.OpenFitnessBandCalibrationAPP();
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
