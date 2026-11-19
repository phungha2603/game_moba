using System;

namespace UnityEngine
{
    public static class Debug
    {
        public static void Log(object message) => Console.WriteLine($"[LOG] {message}");
        public static void LogWarning(object message) => Console.WriteLine($"[WARN] {message}");
        public static void LogError(object message) => Console.WriteLine($"[ERROR] {message}");
        public static void LogError(object message, Object context) => Console.WriteLine($"[ERROR] {message} (context: {context})");
    }

    public static class Time
    {
        public static float deltaTime = 1f / 60f;
    }

    public static class Input
    {
        public static float GetAxisRaw(string axisName) => 0f;
    }

    public static class Random
    {
        private static readonly System.Random s_Rng = new System.Random();

        public static float value => (float)s_Rng.NextDouble();
    }

    public static class Screen
    {
        public static int width = 1280;
        public static int height = 720;
    }

    public struct Rect
    {
        public float x, y, width, height;

        public Rect(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }
    }

    public static class GUI
    {
        public static void Label(Rect position, string text) { /* no-op for shim; UI is not unit-tested */ }
    }

    public class Camera : Component
    {
        public static Camera main { get; set; }

        public Vector3 WorldToScreenPoint(Vector3 point) => point;
    }

    public static class Resources
    {
        public static Func<string, object> LoadOverride;

        public static T Load<T>(string path) where T : class => LoadOverride?.Invoke(path) as T;
    }
}

namespace UnityEngine.Events
{
    public class UnityEvent
    {
        public void Invoke() { }
    }
}
