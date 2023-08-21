using System.Collections;
using System.Collections.Generic;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static System.Net.WebRequestMethods;
using static Unity.XR.PXR.PXR_Plugin;

public class Test : MonoBehaviour
{
    private SimpleSample sample;
    private string content;

    private List<Quaternion> mOriginBonesRotList = new List<Quaternion>(new Quaternion[(int)BodyTrackerRole.ROLE_NUM]);
    private List<FrameData> mFrameDataList = new List<FrameData>();

    private int mFrameCount = 0;

    private Matrix4x4 mTransMatrix;

    private Quaternion mTmpRotationLeft;
    private Matrix4x4 mTmpMatrixRight;

    private Vector3 mMainHipPos;
    private Vector3 mDiffHipPos;



    void Start()
    {
        sample = GetComponent<SimpleSample>();

        content = Resources.Load<TextAsset>("SwiftAI").text;

        string[] frameArray = content.Split('#');

        for (int ij = 0; ij < frameArray.Length; ij++)
        {
            FrameData frameData = new FrameData();
            frameData.boneDataArray = new BoneData[24];

            string[] lineArray = frameArray[ij].Split('\n');

            for (int i = 0; i < lineArray.Length; i++)
            {
                BoneData boneData;

                lineArray[i] = lineArray[i].Trim();
                if (!string.IsNullOrEmpty(lineArray[i]))
                {
                    string[] itemArray = lineArray[i].Split('|');

                    int index = int.Parse(itemArray[0]);
                    boneData.boneId = index;

                    Vector3 pos;
                    string[] posArray = itemArray[1].Split(',');
                    pos.x = float.Parse(posArray[0]);
                    pos.y = float.Parse(posArray[1]);
                    pos.z = float.Parse(posArray[2]);
                    boneData.position = pos;

                    Quaternion rot;
                    string[] rotArray = itemArray[2].Split(',');
                    rot.x = float.Parse(rotArray[0]);
                    rot.y = float.Parse(rotArray[1]);
                    rot.z = float.Parse(rotArray[2]);
                    rot.w = float.Parse(rotArray[3]);
                    boneData.rotation = rot;

                    frameData.boneDataArray[index] = boneData;
                }
            }
            mFrameDataList.Add(frameData);
        }

        Debug.Log("frame data list count: " + mFrameDataList.Count);

        for (int i = 0; i < sample.BonesList.Count; i++)
        {
            if (sample.BonesList[i] != null)
            {
                mOriginBonesRotList[i] = sample.BonesList[i].rotation;
            }
        }

        Vector3 mMainCamPosition = Camera.main.transform.position;
        Vector3 mModelEyePosition = new Vector3(0, 1.41f, 0.01f);
        Vector3 mModelHipPosition = sample.BonesList[0].position;
        Vector3 mModelEyeToHip = mModelHipPosition - mModelEyePosition;
        mMainHipPos = mMainCamPosition + mModelEyeToHip;
    }

    private void Update()
    {
        if (mFrameCount < mFrameDataList.Count)
        {
            if (mDiffHipPos == Vector3.zero)
            {
                mDiffHipPos = mMainHipPos - mFrameDataList[mFrameCount].boneDataArray[0].position;
            }

            sample.BonesList[0].position = mFrameDataList[mFrameCount].boneDataArray[0].position + mDiffHipPos;

            for (int i = 0; i < sample.BonesList.Count; i++)
            {
                if (sample.BonesList[i] != null)
                {
                    sample.BonesList[i].rotation = mFrameDataList[mFrameCount].boneDataArray[i].rotation;

                }
            }

            mFrameCount++;
        }
        else
        {
            Debug.LogError("data end!");
            mFrameCount = 0;
        }

        #region keyboard control

        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            mFrameCount -= 350;
            if (mFrameCount < 0)
            {
                mFrameCount = 0;
            }
        }
        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            mFrameCount += 350;
            if (mFrameCount >= mFrameDataList.Count)
            {
                mFrameCount -= 350;
                Debug.LogError("Stop it! The end is coming!");
            }
        }


        #endregion
    }

}


public struct FrameData
{
    public BoneData[] boneDataArray;
}

public struct BoneData
{
    public int boneId;
    public Vector3 position;
    public Quaternion rotation;
}