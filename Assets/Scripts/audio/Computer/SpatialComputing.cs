using UnityEngine;

namespace audio.computers
{
    public interface AngleComputer
    {
        float computeAngle(Vector3 OT, Vector3 OP);
    }

    /// <summary>
    /// Projects horizontally using the x and z dimensions of the vectors.
    /// </summary>
    public class HorizontalComputer : AngleComputer
    {
        private Vector2 userToTargetHorizontalVector, horizontalPointedDirection;
        public HorizontalComputer() { }

        // Updates the horizontal vectors using the whole vectors.
        public float computeAngle(Vector3 OT, Vector3 OP)
        {
            userToTargetHorizontalVector.Set(OT.x, OT.z);
            horizontalPointedDirection.Set(OP.x, OP.z);
            return Vector2.SignedAngle(userToTargetHorizontalVector, horizontalPointedDirection);
        }

    }

    /// <summary>
    /// Projects vertically using the y dimension of one vector and the magnitude of the other.
    /// </summary>
    public class VerticalComputer : AngleComputer
    {
        private Vector3 verticalPointedDirection;

        public VerticalComputer()
        {
            verticalPointedDirection = new Vector3();
        }

        //OP is normalized, OT is not : to get the correspounding value of OP.y according to the distance, it is multiplied by the OT magnitude.
        public float computeAngle(Vector3 OT, Vector3 OP)
        {
            verticalPointedDirection.Set(OT.x, OP.y * OT.magnitude, OT.z);
            return Vector3.Angle(OT, verticalPointedDirection);
        }

    }

    public class ThreeDComputer : AngleComputer
    {
        public float computeAngle(Vector3 OT, Vector3 OP)
        {
            return Vector3.Angle(OT, OP);
        }

    }

    /// <summary>
    /// Must check direction between origin of a vector and a target.
    /// </summary>
    public interface TargetDirectionComputer
    {
        bool pointsTarget(Vector3 v1, Vector3 v2, Vector3 v3);
    }

    /// <summary>
    /// Checks direction on target itself.
    /// </summary>
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

        public bool pointsTarget(Vector3 pos, Vector3 dir, Vector3 v)
        {
            pointedDirection.origin = pos;
            pointedDirection.direction = dir;
            return target.Raycast(pointedDirection, out hit, 1000);
        }


    }

    /// <summary>
    /// Checks direction using the angle to the user-target direction.
    /// </summary>
    public class AngleDirectionComputer : TargetDirectionComputer
    {
        int maxAngle;

        public AngleDirectionComputer(int maxA)
        {
            maxAngle = maxA;
        }

        public bool pointsTarget(Vector3 pos, Vector3 dir, Vector3 userToTarget)
        {
            return Vector3.Angle(userToTarget, dir) < maxAngle;
        }

    }

    /// <summary>
    /// Empty class for disabled option.
    /// </summary>
    public class VoidComputer : TargetDirectionComputer
    {
        public bool pointsTarget(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            return false;
        }

    }

}