using UnityEngine;

namespace audio
{
    public class AudioConstants
    {
        public const int HIGH_FREQ = 440, LOW_FREQ = 110;


    }

    public interface AngleComputer
    {
        public float compute(Vector3 OT,Vector3 OP);
    }

    public class HorizontalComputer: AngleComputer
    {
        private Vector2 userToTargetHorizontalVector, horizontalPointedDirection;
        public HorizontalComputer() { }

        public float compute(Vector3 OT,Vector3 OP)
        {
            userToTargetHorizontalVector.Set(OT.x, OT.z);
            horizontalPointedDirection.Set(OP.x, OP.z);
            return Vector2.SignedAngle(userToTargetHorizontalVector, horizontalPointedDirection);
        }
    }

    public class VerticalComputer : AngleComputer
    {
        private Vector3 verticalPointedDirection, targetCenterOnScreen, verticalScreenPoint;
        private Camera cam;
        GameObject target;

        public VerticalComputer(Camera c,GameObject target)
        {
            cam = c;
            this.target = target;
            targetCenterOnScreen = cam.WorldToScreenPoint(target.transform.position);
            verticalScreenPoint = new Vector3(targetCenterOnScreen.x, cam.pixelHeight / 2, targetCenterOnScreen.z);
        }

        public float compute(Vector3 OT,Vector3 OP)
        {
            targetCenterOnScreen = cam.WorldToScreenPoint(target.transform.position);
            verticalScreenPoint.x = targetCenterOnScreen.x;
            verticalScreenPoint.z = targetCenterOnScreen.z;
            verticalPointedDirection = cam.ScreenPointToRay(verticalScreenPoint).direction;
            return Vector3.Angle(OT, verticalPointedDirection);
        }
    }

    public class ThreeDComputer : AngleComputer
    {
        public float compute(Vector3 OT,Vector3 OP)
        {
            return Vector3.Angle(OT, OP);
        }
    }

    public abstract class FrequencyComputer
    {
        public abstract float computeFrequency(float angle);
    }

    public class ContinuousComputer : FrequencyComputer
    {
        protected float k, b;
        public ContinuousComputer(float maxAngle)
        {
            k = Mathf.Log(Mathf.Pow(0.25f, 1.0f / Mathf.Log(maxAngle)));
            b = Mathf.Log(AudioConstants.HIGH_FREQ);
        }

        public override float computeFrequency(float angle)
        {
            return Mathf.Exp(k * Mathf.Log(angle) + b);
        }
    }

    public class DiscreteComputer : FrequencyComputer
    {
        float threshold;
        public DiscreteComputer(float threshold)
        {
            this.threshold = threshold;
        }

        public override float computeFrequency(float angle)
        {
            if (angle < threshold)
                return AudioConstants.HIGH_FREQ;
            else return AudioConstants.LOW_FREQ;
        }

    }

}