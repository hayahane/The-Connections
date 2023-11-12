using Character;
using Character.HUD;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.Serialization;

namespace Player
{
    [RequireComponent(typeof(PlayerInput))]
    public class InputController : MonoBehaviour
    {
        private PlayerInput _playerInput;
        public bool IsUsingMouse => _playerInput.currentControlScheme == "KeyboardMouse";

        public Transform CameraTransform;
        public PlayerCharacterController PlayerCharacter;
        public CameraController CamControl;
        public ConnectionScanner Scanner;
        public InstanceSelector Selector;

        private bool _isMouseLocked = true;

        private void OnEnable()
        {
            if (_playerInput == null) _playerInput = GetComponent<PlayerInput>();

            _playerInput.actions["Move"].performed += OnMoveInput;
            _playerInput.actions["Move"].canceled += OnMoveInput;

            _playerInput.actions["Sprint"].started += OnSprintInput;
            _playerInput.actions["Sprint"].canceled += OnSprintInput;

            _playerInput.actions["Jump"].started += OnJumpInput;
            _playerInput.actions["Jump"].canceled += OnJumpInput;

            _playerInput.actions["Aim"].performed += OnAimInput;
            _playerInput.actions["Aim"].canceled += OnAimInput;

            _playerInput.actions["Scan"].started += OnScanInput;

            _playerInput.actions["Attack"].performed += OnAttackInput;
            _playerInput.actions["Attack"].canceled += OnAttackInput;

            _playerInput.actions["Connect"].performed += OnConnectInput;

            _playerInput.actions["UnlockMouse"].performed += UnlockMouse;
        }

        private void OnDisable()
        {
            _playerInput.actions["Move"].performed -= OnMoveInput;
            _playerInput.actions["Move"].canceled -= OnMoveInput;
            
            _playerInput.actions["Sprint"].started -= OnSprintInput;
            _playerInput.actions["Sprint"].canceled -= OnSprintInput;

            _playerInput.actions["Jump"].performed -= OnJumpInput;
            _playerInput.actions["Jump"].canceled -= OnJumpInput;

            _playerInput.actions["Aim"].performed -= OnAimInput;
            _playerInput.actions["Aim"].canceled -= OnAimInput;

            _playerInput.actions["Scan"].started -= OnScanInput;

            _playerInput.actions["Attack"].performed -= OnAttackInput;
            _playerInput.actions["Attack"].canceled -= OnAttackInput;

            _playerInput.actions["Connect"].performed -= OnConnectInput;

            _playerInput.actions["UnlockMouse"].performed -= UnlockMouse;
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
                PlayerCharacter.InputMoveDirection = Vector3.zero;
                return;
            }

            var inputRaw = context.ReadValue<Vector2>();
            PlayerCharacter.InputMoveDirection = Quaternion.Euler(0, CameraTransform.eulerAngles.y, 0) *
                                                  new Vector3(inputRaw.x, 0, inputRaw.y);
        }

        private void OnSprintInput(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                PlayerCharacter.IsSprinting = true;
                return;
            }

            PlayerCharacter.IsSprinting = false;
        }

        private void OnJumpInput(InputAction.CallbackContext context)
        {
            if (context.canceled)
            {
                PlayerCharacter.IsJumpPressed = false;
                return;
            }

            PlayerCharacter.IsJumpPressed = true;
        }

        private void OnAimInput(InputAction.CallbackContext context)
        {
            if (context.canceled)
            {
                CamControl.AimInput = Vector2.zero;
                return;
            }

            CamControl.AimInput = context.ReadValue<Vector2>();
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            CamControl.AimInput *= IsUsingMouse ? 10 : Time.deltaTime;
#else
            CamControl.AimInput *= IsUsingMouse ? 1 : Time.deltaTime;
#endif
        }

        private void OnScanInput(InputAction.CallbackContext context)
        {
            Scanner.TriggerScanPulse();
        }

        private void OnAttackInput(InputAction.CallbackContext context)
        {
            if (context.canceled)
            {
                PlayerCharacter.IsAttacking = false;
                return;
            }

            PlayerCharacter.IsAttacking = true;
        }

        private void OnConnectInput(InputAction.CallbackContext context)
        {
            if (context.interaction is HoldInteraction)
            {
                Selector.ReleaseSourceAttribute();
                return;
            }

            if (context.interaction is PressInteraction)
            {
                Selector.ConnectToInstance();
                Selector.TakeSourceAttribute();
                return;
            }
        }

        private void UnlockMouse(InputAction.CallbackContext context)
        {
        }

        #endregion
    }
}