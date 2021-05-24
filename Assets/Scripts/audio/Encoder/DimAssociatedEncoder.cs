using audio.Computer;

namespace audio
{
    public class DimAssociatedEncoder : TwoDimEncoder
    {
        protected override void selectAngleComputing()
        {
            angleComputer = new ThreeDComputer();
        }

        protected override void setStereo() { }
    }
}