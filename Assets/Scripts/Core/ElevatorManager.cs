// ============================================================================
// ElevatorManager.cs — Central dispatcher: routes floor requests to the
//                       best available elevator using a scoring algorithm
// ============================================================================

using System.Collections.Generic;
using UnityEngine;

namespace ElevatorSimulation
{
    /// <summary>
    /// Singleton that owns all elevators and decides which one should
    /// respond to each floor call.  Guarantees that only ONE elevator
    /// is dispatched per unique request.
    /// </summary>
    public class ElevatorManager : MonoBehaviour
    {
        // ----------------------------------------------------------------
        // Singleton
        // ----------------------------------------------------------------

        public static ElevatorManager Instance { get; private set; }

        // ----------------------------------------------------------------
        // Inspector
        // ----------------------------------------------------------------

        [Tooltip("All elevators in the building.")]
        public Elevator[] elevators;

        [Tooltip("Total number of floors (including ground).")]
        public int totalFloors = 4;

        // ----------------------------------------------------------------
        // Runtime data
        // ----------------------------------------------------------------

        /// <summary>Floors that have been requested but not yet serviced.</summary>
        private readonly HashSet<int> pendingRequests = new HashSet<int>();

        /// <summary>Public read-only access to pending requests (for UI).</summary>
        public IReadOnlyCollection<int> PendingRequests => pendingRequests;

        // ----------------------------------------------------------------
        // Lifecycle
        // ----------------------------------------------------------------

        private void Awake()
        {
            // Enforce singleton
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Subscribe to elevator events so we can clear pending requests
            foreach (Elevator elev in elevators)
            {
                elev.OnFloorServiced += HandleFloorServiced;
            }
        }

        private void OnDestroy()
        {
            foreach (Elevator elev in elevators)
            {
                if (elev != null)
                    elev.OnFloorServiced -= HandleFloorServiced;
            }
        }

        // ----------------------------------------------------------------
        // Public API
        // ----------------------------------------------------------------

        /// <summary>
        /// Called by floor buttons. Finds the best elevator and dispatches
        /// the request.  Duplicate requests are ignored.
        /// </summary>
        public void RequestElevator(int floor)
        {
            if (floor < 0 || floor >= totalFloors)
            {
                Debug.LogWarning($"[ElevatorManager] Invalid floor request: {floor}");
                return;
            }

            // Check if any elevator is already handling this floor
            foreach (Elevator elev in elevators)
            {
                if (elev.HasFloorInQueue(floor))
                    return;

                // Already at that floor with doors open/opening
                if (elev.CurrentFloor == floor &&
                    (elev.State == ElevatorState.DoorsOpening ||
                     elev.State == ElevatorState.WaitingForPassengers))
                    return;
            }

            // Find best elevator
            Elevator best = FindBestElevator(floor);

            if (best != null)
            {
                pendingRequests.Add(floor);
                best.AddRequest(floor);
                Debug.Log($"[ElevatorManager] Dispatched {best.elevatorName} to floor {floor}");
            }
            else
            {
                Debug.LogWarning($"[ElevatorManager] No elevator available for floor {floor}");
            }
        }

        /// <summary>
        /// Returns true when the given floor has a pending (unsatisfied) request.
        /// </summary>
        public bool IsFloorRequested(int floor)
        {
            return pendingRequests.Contains(floor);
        }

        // ----------------------------------------------------------------
        // Scoring algorithm
        // ----------------------------------------------------------------

        /// <summary>
        /// Evaluates every elevator and returns the one with the lowest
        /// cost score for reaching <paramref name="floor"/>.
        /// </summary>
        private Elevator FindBestElevator(int floor)
        {
            Elevator best = null;
            float bestScore = float.MaxValue;

            foreach (Elevator elev in elevators)
            {
                float score = CalculateScore(elev, floor);
                if (score < bestScore)
                {
                    bestScore = score;
                    best = elev;
                }
            }

            return best;
        }

        /// <summary>
        /// Scoring heuristic:
        ///   • Idle elevator        → pure floor distance  (best)
        ///   • Moving TOWARDS floor → distance + small penalty
        ///   • Moving AWAY / busy   → distance + heavy penalty + queue load
        /// </summary>
        private float CalculateScore(Elevator elev, int targetFloor)
        {
            float distance = Mathf.Abs(elev.CurrentFloor - targetFloor);

            switch (elev.State)
            {
                case ElevatorState.Idle:
                    // Best candidate — just needs to travel
                    return distance;

                case ElevatorState.Moving:
                    if (elev.IsMovingTowards(targetFloor))
                    {
                        // Good — can pick it up on the way
                        return distance + 2f + elev.RequestQueue.Count;
                    }
                    // Moving away — penalise heavily
                    return distance + 15f + elev.RequestQueue.Count * 3f;

                default:
                    // Doors opening/closing/waiting — moderate penalty
                    return distance + 8f + elev.RequestQueue.Count * 2f;
            }
        }

        // ----------------------------------------------------------------
        // Event handlers
        // ----------------------------------------------------------------

        private void HandleFloorServiced(Elevator elev, int floor)
        {
            pendingRequests.Remove(floor);
        }
    }
}
