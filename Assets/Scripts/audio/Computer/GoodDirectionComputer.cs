using UnityEngine;

namespace audio.Computer
{
    public interface NoiseInterface
    {
        void setNoise(bool b);
    }

    public class Noise : NoiseInterface
    {
        private AudioInterface ai;
        private bool active;
        public Noise(AudioInterface i)
        {
            ai= i;
            active = false;
        }
        public void setNoise(bool noise)
        {
            if (noise)
            {
                if (!active)
                {
                    ai.setHits(0.5f);
                    ai.setFreq(0);
                    active = true;
                }
            }
            else if (active)
            {
                ai.setHits(0);
                active = false;
            }
        }
    }

    public class Void : NoiseInterface
    {
        public void setNoise(bool b) { }
    }

    public interface AbstractComputer
    {
        bool pointsTarget(Vector3 v1,Vector3 v2);
    }

    public class GoodDirectionComputer :AbstractComputer
    {
        private Collider target;
        private Ray pointedDirection;
        private RaycastHit hit;
        public GoodDirectionComputer(Collider c)
        {
            pointedDirection = new Ray();
            target = c;
        }

        public bool pointsTarget(Vector3 pos,Vector3 dir)
        {
            pointedDirection.origin = pos;
            pointedDirection.direction=dir;
            return target.Raycast(pointedDirection, out hit, 1000);
        }
    }

    public class VoidComputer:AbstractComputer
    {
        public bool pointsTarget(Vector3 v1,Vector3 v2)
        {
            return false;
        }
    }

}