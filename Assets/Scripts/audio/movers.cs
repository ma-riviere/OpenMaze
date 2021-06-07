using UnityEngine;
using DS = data.DataSingleton;

namespace audio
{
    /// <summary>
    /// Currently works with mouse and keyboard, pads haven't been tested. Input interfaces to be separated, for VR, Switch or others devices.
    /// </summary>
    public abstract class Mover
    {
        protected Transform transform;
        private Vector3 motion = Vector3.zero;
        protected Vector2 r = Vector2.zero;
        private float speed;
        private float maxYAngle = 90f, minYAngle = -90f;
        protected Mover(Transform t)
        {
            speed = DS.GetData().CharacterData.MovementSpeed;
            transform = t;
        }

        public abstract void fixToPlane(Vector3 v);

        public void rotate()
        {
            setRotation();
            twoDtoQuaternionRotation();
        }

        public Vector3 computeMotion()
        {
            motion.Set(getSideMotion(), 0, Input.GetAxis("Vertical"));
            return transform.TransformDirection(motion) * speed * Time.deltaTime;
        }

        protected abstract float getSideMotion();

        protected abstract void setRotation();

        private void twoDtoQuaternionRotation()
        {
            transform.rotation = Quaternion.Euler(r.y, r.x, 0);
        }

        protected void setXcurrentRotation()
        {
            r.x += Input.GetAxis("Mouse X");
            r.x = Mathf.Repeat(r.x, 360);
        }

        protected void setYcurrentRotation()
        {
            r.y -= Input.GetAxis("Mouse Y");
            r.y = Mathf.Clamp(r.y, minYAngle, maxYAngle);
        }

    }

    public class HorizontalMover : Mover
    {
        public HorizontalMover(Transform t) : base(t)
        {
        }

        public override void fixToPlane(Vector3 v)
        {
            transform.position = new Vector3(transform.position.x, v.y, transform.position.z);
        }

        protected override void setRotation()
        {
            setXcurrentRotation();
        }

        protected override float getSideMotion()
        {
            return Input.GetAxis("Horizontal");
        }


    }

    public class VerticalMover : Mover
    {
        public VerticalMover(Transform t) : base(t) { }
        public override void fixToPlane(Vector3 v)
        {
            transform.LookAt(new Vector3(v.x, Random.value * 3, v.z));
            r.Set(transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.x);
        }

        protected override void setRotation()
        {
            setYcurrentRotation();
        }

        protected override float getSideMotion()
        {
            return 0;
        }
    }

    public class ThreeDMover : Mover
    {
        public ThreeDMover(Transform t) : base(t) { }

        public override void fixToPlane(Vector3 v)
        {
        }

        protected override void setRotation()
        {
            setXcurrentRotation();
            setYcurrentRotation();
        }

        protected override float getSideMotion()
        {
            return Input.GetAxis("Horizontal");
        }
    }

}