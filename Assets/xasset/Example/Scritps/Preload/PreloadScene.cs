namespace xasset.example
{
    public class PreloadScene : Loadable
    {
        private SceneRequest _request;
        public override bool isDone => _request == null || _request.isDone;

        protected override void OnLoad()
        {
            _request = Scene.LoadAsync(path);
            loading = true;
        }
    }
}