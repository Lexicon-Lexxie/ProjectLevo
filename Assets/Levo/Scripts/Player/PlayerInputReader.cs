using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Levo.Core;

namespace Levo.Player
{
    /// <summary>
    /// The ONLY class that touches input devices. Implements IPlayerInput so every other
    /// script stays independent of keyboards and mice. To change a key, edit it here only.
    /// </summary>
    public class PlayerInputReader : MonoBehaviour, IPlayerInput, IKeyBindings
    {
        InputAction moveAction, lookAction, jumpAction, grabAction, primaryAction, cancelAction,
                    quickSaveAction, quickLoadAction, settingsAction;
        Dictionary<string, InputAction> rebindable;

        public Vector2 Move => moveAction.ReadValue<Vector2>();
        public Vector2 Look => lookAction.ReadValue<Vector2>();
        public bool JumpPressed => jumpAction.WasPressedThisFrame();
        public bool JumpHeld => jumpAction.IsPressed();
        public bool GrabPressed => grabAction.WasPressedThisFrame();
        public bool PrimaryPressed => primaryAction.WasPressedThisFrame();
        public bool CancelPressed => cancelAction.WasPressedThisFrame();
        public bool QuickSavePressed => quickSaveAction.WasPressedThisFrame();
        public bool QuickLoadPressed => quickLoadAction.WasPressedThisFrame();
        public bool SettingsPressed => settingsAction.WasPressedThisFrame();

        // ---- IKeyBindings: the only place a key can be changed at runtime ----
        public string GetBinding(string action) =>
            rebindable != null && rebindable.TryGetValue(action, out InputAction a) ? a.bindings[0].effectivePath : null;

        public string GetDefaultBinding(string action) =>
            rebindable != null && rebindable.TryGetValue(action, out InputAction a) ? a.bindings[0].path : null;

        public bool TrySetBinding(string action, string path)
        {
            if (rebindable == null || string.IsNullOrEmpty(path) || !rebindable.TryGetValue(action, out InputAction a))
                return false;
            bool wasEnabled = a.enabled;
            if (wasEnabled) a.Disable();
            a.ApplyBindingOverride(0, path);
            if (wasEnabled) a.Enable();
            return true;
        }

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
            quickSaveAction = new InputAction("QuickSave", InputActionType.Button, "<Keyboard>/f5");
            quickLoadAction = new InputAction("QuickLoad", InputActionType.Button, "<Keyboard>/f9");
            settingsAction = new InputAction("Settings", InputActionType.Button, "<Keyboard>/tab");

            rebindable = new Dictionary<string, InputAction>
            {
                { BindingNames.Jump, jumpAction },
                { BindingNames.Grab, grabAction },
                { BindingNames.QuickSave, quickSaveAction },
                { BindingNames.QuickLoad, quickLoadAction }
            };
        }

        void OnEnable()
        {
            moveAction.Enable(); lookAction.Enable(); jumpAction.Enable();
            grabAction.Enable(); primaryAction.Enable(); cancelAction.Enable();
            quickSaveAction.Enable(); quickLoadAction.Enable(); settingsAction.Enable();
        }

        void OnDisable()
        {
            moveAction.Disable(); lookAction.Disable(); jumpAction.Disable();
            grabAction.Disable(); primaryAction.Disable(); cancelAction.Disable();
            quickSaveAction.Disable(); quickLoadAction.Disable(); settingsAction.Disable();
        }

        void OnDestroy()
        {
            moveAction?.Dispose(); lookAction?.Dispose(); jumpAction?.Dispose();
            grabAction?.Dispose(); primaryAction?.Dispose(); cancelAction?.Dispose();
            quickSaveAction?.Dispose(); quickLoadAction?.Dispose(); settingsAction?.Dispose();
        }
    }
}
