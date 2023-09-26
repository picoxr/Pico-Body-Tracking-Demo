using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace BodyTrackingDemo
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private GameObject leftHandRay;
        [SerializeField] private GameObject rightHandRay;
        [SerializeField] private InputActionReference leftGrip;
        [SerializeField] private InputActionReference rightGrip;

        private XRInteractorLineVisual _leftLineRenderer;
        private XRInteractorLineVisual _rightLineRenderer;
        private int _curRayMode;
        
        private void Awake()
        {
            _leftLineRenderer = leftHandRay.GetComponent<XRInteractorLineVisual>();
            _rightLineRenderer = rightHandRay.GetComponent<XRInteractorLineVisual>();
        }

        private void Start()
        {
            UpdateInteractionRay(PlayerPrefManager.Instance.PlayerPrefData.interactionRayMode);
        }

        private void OnRightGripStarted(InputAction.CallbackContext obj)
        {
            _rightLineRenderer.enabled = true;
        }
        
        private void OnRightGripCanceled(InputAction.CallbackContext obj)
        {
            _rightLineRenderer.enabled = false;
        }

        private void OnLeftGripStarted(InputAction.CallbackContext obj)
        {
            _leftLineRenderer.enabled = true;
        }

        private void OnLeftGripCanceled(InputAction.CallbackContext obj)
        {
            _leftLineRenderer.enabled = false;
        }

        public void UpdateInteractionRay(int rayMode)
        {
            switch (rayMode)
            {
                case 0:
                    leftGrip.action.started += OnLeftGripStarted;
                    leftGrip.action.canceled += OnLeftGripCanceled;
                    rightGrip.action.started += OnRightGripStarted;
                    rightGrip.action.canceled += OnRightGripCanceled;
                    _leftLineRenderer.enabled = false;
                    _rightLineRenderer.enabled = false;    
                    break;
                case 1:
                    _leftLineRenderer.enabled = true;
                    _rightLineRenderer.enabled = true;
                    if (_curRayMode == 0)
                    {
                        leftGrip.action.started -= OnLeftGripStarted;
                        leftGrip.action.canceled -= OnLeftGripCanceled;
                        rightGrip.action.started -= OnRightGripStarted;
                        rightGrip.action.canceled -= OnRightGripCanceled;
                    }
                    break;
            }

            _curRayMode = rayMode;
        }
    }
}