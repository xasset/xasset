namespace VEngine
{
    public class Reference
    {
        public int count { get; private set; }
        public bool unused
        {
            get { return count <= 0; }
        }
        public void Retain()
        {
            count++;
        }
        public void Release()
        {
            count--;
        }
    }
}