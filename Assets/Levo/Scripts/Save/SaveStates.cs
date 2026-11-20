using System;
using UnityEngine;

namespace Levo.Save
{
    [Serializable]
    public class PlayerState
    {
        public Vector3 position;
        public float yaw;
    }

    [Serializable]
    public class PropState
    {
        public Vector3 position;
        public Quaternion rotation = Quaternion.identity;
    }

    [Serializable]
    public class DoorState
    {
        public bool open;
    }

    /// <summary>Safe parsing and range checks for saved object state. Pure logic, unit-tested.</summary>
    public static class StateJson
    {
        public const float MaxCoordinate = 10000f;

        /// <summary>Never throws.</summary>
        public static bool TryParse<T>(string json, out T value) where T : class
        {
            value = null;
            if (string.IsNullOrEmpty(json)) return false;
            try { value = JsonUtility.FromJson<T>(json); }
            catch (Exception) { value = null; return false; }
            return value != null;
        }

        public static bool IsFinite(float f) => !(float.IsNaN(f) || float.IsInfinity(f));

        /// <summary>Finite and within +/- MaxCoordinate on every axis (inclusive).</summary>
        public static bool IsReasonablePosition(Vector3 v) =>
            IsFinite(v.x) && IsFinite(v.y) && IsFinite(v.z) &&
            Mathf.Abs(v.x) <= MaxCoordinate && Mathf.Abs(v.y) <= MaxCoordinate && Mathf.Abs(v.z) <= MaxCoordinate;

        /// <summary>A real rotation has length close to 1 (all zeros or NaN are not rotations).</summary>
        public static bool IsValidRotation(Quaternion q)
        {
            if (!(IsFinite(q.x) && IsFinite(q.y) && IsFinite(q.z) && IsFinite(q.w))) return false;
            float sqr = q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w;
            return sqr > 0.9f && sqr < 1.1f;
        }
    }
}
