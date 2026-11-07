using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>Pure motion maths for cars that arrive and depart on their own.</summary>
    public static class CarAutoDriveMath
    {
        /// <summary>Remaining distance at or below which an arriving car counts as arrived.</summary>
        public const float ArrivedDistance = 0.001f;

        /// <summary>Arrival speed: cruise, but never faster than can brake to a stop in the distance left.</summary>
        public static float ArriveSpeed(float remaining, float cruise, float ease)
        {
            float stoppable = Mathf.Sqrt(2f * Mathf.Max(0f, ease) * Mathf.Max(0f, remaining));
            return Mathf.Min(cruise, stoppable);
        }

        /// <summary>Distance to move this step while arriving; never more than what remains.</summary>
        public static float ArriveStep(float remaining, float cruise, float ease, float dt)
        {
            float rest = Mathf.Max(0f, remaining);
            return Mathf.Min(rest, ArriveSpeed(rest, cruise, ease) * dt);
        }

        /// <summary>True once the car is close enough to its stop point.</summary>
        public static bool HasArrived(float remaining) => remaining <= ArrivedDistance;

        /// <summary>Speed while leaving: accelerates up to the top speed.</summary>
        public static float DepartSpeed(float speed, float acceleration, float top, float dt)
        {
            return Mathf.MoveTowards(speed, top, acceleration * dt);
        }

        /// <summary>True once the car has driven far enough to be out of view.</summary>
        public static bool HasLeft(float travelled, float exitDistance) => travelled >= exitDistance;
    }
}
