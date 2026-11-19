using UnityEngine;
using UnityEngine.InputSystem;

namespace Levo.Player
{
    /// <summary>Mouse look. Yaw rotates the player body, pitch rotates the camera pivot.</summary>
    [RequireComponent(typeof(PlayerInputReader))]
    public class PlayerLook : MonoBehaviour
    {
        [SerializeField] Transform cameraPivot;
        [SerializeField] float sensitivity = 0.1f;   // degrees per mouse pixel
        [SerializeField] float pitchLimit = 89f;

        PlayerInputReader input;
        float pitch;

        /// <summary>Settings stage will change this; clamped so bad values can't break the game.</summary>
        public float Sensitivity
        {
            get => sensitivity;
            set => sensitivity = Mathf.Clamp(value, 0.01f, 1f);
        }

        void Awake() => input = GetComponent<PlayerInputReader>();

        void Start() => SetCursorLocked(true);

        void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                SetCursorLocked(false);
                return;
            }

            if (Cursor.lockState != CursorLockMode.Locked)
            {
                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                    SetCursorLocked(true);
                return;
            }

            Vector2 delta = input.Look * sensitivity;
            transform.Rotate(Vector3.up, delta.x);
            pitch = Mathf.Clamp(pitch - delta.y, -pitchLimit, pitchLimit);
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        static void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
