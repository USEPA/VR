using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace L58.EPAVR
{
    public class RadSpreadMapMarkerUIObject : MapMarkerUIObject
    {
        #region Initialization
        public override void Init(MapUI _parent, Vector3 _sourcePosition)
        {
            // Call base functionality
            base.Init(_parent, _sourcePosition);
        }

        public void LoadRadCloudInfo(RadCloud _cloud)
        {
            // Get radius
            float radius = _cloud.Radius;
            // Get cone radius angle range and wind direction
            float coneRadiusAngleRange = _cloud.ConeRadiusAngleRange;
            float coneAngle = coneRadiusAngleRange * 2.0f;
            float windDirection = _cloud.WindDirection;

            // Edit the image accordingly
            Image.fillAmount = (coneAngle / 360.0f);
            transform.localEulerAngles = new Vector3(0.0f, 0.0f, -windDirection + coneRadiusAngleRange);

            Vector3 defaultScale = transform.localScale;
            transform.localScale = new Vector3(defaultScale.x * radius, defaultScale.y * radius, defaultScale.z);
        }
        #endregion
    }
}

