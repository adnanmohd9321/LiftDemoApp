// ============================================================================
// Elevator.cs — Individual elevator: state machine, queue, smooth movement
// ============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElevatorSimulation
{
    /// <summary>
    /// Controls a single elevator car. Manages its own request queue,
    /// moves smoothly between floors, and drives door open/close.
    /// </summary>
    public class Elevator : MonoBehaviour
    {
        // ----------------------------------------------------------------
        // Inspector Settings
        // ----------------------------------------------------------------

        [Header("Movement")]
        [Tooltip("Vertical speed in units per second.")]
        public float moveSpeed = 3f;

        [Tooltip("World-space height of one floor.")]
        public float floorHeight = 3f;

        [Header("Timing")]
        [Tooltip("Seconds to keep doors open before closing.")]
        public float doorWaitTime = 2f;

        [Header("References")]
        public ElevatorDoor door;

        [Header("Identity")]
        [Tooltip("Display name shown in the UI (e.g. Lift A).")]
        public string elevatorName = "Lift";

        // ----------------------------------------------------------------
        // Runtime State  (read-only from outside)
        // ----------------------------------------------------------------

        /// <summary>Current floor the elevator is at or passing.</summary>
        public int CurrentFloor { get; private set; }

        /// <summary>Current state in the state machine.</summary>
        public ElevatorState State { get; private set; } = ElevatorState.Idle;

        /// <summary>Current travel direction.</summary>
        public Direction CurrentDirection { get; private set; } = Direction.None;

        /// <summary>Read-only view of pending floor requests.</summary>
        public IReadOnlyList<int> RequestQueue => requestQueue;

        // ----------------------------------------------------------------
        // Events
        // ----------------------------------------------------------------

        /// <summary>Fired whenever the elevator arrives at a floor or the displayed floor changes.</summary>
        public event Action<Elevator> OnFloorChanged;

        /// <summary>Fired whenever the state changes.</summary>
        public event Action<Elevator> OnStateChanged;

        /// <summary>Fired when elevator finishes serving a floor (doors closed).</summary>
        public event Action<Elevator, int> OnFloorServiced;

        // ----------------------------------------------------------------
        // Private fields
        // ----------------------------------------------------------------

        private readonly List<int> requestQueue = new List<int>();
        private int targetFloor;
        private float doorTimer;
        private float baseY;            // Y position of ground floor

        // ----------------------------------------------------------------
        // Lifecycle
        // ----------------------------------------------------------------

        private void Awake()
        {
            baseY = transform.position.y;
        }

        private void Update()
        {
            switch (State)
            {
                case ElevatorState.Idle:
                    HandleIdle();
                    break;

                case ElevatorState.Moving:
                    HandleMoving();
                    break;

                case ElevatorState.Arriving:
                    HandleArriving();
                    break;

                case ElevatorState.DoorsOpening:
                    HandleDoorsOpening();
                    break;

                case ElevatorState.WaitingForPassengers:
                    HandleWaiting();
                    break;

                case ElevatorState.DoorsClosing:
                    HandleDoorsClosing();
                    break;
            }
        }

        // ----------------------------------------------------------------
        // Public API
        // ----------------------------------------------------------------

        /// <summary>
        /// Enqueue a floor request. Duplicates and requests for the
        /// current floor (while idle) are ignored.
        /// </summary>
        public void AddRequest(int floor)
        {
            // Already going there or already there and idle
            if (requestQueue.Contains(floor))
                return;

            if (floor == CurrentFloor && State == ElevatorState.Idle)
            {
                // Already here — just open doors
                SetState(ElevatorState.Arriving);
                return;
            }

            requestQueue.Add(floor);

            // Sort queue intelligently based on current direction
            SortQueue();
        }

        /// <summary>
        /// Returns true if this elevator already has the given floor
        /// in its queue or is currently heading to it.
        /// </summary>
        public bool HasFloorInQueue(int floor)
        {
            return requestQueue.Contains(floor) ||
                   (State == ElevatorState.Moving && targetFloor == floor);
        }

        /// <summary>
        /// True when the elevator is traveling toward the given floor
        /// (same direction, not yet past it).
        /// </summary>
        public bool IsMovingTowards(int floor)
        {
            if (State != ElevatorState.Moving)
                return false;

            if (CurrentDirection == Direction.Up && floor >= CurrentFloor)
                return true;

            if (CurrentDirection == Direction.Down && floor <= CurrentFloor)
                return true;

            return false;
        }

        // ----------------------------------------------------------------
        // State handlers
        // ----------------------------------------------------------------

        private void HandleIdle()
        {
            if (requestQueue.Count == 0)
                return;

            targetFloor = requestQueue[0];
            requestQueue.RemoveAt(0);

            if (targetFloor == CurrentFloor)
            {
                // Same floor — skip movement, go straight to arriving
                SetState(ElevatorState.Arriving);
                return;
            }

            CurrentDirection = targetFloor > CurrentFloor ? Direction.Up : Direction.Down;
            SetState(ElevatorState.Moving);
        }

        private void HandleMoving()
        {
            Vector3 target = new Vector3(
                transform.position.x,
                baseY + targetFloor * floorHeight,
                transform.position.z);

            transform.position = Vector3.MoveTowards(
                transform.position, target, moveSpeed * Time.deltaTime);

            // Update displayed floor based on position
            int approxFloor = Mathf.RoundToInt((transform.position.y - baseY) / floorHeight);
            if (approxFloor != CurrentFloor)
            {
                CurrentFloor = approxFloor;
                OnFloorChanged?.Invoke(this);
            }

            // Check arrival
            if (Mathf.Abs(transform.position.y - target.y) < 0.01f)
            {
                transform.position = target;
                CurrentFloor = targetFloor;
                CurrentDirection = Direction.None;
                OnFloorChanged?.Invoke(this);
                SetState(ElevatorState.Arriving);
            }
        }

        private void HandleArriving()
        {
            if (door != null)
                door.Open();

            SetState(ElevatorState.DoorsOpening);
        }

        private void HandleDoorsOpening()
        {
            if (door == null || door.IsFullyOpen)
            {
                doorTimer = doorWaitTime;
                SetState(ElevatorState.WaitingForPassengers);
            }
        }

        private void HandleWaiting()
        {
            doorTimer -= Time.deltaTime;
            if (doorTimer <= 0f)
            {
                if (door != null)
                    door.Close();

                SetState(ElevatorState.DoorsClosing);
            }
        }

        private void HandleDoorsClosing()
        {
            if (door == null || door.IsFullyClosed)
            {
                int servicedFloor = CurrentFloor;
                SetState(ElevatorState.Idle);
                OnFloorServiced?.Invoke(this, servicedFloor);
            }
        }

        // ----------------------------------------------------------------
        // Helpers
        // ----------------------------------------------------------------

        private void SetState(ElevatorState newState)
        {
            State = newState;
            OnStateChanged?.Invoke(this);
        }

        /// <summary>
        /// Sorts the request queue so the elevator services floors
        /// in the current direction of travel first (elevator algorithm).
        /// </summary>
        private void SortQueue()
        {
            if (requestQueue.Count <= 1) return;

            // Determine effective direction
            Direction dir = CurrentDirection;
            if (dir == Direction.None && requestQueue.Count > 0)
            {
                dir = requestQueue[0] > CurrentFloor ? Direction.Up : Direction.Down;
            }

            // Partition into same-direction and opposite-direction
            List<int> sameDir = new List<int>();
            List<int> otherDir = new List<int>();

            foreach (int f in requestQueue)
            {
                bool isSameDir = (dir == Direction.Up && f >= CurrentFloor) ||
                                 (dir == Direction.Down && f <= CurrentFloor);
                if (isSameDir)
                    sameDir.Add(f);
                else
                    otherDir.Add(f);
            }

            // Sort same-direction ascending if going up, descending if down
            if (dir == Direction.Up)
                sameDir.Sort();
            else
                sameDir.Sort((a, b) => b.CompareTo(a));

            // Opposite direction in reverse
            if (dir == Direction.Up)
                otherDir.Sort((a, b) => b.CompareTo(a));
            else
                otherDir.Sort();

            requestQueue.Clear();
            requestQueue.AddRange(sameDir);
            requestQueue.AddRange(otherDir);
        }
    }
}
