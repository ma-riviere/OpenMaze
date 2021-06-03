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

        public VerticalComputer()
        {
            verticalPointedDirection = new Vector3();
        }

        public float compute(Vector3 OT, Vector3 OP)
        {
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

    public interface TargetDirectionComputer
    {
        bool pointsTarget(Vector3 v1,Vector3 v2,Vector3 v3);
    }

    public class RayDirectionComputer : TargetDirectionComputer
    {
        private Collider target;
        private Ray pointedDirection;
        private RaycastHit hit;
        public RayDirectionComputer(Collider c)
        {
            pointedDirection = new Ray();
            target = c;
        }

        public bool pointsTarget(Vector3 pos,Vector3 dir,Vector3 v)
        {
            pointedDirection.origin = pos;
            pointedDirection.direction = dir;
            return target.Raycast(pointedDirection, out hit, 1000);
        }


    }

    public class AngleDirectionComputer : TargetDirectionComputer
    {
        int maxAngle;

        public AngleDirectionComputer(int maxA)
        {
            maxAngle = maxA;
        }

        public new bool pointsTarget(Vector3 pos,Vector3 dir,Vector3 userToTarget)
        {
            return Vector3.Angle(userToTarget, dir) < maxAngle;
        }

    }

    public class VoidComputer : TargetDirectionComputer
    {
        public bool pointsTarget(Vector3 v1,Vector3 v2)
        {
            return false;
        }

    }

}