// ============================================================================
// ElevatorDoor.cs — Handles smooth open/close animation for elevator doors
// ============================================================================

using UnityEngine;

namespace ElevatorSimulation
{
    /// <summary>
    /// Animates left and right door transforms by sliding them apart (open)
    /// or together (close). Attach to the elevator car GameObject; assign
    /// the two door child transforms in the Inspector or via code.
    /// </summary>
    public class ElevatorDoor : MonoBehaviour
    {
        [Header("Door References")]
        public Transform leftDoor;
        public Transform rightDoor;

        [Header("Settings")]
        [Tooltip("How far each door slides from center when fully open.")]
        public float openOffset = 0.45f;

        [Tooltip("Speed of door movement in units/second.")]
        public float doorSpeed = 3f;

        // Cached positions
        private Vector3 leftClosedPos;
        private Vector3 rightClosedPos;
        private Vector3 leftOpenPos;
        private Vector3 rightOpenPos;

        // Animation state
        private bool isOpening;
        private bool isClosing;

        /// <summary>True when the doors have finished opening.</summary>
        public bool IsFullyOpen { get; private set; }

        /// <summary>True when the doors have finished closing.</summary>
        public bool IsFullyClosed { get; private set; } = true;

        // --------------------------------------------------------------------
        // Lifecycle
        // --------------------------------------------------------------------

        private void Start()
        {
            if (leftDoor == null || rightDoor == null)
            {
                Debug.LogError($"[ElevatorDoor] Missing door references on {gameObject.name}");
                return;
            }

            // Cache the closed positions from their initial placement
            leftClosedPos  = leftDoor.localPosition;
            rightClosedPos = rightDoor.localPosition;

            // Open positions: slide apart along the X axis
            leftOpenPos  = leftClosedPos  + Vector3.left  * openOffset;
            rightOpenPos = rightClosedPos + Vector3.right * openOffset;
        }

        private void Update()
        {
            if (isOpening)
                AnimateOpen();
            else if (isClosing)
                AnimateClose();
        }

        // --------------------------------------------------------------------
        // Public API
        // --------------------------------------------------------------------

        /// <summary>Begin opening the doors.</summary>
        public void Open()
        {
            isOpening    = true;
            isClosing    = false;
            IsFullyClosed = false;
        }

        /// <summary>Begin closing the doors.</summary>
        public void Close()
        {
            isClosing   = true;
            isOpening   = false;
            IsFullyOpen = false;
        }

        // --------------------------------------------------------------------
        // Animation helpers
        // --------------------------------------------------------------------

        private void AnimateOpen()
        {
            leftDoor.localPosition  = Vector3.MoveTowards(leftDoor.localPosition,  leftOpenPos,  doorSpeed * Time.deltaTime);
            rightDoor.localPosition = Vector3.MoveTowards(rightDoor.localPosition, rightOpenPos, doorSpeed * Time.deltaTime);

            if (Vector3.Distance(leftDoor.localPosition, leftOpenPos) < 0.005f)
            {
                leftDoor.localPosition  = leftOpenPos;
                rightDoor.localPosition = rightOpenPos;
                isOpening  = false;
                IsFullyOpen = true;
            }
        }

        private void AnimateClose()
        {
            leftDoor.localPosition  = Vector3.MoveTowards(leftDoor.localPosition,  leftClosedPos,  doorSpeed * Time.deltaTime);
            rightDoor.localPosition = Vector3.MoveTowards(rightDoor.localPosition, rightClosedPos, doorSpeed * Time.deltaTime);

            if (Vector3.Distance(leftDoor.localPosition, leftClosedPos) < 0.005f)
            {
                leftDoor.localPosition  = leftClosedPos;
                rightDoor.localPosition = rightClosedPos;
                isClosing    = false;
                IsFullyClosed = true;
            }
        }
    }
}
