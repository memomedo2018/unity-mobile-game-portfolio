using System;

namespace PortfolioSamples
{
    /// <summary>One-pointer virtual joystick, independent of Unity and frame rate.</summary>
    public sealed class MobileStick
    {
        private int? activePointer;
        private readonly float radius;
        private readonly float deadZone;

        public float X { get; private set; }
        public float Y { get; private set; }
        public bool IsHeld { get { return activePointer.HasValue; } }

        public MobileStick(float radiusPixels, float deadZoneFraction = 0.15f)
        {
            if (!Finite(radiusPixels) || radiusPixels <= 0)
                throw new ArgumentOutOfRangeException(nameof(radiusPixels));
            if (!Finite(deadZoneFraction) || deadZoneFraction < 0 || deadZoneFraction >= 1)
                throw new ArgumentOutOfRangeException(nameof(deadZoneFraction));
            radius = radiusPixels;
            deadZone = deadZoneFraction;
        }

        public bool Begin(int pointerId)
        {
            if (IsHeld) return false;
            activePointer = pointerId;
            X = Y = 0;
            return true;
        }

        // Displacement is relative to the initial press, in the same units as radius.
        public void Move(int pointerId, float deltaX, float deltaY)
        {
            if (activePointer != pointerId) return;
            if (!Finite(deltaX) || !Finite(deltaY))
            {
                X = Y = 0;
                return;
            }

            // Use doubles for length calculation to avoid overflow for large floats.
            double distance = Math.Sqrt((double)deltaX * deltaX + (double)deltaY * deltaY);
            double magnitude = Math.Min(distance / radius, 1);
            if (magnitude <= deadZone)
            {
                X = Y = 0;
                return;
            }
            double scaled = (magnitude - deadZone) / (1 - deadZone);
            X = (float)(deltaX / distance * scaled);
            Y = (float)(deltaY / distance * scaled);
        }

        public void End(int pointerId)
        {
            if (activePointer == pointerId) Cancel();
        }

        // Call on focus loss, app pause, or when the input UI is disabled.
        public void Cancel()
        {
            activePointer = null;
            X = Y = 0;
        }

        private static bool Finite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
