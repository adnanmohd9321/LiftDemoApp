// ============================================================================
// ElevatorStatusUI.cs — HUD panel showing each elevator's current floor,
//                        direction arrow, and state label
// ============================================================================

using UnityEngine;
using UnityEngine.UI;

namespace ElevatorSimulation.UI
{
    /// <summary>
    /// Manages the bottom status bar that shows real-time information
    /// for every elevator.  Dynamically creates UI text labels at Start
    /// based on the elevators registered in <see cref="ElevatorManager"/>.
    /// </summary>
    public class ElevatorStatusUI : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Parent transform where per-elevator panels will be created.")]
        public Transform panelParent;

        [Header("Style")]
        public Color textColor     = Color.white;
        public int   fontSize      = 18;
        public Font  font;

        // One text element per elevator
        private Text[] statusLabels;

        // ----------------------------------------------------------------

        private void Start()
        {
            if (ElevatorManager.Instance == null)
            {
                Debug.LogWarning("[ElevatorStatusUI] No ElevatorManager found.");
                return;
            }

            Elevator[] elevators = ElevatorManager.Instance.elevators;
            statusLabels = new Text[elevators.Length];

            for (int i = 0; i < elevators.Length; i++)
            {
                statusLabels[i] = CreateLabel(elevators[i].elevatorName);
                int idx = i; // capture for closure
                elevators[i].OnFloorChanged += _ => UpdateLabel(idx);
                elevators[i].OnStateChanged += _ => UpdateLabel(idx);
                UpdateLabel(i);
            }
        }

        // ----------------------------------------------------------------

        private void UpdateLabel(int index)
        {
            Elevator elev = ElevatorManager.Instance.elevators[index];

            string floorName = GetFloorName(elev.CurrentFloor);
            string arrow     = GetDirectionArrow(elev.CurrentDirection);
            string state     = GetStateShortName(elev.State);

            statusLabels[index].text = $"{elev.elevatorName}\n" +
                                       $"Floor: {floorName} {arrow}\n" +
                                       $"{state}";
        }

        // ----------------------------------------------------------------
        // Helpers
        // ----------------------------------------------------------------

        private Text CreateLabel(string labelName)
        {
            GameObject go = new GameObject(labelName + "_Status");
            go.transform.SetParent(panelParent, false);

            Text txt       = go.AddComponent<Text>();
            txt.font       = font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize   = fontSize;
            txt.color      = textColor;
            txt.alignment  = TextAnchor.MiddleCenter;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            txt.verticalOverflow   = VerticalWrapMode.Overflow;

            // Sizing via LayoutElement
            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth  = 160;
            le.preferredHeight = 70;

            return txt;
        }

        private static string GetFloorName(int floor)
        {
            return floor == 0 ? "G" : floor.ToString();
        }

        private static string GetDirectionArrow(Direction dir)
        {
            switch (dir)
            {
                case Direction.Up:   return "\u25B2"; // ▲
                case Direction.Down: return "\u25BC"; // ▼
                default:             return "";
            }
        }

        private static string GetStateShortName(ElevatorState state)
        {
            switch (state)
            {
                case ElevatorState.Idle:                   return "Idle";
                case ElevatorState.Moving:                 return "Moving...";
                case ElevatorState.Arriving:               return "Arriving";
                case ElevatorState.DoorsOpening:           return "Doors Opening";
                case ElevatorState.WaitingForPassengers:   return "Doors Open";
                case ElevatorState.DoorsClosing:           return "Doors Closing";
                default:                                   return "";
            }
        }
    }
}
