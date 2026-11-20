using UnityEngine;
using Levo.Core;
using Levo.Player;

namespace Levo.Settings
{
    /// <summary>
    /// A small on-screen settings window. Tab opens and closes it. Sliders and key buttons apply immediately;
    /// "Save" writes the file. Keys are chosen with the arrow buttons, so this class never reads a device.
    /// Put it on the Player, next to a SettingsManager.
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        [SerializeField] SettingsManager manager;

        IPlayerInput input;
        PlayerLook look;
        SettingsData draft;
        bool open;
        string message = "";

        void Awake()
        {
            input = GetComponent<IPlayerInput>();
            look = GetComponent<PlayerLook>();
            if (manager == null) manager = GetComponent<SettingsManager>();
            if (manager == null) manager = FindFirstObjectByType<SettingsManager>();
            if (input == null || manager == null)
            {
                Debug.LogError("SettingsPanel needs an IPlayerInput component and a SettingsManager in the scene.", this);
                enabled = false;
                return;
            }
            manager.StatusChanged += OnStatus;
        }

        void OnDestroy()
        {
            if (manager != null) manager.StatusChanged -= OnStatus;
        }

        void OnStatus(string text) => message = text;

        void Update()
        {
            if (!input.SettingsPressed) return;
            if (open) Close(); else Open();
        }

        void Open()
        {
            draft = manager.Current.Clone();
            open = true;
            if (look != null) look.SetPaused(true);
        }

        void Close()
        {
            open = false;
            if (look != null) look.SetPaused(false);
        }

        void Preview() => manager.TryApply(draft, out _);

        void OnGUI()
        {
            if (!open) return;

            GUILayout.BeginArea(new Rect(20f, 70f, 440f, 420f), GUI.skin.box);
            GUILayout.Label("Settings   (Tab to close)");

            GUILayout.Label("Mouse sensitivity: " + draft.sensitivity.ToString("0.00"));
            float s = GUILayout.HorizontalSlider(draft.sensitivity, SettingsRules.MinSensitivity, SettingsRules.MaxSensitivity);
            if (!Mathf.Approximately(s, draft.sensitivity)) { draft.sensitivity = s; Preview(); }

            GUILayout.Label("Volume: " + Mathf.RoundToInt(draft.volume * 100f) + "%");
            float v = GUILayout.HorizontalSlider(draft.volume, SettingsRules.MinVolume, SettingsRules.MaxVolume);
            if (!Mathf.Approximately(v, draft.volume)) { draft.volume = v; Preview(); }

            GUILayout.Space(8f);
            GUILayout.Label("Keys (use the arrows to pick a key)");
            foreach (string action in BindingNames.All) KeyRow(action);

            GUILayout.Space(8f);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save")) manager.Save();
            if (GUILayout.Button("Reset to defaults")) { manager.ResetToDefaults(); draft = manager.Current.Clone(); }
            if (GUILayout.Button("Close")) Close();
            GUILayout.EndHorizontal();

            GUILayout.Space(6f);
            GUILayout.Label(message);
            GUILayout.EndArea();
        }

        void KeyRow(string action)
        {
            string current = draft.GetPath(action);
            GUILayout.BeginHorizontal();
            GUILayout.Label(action, GUILayout.Width(120f));
            if (GUILayout.Button("<", GUILayout.Width(30f))) ChangeKey(action, current, -1);
            GUILayout.Label(SettingsRules.Friendly(current), GUILayout.Width(110f));
            if (GUILayout.Button(">", GUILayout.Width(30f))) ChangeKey(action, current, +1);
            GUILayout.EndHorizontal();
        }

        void ChangeKey(string action, string current, int direction)
        {
            var used = new System.Collections.Generic.HashSet<string>();
            foreach (var b in draft.bindings) if (b.action != action) used.Add(b.path);
            draft.SetPath(action, SettingsRules.NextKey(current, direction, used));
            Preview();
        }
    }
}
