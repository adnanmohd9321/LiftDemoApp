// ============================================================================
// FloorCallButton.cs — Handles a single floor-call button in the UI
// ============================================================================

using UnityEngine;
using UnityEngine.UI;

namespace ElevatorSimulation.UI
{
    /// <summary>
    /// Attach to a UI Button.  When clicked it asks the
    /// <see cref="ElevatorManager"/> to send the nearest elevator
    /// to <see cref="floor"/>.  Provides visual feedback while
    /// the request is pending.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class FloorCallButton : MonoBehaviour
    {
        [Tooltip("The floor this button calls an elevator to.")]
        public int floor;

        [Header("Colors")]
        public Color normalColor   = new Color(0.22f, 0.51f, 0.89f, 1f);  // blue
        public Color pressedColor  = new Color(0.95f, 0.65f, 0.15f, 1f);  // amber
        public Color arrivedColor  = new Color(0.36f, 0.72f, 0.36f, 1f);  // green

        private Button button;
        private Image  buttonImage;
        private bool   isPending;

        // ----------------------------------------------------------------

        private void Awake()
        {
            button      = GetComponent<Button>();
            buttonImage = GetComponent<Image>();

            button.onClick.AddListener(OnButtonClicked);
            SetColor(normalColor);
        }

        private void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(OnButtonClicked);
        }

        private void Update()
        {
            if (!isPending) return;

            // Check if the request has been serviced
            if (ElevatorManager.Instance != null &&
                !ElevatorManager.Instance.IsFloorRequested(floor))
            {
                // Flash green briefly then reset
                SetColor(arrivedColor);
                isPending = false;
                Invoke(nameof(ResetColor), 0.8f);
            }
        }

        // ----------------------------------------------------------------

        private void OnButtonClicked()
        {
            if (ElevatorManager.Instance == null)
            {
                Debug.LogWarning("[FloorCallButton] ElevatorManager not found.");
                return;
            }

            ElevatorManager.Instance.RequestElevator(floor);
            SetColor(pressedColor);
            isPending = true;
        }

        private void ResetColor()
        {
            SetColor(normalColor);
        }

        private void SetColor(Color c)
        {
            if (buttonImage != null)
                buttonImage.color = c;
        }
    }
}
