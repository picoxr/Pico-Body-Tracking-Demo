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
        
        private void Awake()
        {
            leftGrip.action.started += OnLeftGripStarted;
            leftGrip.action.canceled += OnLeftGripCanceled;
            rightGrip.action.started += OnRightGripStarted;
            rightGrip.action.canceled += OnRightGripCanceled;

            _leftLineRenderer = leftHandRay.GetComponent<XRInteractorLineVisual>();
            _rightLineRenderer = rightHandRay.GetComponent<XRInteractorLineVisual>();

            _leftLineRenderer.enabled = false;
            _rightLineRenderer.enabled = false;
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
    }
}