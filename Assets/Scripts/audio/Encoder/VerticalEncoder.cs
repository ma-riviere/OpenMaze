using audio.Computer;

namespace audio
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