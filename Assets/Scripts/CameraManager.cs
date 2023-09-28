using UnityEngine;
using UnityEngine.UI;

namespace BodyTrackingDemo
{
    public class CameraManager : MonoBehaviour
    {
        public static CameraManager Instance { get; private set; }
        
        [SerializeField]
        private GameObject frontCamera;
        
        [SerializeField]
        private GameObject backCamera;
        
        [SerializeField]
        private RawImage screen;

        private bool _isDanceGamePlaying;
        private CameraStandMode _curCameraStandMode;

            
        private void Awake()
        {
            Instance = this;
            Events.DanceGameStart += OnDanceGameStart;
            Events.DanceGameStop += OnDanceGameStop;
        }

        private void Start()
        {
            SetCameraStandType((CameraStandMode) PlayerPrefManager.Instance.PlayerPrefData.cameraStandMode);
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        private void OnDanceGameStart()
        {
            _isDanceGamePlaying = true;
            if (_curCameraStandMode == CameraStandMode.Auto)
            {
                SetCameraStandType(CameraStandMode.Auto);
            }
        }
        
        private void OnDanceGameStop()
        {
            _isDanceGamePlaying = false;
            if (_curCameraStandMode == CameraStandMode.Auto)
            {
                SetCameraStandType(CameraStandMode.Auto);
            }
        }
        
        public void SetCameraStandType(CameraStandMode standMode)
        {
            _curCameraStandMode = standMode;
            switch (standMode)
            {
                case CameraStandMode.Auto:
                    backCamera.SetActive(_isDanceGamePlaying);
                    backCamera.GetComponent<SnapFollow>().enabled = false;
                    frontCamera.GetComponent<SnapFollow>().enabled = false;
                    screen.uvRect = _isDanceGamePlaying ? new Rect(0, 0, 1, 1) : new Rect(0, 0, -1, 1); 
                    break;
                case CameraStandMode.FixedFront:
                    backCamera.SetActive(false);
                    frontCamera.GetComponent<SnapFollow>().enabled = false;
                    screen.uvRect = new Rect(0, 0, -1, 1); 
                    break;
                case CameraStandMode.FollowingFront:
                    backCamera.SetActive(false);
                    frontCamera.GetComponent<SnapFollow>().enabled = true;
                    screen.uvRect = new Rect(0, 0, -1, 1); 
                    break;
                case CameraStandMode.FixedBack:
                    backCamera.SetActive(true);
                    backCamera.GetComponent<SnapFollow>().enabled = false;
                    screen.uvRect = new Rect(0, 0, 1, 1); 
                    break;
                case CameraStandMode.FollowingBack:
                    backCamera.SetActive(true);
                    backCamera.GetComponent<SnapFollow>().enabled = true;
                    screen.uvRect = new Rect(0, 0, 1, 1); 
                    break;
                default:
                    backCamera.SetActive(false);
                    break;
            }
            frontCamera.SetActive(true);
        }
    }

    public enum CameraStandMode
    {
        Auto,
        FixedFront,
        FixedBack,
        FollowingFront,
        FollowingBack,
    }
}