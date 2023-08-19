using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace xasset.tests
{
    public class RuntimeTest
    {
        [UnityTest]
        public IEnumerator ActionAsync()
        {
            Assets.MaxDownloads = 5;
            Downloader.Pause();
            if (Downloader.Paused) Logger.D("Paused");

            Downloader.UnPause();

            for (var i = 0; i < 20; i++)
            {
                var index = i;
                ActionRequest.CallAsync(() => Logger.D($"CallAsync {index} on frame {Time.frameCount}"));
            }

            while (Scheduler.Working) yield return null;
        }

        [UnityTest]
        public IEnumerator CancelActionAsync()
        {
            for (var i = 0; i < 5; i++)
            {
                var index = i;
                ActionRequest.CallAsync(() => Logger.D($"CallAsync {index} on frame {Time.frameCount}")).Cancel();
            }

            while (Scheduler.Working) yield return null;
        }

        [UnityTest]
        public IEnumerator InitializeAsync()
        {
            var request = Assets.InitializeAsync();
            yield return request;
            Assert.IsNull(request.error);
        }

        [UnityTest]
        public IEnumerator CheckForUpdatesAsync()
        {
            yield return InitializeAsync();
            var getUpdateInfoAsync = Assets.GetUpdateInfoAsync();
            yield return getUpdateInfoAsync;
            var getVersionsAsync = getUpdateInfoAsync.GetVersionsAsync();
            yield return getVersionsAsync;
            Logger.I(Assets.IsDownloaded("Assets/xasset/Samples/Arts/Textures/igg.png"));
        }

        [UnityTest]
        public IEnumerator GetDownloadSizeAsync()
        {
            yield return InitializeAsync();
            var request = Assets.Versions.GetDownloadSizeAsync("Assets/xasset/Samples/Arts/Textures");
            yield return request;
            if (request.downloadSize <= 0) yield break;
            var message = $"[xasset] New content available({Utility.FormatBytes(request.downloadSize)}), download now?";
            Logger.D(message);
            var download = request.DownloadAsync();
            yield return download;
        }

        [UnityTest]
        public IEnumerator LoadAssetAsync()
        {
            yield return InitializeAsync();
            var path = "Cube.prefab";
            if (!Assets.Contains(path)) yield break;

            var request = Asset.LoadAsync(path, typeof(GameObject));
            yield return request;
            Assert.NotNull(request.asset);
            request.Release();

            path = "Assets/Player@idle.fbx";
            var sub = Asset.LoadAllAsync(path, typeof(AnimationClip));
            yield return sub;
            Assert.NotNull(sub.assets);
            request.Release();
        }

        [UnityTest]
        public IEnumerator LoadSceneAsync()
        {
            yield return InitializeAsync();
            const string path = "Autorelease.unity";
            var request = Scene.LoadAsync(path, true);
            request.allowSceneActivation = false;
            request.WaitForCompletion();
            request.allowSceneActivation = true;
            request.Release();
        }
    }
}