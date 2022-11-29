namespace xasset
{
    public interface IDownloadHandler
    {
        void OnStart();
        void OnPause(bool paused);
        bool Update();
        void OnCancel();
    }
}