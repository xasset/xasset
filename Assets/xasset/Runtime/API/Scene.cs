namespace xasset
{
    public static class Scene
    {
        public static SceneRequest Load(string path, bool withAdditive = false)
        {
            var request = SceneRequest.LoadInternal(path, withAdditive);
            request?.WaitForCompletion();
            return request;
        }

        public static SceneRequest LoadAsync(string path, bool withAdditive = false)
        {
            return SceneRequest.LoadInternal(path, withAdditive);
        }
    }
}