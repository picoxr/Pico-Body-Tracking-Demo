using UnityEngine;
using Unity.XR.PXR;
using UnityEngine.UI;

public class FitnessBandTest : MonoBehaviour
{
    public Text Text1;
    PxrFitnessBandConnectState pxrFitnessBand = new PxrFitnessBandConnectState();
    // Start is called before the first frame update
    void Start()
    {
        PXR_Plugin.System.FitnessBandElectricQuantity += LowPower;
        PXR_Plugin.System.FitnessBandNumberOfConnections += ConnectionState;
        PXR_Plugin.System.FitnessBandAbnormalCalibrationData += Calibration;

    }

    public void GetFitnessBandConnectState()
    {
        Debug.Log("LLRR:1GetFitnessBandConnectState");
        int state = PXR_Input.GetFitnessBandConnectState(ref pxrFitnessBand);
        Debug.Log("LLRR:2GetFitnessBandConnectState");
        Text1.text = pxrFitnessBand.num.ToString() + "   ";
        Debug.Log("LLRR:3GetFitnessBandConnectState");
        for (int i = 0; i < 2; i++)
        {
            unsafe
            {
                Text1.text = $"{Text1.text}{pxrFitnessBand.trackerID[i]}  ";
            }
        }
        Debug.Log("LLRR:4GetFitnessBandConnectState");
    }

    public void GetFitnessBandBattery()
    {
        unsafe
        {
            int battery = 0;
            PXR_Input.GetFitnessBandBattery(pxrFitnessBand.trackerID[0], ref battery);
            Text1.text = battery.ToString();
        }
    }

    public void GetFitnessBandCalibState()
    {
        int calibrated = 99;
        PXR_Input.GetFitnessBandCalibState(ref calibrated);
        Text1.text = calibrated.ToString();
    }

    public void OpenSwiftCalibrationAPP()
    {
        PXR_Input.OpenFitnessBandCalibrationAPP();
    }

    public void LowPower(int id,int battery)
    {
        Text1.text = $"{id}   {battery}";
    }

    public void ConnectionState(int state, int count) {
        Text1.text = $"{state}  {count}";
    }

    public void Calibration(int a, int b) {
        Debug.Log("LLRR jiaozhun");
        Text1.text = "Calibration";
    }
}
