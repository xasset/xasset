using System;
using System.IO;
using xasset;

public class EditorFileHandler : IFileHandler
{
    public static readonly string dirPath = $"{Environment.CurrentDirectory}/SourceRes".Replace("\\", "/");

    public void OnStart(FileRequest request)
    {
        FileInfo fileInfo = new FileInfo(request.path);
        request.progress = 1;
        if (!fileInfo.Exists)
        {
            request.SetResult(Request.Result.Failed);
            return;
        }

        request.asset = GetFileAsset(request);
        request.SetResult(Request.Result.Success);
    }

    private FileAsset GetFileAsset(FileRequest request)
    {
        FileAsset fileAsset = new FileAsset();
        switch (request.readMode)
        {
            case FileReadMode.Bytes:
                fileAsset.bytes = File.ReadAllBytes(request.path);
                break;
            default:
                fileAsset.content = File.ReadAllText(request.path);
                break;
        }

        return fileAsset;
    }

    public void Update(FileRequest request)
    {
    }

    public void Dispose(FileRequest request)
    {
    }

    public void WaitForCompletion(FileRequest request)
    {
    }

    public void OnReload(FileRequest request)
    {
    }

    public static IFileHandler CreateInstance()
    {
        return new EditorFileHandler();
    }
}