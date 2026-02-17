namespace GORE.Engine
{
    public class MusicSystem
    {
        public void Play(string track)
        {
            System.Diagnostics.Debug.WriteLine($"Music: Play {track}");
        }

        public void Stop()
        {
            System.Diagnostics.Debug.WriteLine($"Music: Stop");
        }
    }
}