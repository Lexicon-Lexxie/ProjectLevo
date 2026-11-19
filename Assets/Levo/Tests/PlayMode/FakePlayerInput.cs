using UnityEngine;
using Levo.Core;

namespace Levo.Tests
{
    /// <summary>
    /// A pretend keyboard/mouse. Tests set values directly. Possible because of IPlayerInput.
    /// Button "pressed" events (grab, primary, cancel) are one-shot: call PressGrab() etc. and the press
    /// stays armed until a script reads it AFTER the press, then clears at the end of that frame. This way a test can
    /// never miss a press because of frame timing.
    /// </summary>
    public class FakePlayerInput : MonoBehaviour, IPlayerInput
    {
        public Vector2 Move { get; set; }
        public Vector2 Look { get; set; }
        public bool JumpPressed { get; set; }
        public bool JumpHeld { get; set; }

        bool grabArmed, primaryArmed, cancelArmed;
        bool grabRead, primaryRead, cancelRead;

        public bool GrabPressed { get { grabRead = true; return grabArmed; } }
        public bool PrimaryPressed { get { primaryRead = true; return primaryArmed; } }
        public bool CancelPressed { get { cancelRead = true; return cancelArmed; } }

        public void PressGrab() { grabArmed = true; grabRead = false; }
        public void PressPrimary() { primaryArmed = true; primaryRead = false; }
        public void PressCancel() { cancelArmed = true; cancelRead = false; }

        void LateUpdate()
        {
            if (grabRead) grabArmed = false;
            if (primaryRead) primaryArmed = false;
            if (cancelRead) cancelArmed = false;
            grabRead = primaryRead = cancelRead = false;
        }
    }
}
