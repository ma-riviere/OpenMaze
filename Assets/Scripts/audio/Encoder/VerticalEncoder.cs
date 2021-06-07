using audio.computers;

namespace audio.encoders
{
    public class VerticalEncoder : OneDEncoder
    {
        protected override void selectAngleComputing()
        {
            angleComputer = new VerticalComputer();
        }

        void Update()
        {
            compute();
        }

    }
}