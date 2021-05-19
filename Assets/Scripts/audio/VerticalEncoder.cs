namespace audio
{
    public class VerticalEncoder : OneDEncoder
    {
        private void Awake()
        {
            init();
            angleComputer = new VerticalComputer(Cam, target);
        }
        void Update()
        {
            compute();
        }
    }
}