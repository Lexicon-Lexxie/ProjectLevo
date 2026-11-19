using UnityEngine;

namespace Levo.Puzzle
{
    /// <summary>
    /// A target. Hook a trigger's "On State Changed (Boolean)" event to
    /// SlidingDoor.SetOpen using the dynamic bool option.
    /// </summary>
    public class SlidingDoor : MonoBehaviour
    {
        [SerializeField] Vector3 openOffset = new Vector3(0f, 3f, 0f);   // local space
        [SerializeField] float speed = 2f;

        Vector3 closedPosition, openPosition;
        bool open;

        public bool IsOpen => open;

        void Awake()
        {
            closedPosition = transform.localPosition;
            openPosition = closedPosition + openOffset;
        }

        public void SetOpen(bool value) => open = value;

        void Update()
        {
            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition, open ? openPosition : closedPosition, speed * Time.deltaTime);
        }
    }
}
