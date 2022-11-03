namespace xasset.example
{
    public class Loadable
    {
        public string path { get; set; }
        public bool loading { get; protected set; }
        public virtual bool isDone => true;

        protected virtual void OnLoad()
        {
        }

        protected virtual void OnLoaded()
        {
        }

        public void Complete()
        {
            OnLoaded();
        }

        public void Load()
        {
            OnLoad();
            loading = true;
        }
    }
}