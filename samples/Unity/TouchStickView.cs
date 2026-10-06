using PortfolioSamples;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PortfolioSamples.Unity
{
    /// <summary>Attach to a raycastable UI Image; requires an EventSystem and GraphicRaycaster.</summary>
    public sealed class TouchStickView : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField, Min(1)] private float radius = 80;
        [SerializeField, Range(0, 0.95f)] private float deadZone = 0.15f;
        private MobileStick stick;
        private Vector2 origin;
        private RectTransform surface;
        public Vector2 Value { get { return stick == null ? Vector2.zero : new Vector2(stick.X, stick.Y); } }

        private void Awake()
        {
            surface = GetComponent<RectTransform>();
            stick = new MobileStick(radius, deadZone);
        }

        public void OnPointerDown(PointerEventData data)
        {
            Vector2 position;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(surface, data.position, data.pressEventCamera, out position)) return;
            if (stick.Begin(data.pointerId)) origin = position;
        }

        public void OnDrag(PointerEventData data)
        {
            Vector2 position;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(surface, data.position, data.pressEventCamera, out position))
            {
                Vector2 delta = position - origin;
                stick.Move(data.pointerId, delta.x, delta.y);
            }
        }

        public void OnPointerUp(PointerEventData data) { stick.End(data.pointerId); }
        private void OnDisable() { if (stick != null) stick.Cancel(); }
        private void OnApplicationFocus(bool focused) { if (!focused && stick != null) stick.Cancel(); }
        private void OnApplicationPause(bool paused) { if (paused && stick != null) stick.Cancel(); }
    }
}
