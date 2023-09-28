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
        public TMP_Dropdown dropdownInterationRay;
        public Player player;

        private void Awake()
        {
            btnAlignGround.onClick.AddListener(OnAlignGround);
            dropdownSteppingEffect.onValueChanged.AddListener(OnSteppingEffectChanged);
            dropdownMirror.onValueChanged.AddListener(OnMirrorChanged);
            dropdownInterationRay.onValueChanged.AddListener(OnInterationRayChanged);
        }

        private void OnAlignGround()
        {
            BodyTrackingManager.Instance.AlignGround();
        }
        
        private void Start()
        {
            dropdownSteppingEffect.value = PlayerPrefManager.Instance.PlayerPrefData.steppingEffect;
            dropdownMirror.value = PlayerPrefManager.Instance.PlayerPrefData.cameraStandMode;
            dropdownInterationRay.value = PlayerPrefManager.Instance.PlayerPrefData.interactionRayMode;
        }

        private void OnSteppingEffectChanged(int value)
        {
            PlayerPrefManager.Instance.PlayerPrefData.steppingEffect = value;
            Debug.Log($"UIDisplaySetting.OnSteppingEffectChanged: value = {value}");
        }
        
        private void OnMirrorChanged(int value)
        {
            PlayerPrefManager.Instance.PlayerPrefData.cameraStandMode = value;
            CameraManager.Instance.SetCameraStandType((CameraStandMode)value);
            Debug.Log($"UIDisplaySetting.OnMirrorChanged: value = {value}");
        }
        
        private void OnInterationRayChanged(int value)
        {
            PlayerPrefManager.Instance.PlayerPrefData.interactionRayMode = value;
            player.UpdateInteractionRay(value);
            Debug.Log($"UIDisplaySetting.OnInterationRayChanged: value = {value}");
        }
    }
}