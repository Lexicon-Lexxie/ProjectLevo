using UnityEngine;
using UnityEngine.InputSystem;
using Levo.Core;

namespace Levo.Player
{
    /// <summary>
    /// The ONLY class that touches input devices. Implements IPlayerInput so every other
    /// script stays independent of keyboards and mice. To change a key, edit it here only.
    /// </summary>
    public class PlayerInputReader : MonoBehaviour, IPlayerInput
    {
        InputAction moveAction, lookAction, jumpAction, grabAction, primaryAction, cancelAction;

        public Vector2 Move => moveAction.ReadValue<Vector2>();
        public Vector2 Look => lookAction.ReadValue<Vector2>();
        public bool JumpPressed => jumpAction.WasPressedThisFrame();
        public bool JumpHeld => jumpAction.IsPressed();
        public bool GrabPressed => grabAction.WasPressedThisFrame();
        public bool PrimaryPressed => primaryAction.WasPressedThisFrame();
        public bool CancelPressed => cancelAction.WasPressedThisFrame();

        void Awake()
        {
            moveAction = new InputAction("Move", InputActionType.Value);
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");

            lookAction = new InputAction("Look", InputActionType.Value, "<Mouse>/delta");
            jumpAction = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
            grabAction = new InputAction("Grab", InputActionType.Button, "<Keyboard>/e");
            primaryAction = new InputAction("Primary", InputActionType.Button, "<Mouse>/leftButton");
            cancelAction = new InputAction("Cancel", InputActionType.Button, "<Keyboard>/escape");
        }

        void OnEnable()
        {
            moveAction.Enable(); lookAction.Enable(); jumpAction.Enable();
            grabAction.Enable(); primaryAction.Enable(); cancelAction.Enable();
        }

        void OnDisable()
        {
            moveAction.Disable(); lookAction.Disable(); jumpAction.Disable();
            grabAction.Disable(); primaryAction.Disable(); cancelAction.Disable();
        }

        void OnDestroy()
        {
            moveAction?.Dispose(); lookAction?.Dispose(); jumpAction?.Dispose();
            grabAction?.Dispose(); primaryAction?.Dispose(); cancelAction?.Dispose();
        }
    }
}
