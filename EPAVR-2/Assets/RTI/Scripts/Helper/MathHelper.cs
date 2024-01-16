using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class MathHelper : MonoBehaviour
    {
        public static float QuickDistance(Vector3 _start, Vector3 _end)
        {
            // Create the vector for heading
            Vector3 heading;
            // Calculate the heading values
            heading.x = _start.x - _end.x;
            heading.y = _start.y - _end.y;
            heading.z = _start.z - _end.z;
            // Calculate the distance squared
            float distanceSquared = heading.x * heading.x + heading.y * heading.y + heading.z * heading.z;
            // Calculate and return the actual distance by finding the square root of that
            return Mathf.Sqrt(distanceSquared);
        }

        public static float QuickDistance(Vector2 _start, Vector2 _end)
        {
            // Create the vector for heading
            Vector2 heading;
            // Calculate the heading values
            heading.x = _start.x - _end.x;
            heading.y = _start.y - _end.y;
            // Calculate the distance squared
            float distanceSquared = heading.x * heading.x + heading.y * heading.y;
            // Calculate and return the actual distance by finding the square root of that
            return Mathf.Sqrt(distanceSquared);
        }

        public static Vector3 GetRandomPointWithinCollider(Collider _col)
        {
            // Check what kind of collider this is
            if (_col is SphereCollider sphere)
            {
                // Return a random point within the sphere collider
                return ((Random.insideUnitSphere * sphere.radius) + _col.transform.TransformPoint(sphere.center));
            }
            else
            {
                // Get the bounds of the collider
                Bounds bounds = _col.bounds;
                // Generate a random point within these bounds
                return new Vector3(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.y, bounds.max.y), Random.Range(bounds.min.z, bounds.max.z));
            }
        }

        public static Bounds GetBounds(Vector3 _center, Vector3 _minPosition, Vector3 _maxPosition)
        {
            return new Bounds(_center, (_maxPosition - _minPosition));
        }

        public static float ClampAngle(float _lfAngle, float _lfMin, float _lfMax)
        {
            if (_lfAngle < -360f) _lfAngle += 360f;
            if (_lfAngle > 360f) _lfAngle -= 360f;
            return Mathf.Clamp(_lfAngle, _lfMin, _lfMax);
        }

        public static float ClampAngle(float _angle)
        {
            _angle = _angle % 360.0f;
            if (_angle < 0.0f) _angle += 360.0f;
            return _angle;
        }

        public static string FormatTime(float _time)
        {
            int hours = (int) _time / 3600;
            int minutes = (int)_time / 60;
            int seconds = (int)_time - 60 * minutes;

            return string.Format($"{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
        }

        public static float CalculateRectangularPrismArea(Vector3 _dimensions)
        {
            float width = _dimensions.x;
            float height = _dimensions.y;
            float length = _dimensions.z;

            return 2 * (width * length + height * length + height * width);
        }


        public static float ConvertMetersToFeet(float _value)
        {
            return _value * 3.2808399f;
        }

        public static string GetMeasurementDisplay(float _value)
        {
            string text = $"{(MathHelper.ConvertMetersToFeet(_value)).ToString("0.000")} ft ({_value.ToString("0.000")} m)";
            return text;
        }
    }
}

