using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BodyTrackingDemo
{
    public class UIDisplaySetting : MonoBehaviour
    {
        public Button btnAlignGround;
        public TMP_Dropdown dropdownSteppingEffect;
        public TMP_Dropdown dropdownMirror;

        private void Awake()
        {
            btnAlignGround.onClick.AddListener(OnAlignGround);
            dropdownSteppingEffect.onValueChanged.AddListener(OnSteppingEffectChanged);
            dropdownMirror.onValueChanged.AddListener(OnMirrorChanged);
        }

        private void OnAlignGround()
        {
            LegTrackingModeSceneManager.Instance.AlignGround();
        }
        
        private void Start()
        {
            dropdownSteppingEffect.value = PlayerPrefManager.Instance.PlayerPrefData.steppingEffect;
            dropdownMirror.value = PlayerPrefManager.Instance.PlayerPrefData.mirrorMode;
        }

        private void OnSteppingEffectChanged(int value)
        {
            PlayerPrefManager.Instance.PlayerPrefData.bodyTrackMode = value;
            Debug.Log($"LegTrackingModeUIManager.OnSteppingEffectChanged: value = {value}");
        }
        
        private void OnMirrorChanged(int value)
        {
            PlayerPrefManager.Instance.PlayerPrefData.mirrorMode = value;
            if (UIManager.Instance != null)
            {
                UIManager.Instance.SetLazyFollowEnable(value == 1);
            }
            Debug.Log($"LegTrackingModeUIManager.OnMirrorChanged: value = {value}");
        }
    }
}