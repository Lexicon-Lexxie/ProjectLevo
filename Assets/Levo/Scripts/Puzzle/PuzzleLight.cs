using UnityEngine;

namespace Levo.Puzzle
{
    /// <summary>
    /// A NEW puzzle target added in Stage 2 to prove the design is modular.
    /// It reacts to any PuzzleTrigger (hook up "On State Changed" -> PuzzleLight.SetOn),
    /// and no existing script had to change.
    /// </summary>
    public class PuzzleLight : MonoBehaviour
    {
        [SerializeField] Light targetLight;
        [SerializeField] bool startOn = false;

        void Awake()
        {
            if (targetLight == null) targetLight = GetComponent<Light>();
        }

        void Start() => SetOn(startOn);

        public void SetOn(bool on)
        {
            if (targetLight != null) targetLight.enabled = on;
        }
    }
}
