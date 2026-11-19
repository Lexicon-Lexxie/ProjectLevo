using UnityEngine;
using UnityEngine.InputSystem;

namespace Levo.Player
{
    /// <summary>
    /// The ONLY class that touches input devices. Other scripts read from here,
    /// so keybinds (Settings stage) and keyboard-only support (Access stage)
    /// can be changed in one place.
    /// </summary>
    public class PlayerInputReader : MonoBehaviour
    {
        InputAction moveAction, lookAction, jumpAction, grabAction, throwAction;

        public Vector2 Move => moveAction.ReadValue<Vector2>();
        public Vector2 Look => lookAction.ReadValue<Vector2>();
        public bool JumpPressed => jumpAction.WasPressedThisFrame();
        public bool JumpHeld => jumpAction.IsPressed();
        public bool GrabPressed => grabAction.WasPressedThisFrame();
        public bool ThrowPressed => throwAction.WasPressedThisFrame();

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
            throwAction = new InputAction("Throw", InputActionType.Button, "<Mouse>/leftButton");
        }

        void OnEnable()
        {
            moveAction.Enable(); lookAction.Enable(); jumpAction.Enable();
            grabAction.Enable(); throwAction.Enable();
        }

        void OnDisable()
        {
            moveAction.Disable(); lookAction.Disable(); jumpAction.Disable();
            grabAction.Disable(); throwAction.Disable();
        }

        void OnDestroy()
        {
            moveAction?.Dispose(); lookAction?.Dispose(); jumpAction?.Dispose();
            grabAction?.Dispose(); throwAction?.Dispose();
        }
    }
}
