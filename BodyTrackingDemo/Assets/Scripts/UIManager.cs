using UnityEngine;

namespace BodyTrackingDemo
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        private SnapFollow _lazyFollow;

        private void Awake()
        {
            Instance = this;
            _lazyFollow = GetComponent<SnapFollow>();
        }

        public void Start()
        {
            _lazyFollow.enabled = PlayerPrefManager.Instance.PlayerPrefData.mirrorMode == 1;
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public void SetLazyFollowEnable(bool value)
        {
            _lazyFollow.enabled = value;
            if (!value)
            {
                transform.localPosition = Vector3.zero;
            }
        }
    }
}