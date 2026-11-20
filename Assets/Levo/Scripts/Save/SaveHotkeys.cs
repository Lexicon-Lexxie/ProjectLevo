using UnityEngine;
using Levo.Core;

namespace Levo.Save
{
    /// <summary>F5 = quick save, F9 = quick load. Shows the result on screen for a few seconds.</summary>
    public class SaveHotkeys : MonoBehaviour
    {
        [SerializeField] SaveSystem saveSystem;

        IPlayerInput input;
        string message = "";
        float messageUntil;
        GUIStyle style;

        void Awake()
        {
            input = GetComponent<IPlayerInput>();
            if (saveSystem == null) saveSystem = FindFirstObjectByType<SaveSystem>();
            if (input == null || saveSystem == null)
            {
                Debug.LogError("SaveHotkeys needs an IPlayerInput component and a SaveSystem in the scene.", this);
                enabled = false;
                return;
            }
            saveSystem.StatusChanged += OnStatus;
        }

        void OnDestroy()
        {
            if (saveSystem != null) saveSystem.StatusChanged -= OnStatus;
        }

        void Update()
        {
            if (input.QuickSavePressed) saveSystem.Save();
            else if (input.QuickLoadPressed) saveSystem.Load();
        }

        void OnStatus(string text)
        {
            message = text;
            messageUntil = Time.unscaledTime + 4f;
        }

        void OnGUI()
        {
            if (Time.unscaledTime > messageUntil) return;
            if (style == null) style = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold };
            GUI.Label(new Rect(20f, 20f, 900f, 40f), message, style);
        }
    }
}
