using audio.computers;

namespace audio.encoders
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