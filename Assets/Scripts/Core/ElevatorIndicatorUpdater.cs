// ============================================================================
// ElevatorIndicatorUpdater.cs — Keeps the floor-number TextMesh above
// each elevator car in sync with the Elevator component.
// ============================================================================

using UnityEngine;

namespace ElevatorSimulation
{
    /// <summary>
    /// Tiny runtime helper that listens to <see cref="Elevator.OnFloorChanged"/>
    /// and updates a TextMesh label with the current floor name.
    /// </summary>
    public class ElevatorIndicatorUpdater : MonoBehaviour
    {
        [HideInInspector] public TextMesh label;

        private Elevator elevator;
        private static readonly string[] FLOOR_NAMES = { "G", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

        private void Start()
        {
            elevator = GetComponent<Elevator>();
            if (elevator != null)
                elevator.OnFloorChanged += UpdateLabel;
        }

        private void OnDestroy()
        {
            if (elevator != null)
                elevator.OnFloorChanged -= UpdateLabel;
        }

        private void UpdateLabel(Elevator e)
        {
            if (label == null) return;
            int f = e.CurrentFloor;
            label.text = f >= 0 && f < FLOOR_NAMES.Length ? FLOOR_NAMES[f] : f.ToString();
        }
    }
}
