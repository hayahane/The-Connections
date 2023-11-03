using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerController
{
    [RequireComponent(typeof(PlayerInput))]
    public class InputController : MonoBehaviour
    {
        private PlayerInput _playerInput;
        public bool IsUsingMouse => _playerInput.currentControlScheme == "KeyboardMouse";

        public Transform CameraTransform;
        public PlayerController PlayerController;
        public CameraController CamControl;
        public ConnectionScanner Scanner;

        private bool _isMouseLocked = true;

        private void OnEnable()
        {
            if (_playerInput == null) _playerInput = GetComponent<PlayerInput>();

            _playerInput.actions["Move"].performed += OnMoveInput;
            _playerInput.actions["Move"].canceled += OnMoveInput;

            _playerInput.actions["Jump"].started += OnJumpInput;
            _playerInput.actions["Jump"].canceled += OnJumpInput;

            _playerInput.actions["Aim"].performed += OnAimInput;
            _playerInput.actions["Aim"].canceled += OnAimInput;

            _playerInput.actions["Scan"].started += OnScanInput;

            _playerInput.actions["UnlockMouse"].performed += UnlockMouse;
        }

        private void OnDisable()
        {
            _playerInput.actions["Move"].performed -= OnMoveInput;
            _playerInput.actions["Move"].canceled -= OnMoveInput;

            _playerInput.actions["Jump"].performed -= OnJumpInput;
            _playerInput.actions["Jump"].canceled -= OnJumpInput;

            _playerInput.actions["Aim"].performed -= OnAimInput;
            _playerInput.actions["Aim"].canceled -= OnAimInput;

            _playerInput.actions["Scan"].started -= OnScanInput;
        }

        private void Update()
        {
            if (_isMouseLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        #region Input Callbacks

        private void OnMoveInput(InputAction.CallbackContext context)
        {
            if (context.canceled)
            {
                PlayerController.InputMoveDirection = Vector3.zero;
                return;
            }

            var inputRaw = context.ReadValue<Vector2>();
            PlayerController.InputMoveDirection = Quaternion.Euler(0, CameraTransform.eulerAngles.y, 0) *
                                                  new Vector3(inputRaw.x, 0, inputRaw.y);
        }

        private void OnJumpInput(InputAction.CallbackContext context)
        {
            if (context.canceled)
            {
                PlayerController.IsJumpPressed = false;
                return;
            }

            PlayerController.IsJumpPressed = true;
        }

        private void OnAimInput(InputAction.CallbackContext context)
        {
            if (context.canceled)
            {
                CamControl.AimInput = Vector2.zero;
                return;
            }

            CamControl.AimInput = context.ReadValue<Vector2>();
            CamControl.AimInput *= IsUsingMouse ? 1 : Time.deltaTime;
        }

        private void OnScanInput(InputAction.CallbackContext context)
        {
            Scanner.TriggerScanPulse();
        }

        private void UnlockMouse(InputAction.CallbackContext context)
        {
            
        }

        #endregion
    }
}