using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System;

namespace Topacai.Utils.Attributes
{
#if UNITY_EDITOR

    //public class ReadOnlyAttribute : PropertyAttribute
    //{
    //    // Use [SerializeField/*, ReadOnly*/] for view fields in editor and can't edit.
    //}
    //[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    //public class ReadOnlyDrawer : PropertyDrawer
    //{
    //    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    //    {
    //        GUI.enabled = false;
    //        EditorGUI.PropertyField(position, property, label, true);
    //        GUI.enabled = true;
    //    }
    //}
    //
    //public class LayerAttribute : PropertyAttribute
    //{
    //    // NOTHING - just oxygen.
    //}
    //
    //[CustomPropertyDrawer(typeof(LayerAttribute))]
    //class LayerAttributeEditor : PropertyDrawer
    //{
    //    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    //    {
    //        // One line of  oxygen free code.
    //        property.intValue = EditorGUI.LayerField(position, label, property.intValue);
    //    }
    //
    //}
#endif
}

namespace Topacai.Utils
{
    public static class FileManager
    {
        public static List<string> GetFilesByPrefix(string directoryPath, string prefix, string fileExtension = ".json")
        {
            // Verificamos si el directorio existe
            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");
            }

            // Buscamos todos los archivos que tengan el prefijo y la extensión deseada
            string searchPattern = $"{prefix}*{fileExtension}"; // Prefijo seguido de cualquier nombre y la extensión
            string[] files = Directory.GetFiles(directoryPath, searchPattern);

            // Convertimos el array en una lista
            return new List<string>(files);
        }

        public static List<T> DeserializeFiles<T>(List<string> filePaths)
        {
            List<T> deserializedObjects = new List<T>();

            foreach (string filePath in filePaths)
            {
                try
                {
                    // Leer el contenido del archivo
                    string jsonContent = File.ReadAllText(filePath);

                    // Deserializar el contenido del archivo en un objeto de tipo T
                    T obj = JsonConvert.DeserializeObject<T>(jsonContent);

                    // Agregar el objeto deserializado a la lista
                    deserializedObjects.Add(obj);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deserializing file {filePath}: {ex.Message}");
                }
            }

            return deserializedObjects;
        }
    }
    public static class Miscelanius
    {
        public static Color GetColorFromInt(int value)
        {
            // Usa un hash basado en el valor int
            int hash = value.GetHashCode();

            // Extrae componentes RGB únicos del hash
            float r = ((hash >> 16) & 0xFF) / 255f;
            float g = ((hash >> 8) & 0xFF) / 255f;
            float b = (hash & 0xFF) / 255f;

            // Retorna el color generado
            return new Color(r, g, b, 1f);
        }
    }

    public static class Transforms
    {
        public static void ScaleRelativeToPivot(Transform transform, Vector3 scaleFactor, Vector3 pivot)
        {
            // Guardamos la posición original
            Vector3 originalPos = transform.position;

            // Calculamos la nueva posición para que el pivot se mantenga fijo:
            // newPos = pivot + (originalPos - pivot) * scaleFactor
            Vector3 newPos = pivot + Vector3.Scale((originalPos - pivot), scaleFactor);

            // Actualizamos la posición y la escala
            transform.position = newPos;
            transform.localScale = Vector3.Scale(transform.localScale, scaleFactor);
        }
    }

    public static class Mathematics
    {
        public static bool AreVectorsApproximatelyOpposite(Vector3 a, Vector3 b, float tolerance = 0.1f)
        {
            // Normalizar ambos vectores
            Vector3 normalizedA = a.normalized;
            Vector3 normalizedB = b.normalized;

            // Producto escalar
            float dotProduct = Vector3.Dot(normalizedA, normalizedB);

            // Verificar si están aproximadamente opuestos
            return dotProduct <= -1 + tolerance; // Cerca de -1
        }
    }

    public class PriorityQueue<T>
    {
        private List<Tuple<T, int>> _queue = new List<Tuple<T, int>>();

        public int Count => _queue.Count;

        private void SortByPriority()
        {
            _queue.Sort((a, b) => a.Item2.CompareTo(b.Item2));
        }

        public void Enqueue(T item, int priority)
        {
            _queue.Add(Tuple.Create(item, priority));
            SortByPriority();
        }

        public T Dequeue()
        {
            if (_queue.Count == 0)
                return default;

            SortByPriority();
            T element = _queue[0].Item1;
            _queue.RemoveAt(0);
            return element;
        }
    }

    public static class Numbers
    {
        public static float InterpolateWithMedian(float value, float min, float median, float max)
        {
            if (value <= min) return 0f; // Por debajo del mínimo
            if (value >= max) return 1f; // Por encima del máximo

            if (value <= median)
            {
                // Interpolación de 0 a 0.5 (mínimo a mediano)
                return Mathf.InverseLerp(min, median, value) * 0.5f;
            }
            else
            {
                // Interpolación de 0.5 a 1 (mediano a máximo)
                return 0.5f + Mathf.InverseLerp(median, max, value) * 0.5f;
            }
        }
    }
    public static class CustomLerps
    {
        public static Vector3 LinearLerp(Vector3 start, Vector3 end, float timer) => Vector3.Lerp(start, end, timer);

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
