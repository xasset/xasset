namespace xasset.example
{
    public class PreloadGameObject : Loadable
    {
        private InstantiateRequest _request;

        public override bool isDone => _request == null || _request.isDone;

        protected override void OnLoad()
        {
            _request = Asset.InstantiateAsync(path);
        }

        protected override void OnLoaded()
        {
            if (_request.result == Request.Result.Success) _request.gameObject.SetActive(false);
        }
    }
}