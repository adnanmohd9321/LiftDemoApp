// ============================================================================
// ElevatorEnums.cs — State and direction enumerations for the elevator system
// ============================================================================

namespace ElevatorSimulation
{
    /// <summary>
    /// Represents the current operational state of an individual elevator.
    /// </summary>
    public enum ElevatorState
    {
        Idle,                   // Stationary, no pending requests
        Moving,                 // Traveling to a target floor
        Arriving,               // Just reached target floor
        DoorsOpening,           // Doors are sliding open
        WaitingForPassengers,   // Doors open, waiting before closing
        DoorsClosing            // Doors are sliding closed
    }

    /// <summary>
    /// Direction of elevator travel.
    /// </summary>
    public enum Direction
    {
        None,
        Up,
        Down
    }
}
