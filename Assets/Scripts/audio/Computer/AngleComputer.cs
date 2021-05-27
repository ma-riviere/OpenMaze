using UnityEngine;

namespace audio.Computer
{
    public interface AngleComputer
    {
        float compute(Vector3 OT, Vector3 OP);
    }

    public class HorizontalComputer : AngleComputer
    {
        private Vector2 userToTargetHorizontalVector, horizontalPointedDirection;
        public HorizontalComputer() { }

        public float compute(Vector3 OT, Vector3 OP)
        {
            userToTargetHorizontalVector.Set(OT.x, OT.z);
            horizontalPointedDirection.Set(OP.x, OP.z);
            return Vector2.SignedAngle(userToTargetHorizontalVector, horizontalPointedDirection);
        }
    }

    public class VerticalComputer : AngleComputer
    {
        
        private Vector3 verticalPointedDirection;
        
        /*targetCenterOnScreen, verticalScreenPoint;
        private Camera cam;
        GameObject target;
        
        public VerticalComputer(Camera c, GameObject target)
        {
            cam = c;
            this.target = target;
            targetCenterOnScreen = cam.WorldToScreenPoint(target.transform.position);
            verticalScreenPoint = new Vector3(targetCenterOnScreen.x, cam.pixelHeight / 2, targetCenterOnScreen.z);
        }
        */
        public VerticalComputer()
        {
            verticalPointedDirection = new Vector3();
        }

        public float compute(Vector3 OT, Vector3 OP)
        {
            /*
            targetCenterOnScreen = cam.WorldToScreenPoint(target.transform.position);
            verticalScreenPoint.x = targetCenterOnScreen.x;
            verticalScreenPoint.z = targetCenterOnScreen.z;
            verticalPointedDirection = cam.ScreenPointToRay(verticalScreenPoint).direction;
            */
            verticalPointedDirection.Set(OT.x, OP.y * OT.magnitude, OT.z);
            return Vector3.Angle(OT, verticalPointedDirection);
        }
    }

    public class ThreeDComputer : AngleComputer
    {
        public float compute(Vector3 OT, Vector3 OP)
        {
            return Vector3.Angle(OT, OP);
        }
    }

}