using audio.Computer;

namespace audio
{
    public class DimDissociatedEncoder : TwoDimEncoder
    {
        private AngleComputer horizontalComputer;
        private float horizontalAngle;

        protected override void selectAngleComputing()
        {
            angleComputer = new VerticalComputer();
            horizontalComputer = new HorizontalComputer();
        }

        protected override void setStereo()
        {
            horizontalAngle = horizontalComputer.compute(userToTargetVector, transform.forward);
            stereoMonoInterface.computeAndSend(horizontalAngle);
        }

    }

}