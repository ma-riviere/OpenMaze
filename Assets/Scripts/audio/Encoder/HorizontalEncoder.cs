using audio.Computer;

namespace audio
{
    public class HorizontalEncoder : OneDEncoder
    {
        protected override void selectAngleComputing()
        {
            angleComputer = new HorizontalComputer();
        }

        void Update()
        {
            compute();
            stereoMonoInterface.computeAndSend(angle);
        }
    }
}