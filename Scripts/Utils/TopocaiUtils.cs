using UnityEngine;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Topacai.Utils
{
    /// <summary>
    /// Event where the broadcaster expects to receibe a fallback from all listeners in an async way
    /// in order to receive data from them or just wait for all listeners to finish their tasks after emit the event
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Func<object, Task>>> _handlers = new();

        /// <summary>
        /// Suscribers must provide a type used to identify the arguments and the event itself, such a class like PlayerDamaged
        /// if there is no existent event using that type, it will be created on suscribe, but not on publish.
        /// </summary>
        /// <param name="handler">Handlers must be asyncronous Task</param>
        /// <typeparam name="T"></typeparam>
        public static void Subscribe<T>(Func<T, Task> handler)
        {
            var type = typeof(T);
            if (!_handlers.ContainsKey(type))
                _handlers[type] = new List<Func<object, Task>>();

            _handlers[type].Add(async (obj) => await handler((T)obj));
        }

        /// <summary>
        /// Invokes an event using an "instance" of the desired type
        /// </summary>
        /// <param name="message"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task Publish<T>(T message)
        {
            var type = typeof(T);
            if (!_handlers.ContainsKey(type))
                return;

            IEnumerable<Task> tasks = _handlers[type].Select(h => h(message));
            await Task.WhenAll(tasks);
        }
    }


    public static class Miscelanius
    {
        // Gets a random Color from an int
        public static Color GetColorFromInt(int value)
        {
            int hash = value.GetHashCode();

            float r = ((hash >> 16) & 0xFF) / 255f;
            float g = ((hash >> 8) & 0xFF) / 255f;
            float b = (hash & 0xFF) / 255f;

            return new Color(r, g, b, 1f);
        }
    }

    public static class Transforms
    {
        public static void ScaleRelativeToPivot(Transform transform, Vector3 scaleFactor, Vector3 pivot)
        {
            Vector3 originalPos = transform.position;

            // New pos is calculated to keep object in same position
            // newPos = pivot + (originalPos - pivot) * scaleFactor
            Vector3 newPos = pivot + Vector3.Scale((originalPos - pivot), scaleFactor);

            transform.position = newPos;
            transform.localScale = Vector3.Scale(transform.localScale, scaleFactor);
        }
    }

    public static class Mathematics
    {
        public static bool AreVectorsApproximatelyOpposite(Vector3 a, Vector3 b, float tolerance = 0.1f)
        {
            Vector3 normalizedA = a.normalized;
            Vector3 normalizedB = b.normalized;
            float dotProduct = Vector3.Dot(normalizedA, normalizedB);

            return dotProduct <= -1 + tolerance;
        }

        public static float NormalizeAngle(float angle)
        {
            angle %= 360f;
            if (angle < 0f) angle += 360f;
            return angle;
        }

        public static float GetNormalizedProgress(float from, float to, float current)
        {
            float start = NormalizeAngle(from);
            float end = NormalizeAngle(to);
            float value = NormalizeAngle(current);

            float delta = Mathf.DeltaAngle(start, end);
            float progress = Mathf.DeltaAngle(start, value) / delta;

            return Mathf.Clamp01(progress);
        }

    }

    public static class Numbers
    {
        public static float InterpolateWithMedian(float value, float min, float median, float max)
        {
            if (value <= min) return 0f; // Max clamp
            if (value >= max) return 1f; // Min clamp

            if (value <= median)
            {
                // From 0 to 0.5
                return Mathf.InverseLerp(min, median, value) * 0.5f;
            }
            else
            {
                // From 0.5 to 1
                return 0.5f + Mathf.InverseLerp(median, max, value) * 0.5f;
            }
        }
    }
    public static class Interpolations
    {
        public static Vector3 LinearLerp(Vector3 start, Vector3 end, float timer)
        {
            return start + (end - start) * timer;
        }

        public static Vector3 QuadraticLerp(Vector3 a, Vector3 b, Vector3 c, float t)
        {
            Vector3 ab = LinearLerp(a, b, t);
            Vector3 bc = LinearLerp(b, c, t);

            return LinearLerp(ab, bc, t);
        }

        public static Vector3 CubicLerp(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
        {
            Vector3 ab_bc = QuadraticLerp(a, b, c, t);
            Vector3 bc_cd = QuadraticLerp(b, c, d, t);

            return LinearLerp(ab_bc, bc_cd, t);
        }
    }
#if UNITY_EDITOR
    public static class GizmosUtils
    {
        public static void DrawCapsule(Vector3 top, Vector3 bottom, float radius, Color color)
        {
            Gizmos.color = color;
            DrawSphere(top, radius, color);
            DrawSphere(bottom, radius, color);
            Gizmos.DrawLine(top + Vector3.up * radius, bottom + Vector3.up * radius);
            Gizmos.DrawLine(top - Vector3.up * radius, bottom - Vector3.up * radius);
            Gizmos.DrawLine(top + Vector3.forward * radius, bottom + Vector3.forward * radius);
            Gizmos.DrawLine(top - Vector3.forward * radius, bottom - Vector3.forward * radius);
            Gizmos.DrawLine(top + Vector3.right * radius, bottom + Vector3.right * radius);
            Gizmos.DrawLine(top - Vector3.right * radius, bottom - Vector3.right * radius);
        }

        public static void DrawSphere(Vector3 position, float radius, Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawWireSphere(position, radius);
        }
    }
#endif
}
