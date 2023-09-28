using System;

namespace BodyTrackingDemo
{
    public static class Events
    {
        public static event Action DanceGameStart;
        public static event Action DanceGameStop;

        public static void OnDanceGameStart()
        {
            DanceGameStart?.Invoke();
        }

        public static void OnDanceGameStop()
        {
            DanceGameStop?.Invoke();
        }
    }
}