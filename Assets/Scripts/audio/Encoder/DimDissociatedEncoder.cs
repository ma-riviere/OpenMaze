using audio.computers;

namespace audio.encoders
{
    /// <summary>
    /// Encodes the horizontal and vertical projections of the POT angle, and contains an optional stereo interface.
    /// </summary>
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
            horizontalAngle = horizontalComputer.computeAngle(userToTargetVector, transform.forward);
            stereoMonoInterface.computeAndSend(horizontalAngle);
        }

    }

}