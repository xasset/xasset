using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Versions
{
    public class ManifestAsset : Loadable
    {
        private static readonly List<ManifestAsset> Unused = new List<ManifestAsset>();
        private bool builtin;
        private UnityWebRequest request;
        public Manifest asset { get; protected set; }
        public ManifestVersion assetVersion { get; protected set; }
        protected string name { get; set; }

        public bool changed
        {
            get
            {
                if (assetVersion == null)
                {
                    return false;
                }

                var find = Versions.Manifest;
                if (find != null)
                {
                    return find.version < assetVersion.version;
                }

                return true;
            }
        }

        protected override void OnLoad()
        {
            asset = ScriptableObject.CreateInstance<Manifest>();
            asset.name = name;
            pathOrURL = builtin ? Versions.GetPlayerDataURL(name) : Versions.GetDownloadURL(name);
            var file = Manifest.GetVersionFile(name);
            var url = builtin ? Versions.GetPlayerDataURL(file) : Versions.GetDownloadURL(file);
            DownloadAsync(url, GetTemporaryPath(file));
            status = LoadableStatus.CheckVersion;
        }

        protected override void OnUnused()
        {
            Unused.Add(this);
        }

        public virtual void Override()
        {
            if (assetVersion == null)
            {
                return;
            }

            if (builtin)
            {
                var path = Versions.GetDownloadDataPath(Manifest.GetVersionFile(asset.name));
                var file = ManifestVersion.Load(path);
                if (file.version > assetVersion.version)
                {
                    path = Versions.GetDownloadDataPath(asset.name);
                    if (File.Exists(path))
                    {
                        using (var stream = File.OpenRead(path))
                        {
                            if (Utility.ComputeCRC32(stream) == file.crc)
                            {
                                asset.Load(path);
                                Versions.Override(asset);
                                return;
                            }
                        }
                    }
                }

                asset.Load(GetTemporaryPath(name));
                Versions.Override(asset);
            }
            else
            {
                if (!changed)
                {
                    return;
                }

                var split = name.Split(new[]
                {
                    '_'
                }, StringSplitOptions.RemoveEmptyEntries);
                if (split.Length > 1)
                {
                    var newName = split[0];
                    asset.name = newName;
                }

                var from = GetTemporaryPath(name);
                var dest = Versions.GetDownloadDataPath(name).Replace(name, asset.name);
                if (File.Exists(from))
                {
                    Logger.I("Copy {0} to {1}.", from, dest);
                    File.Copy(from, dest, true);
                }

                var versionName = Manifest.GetVersionFile(name);
                from = GetTemporaryPath(versionName);
                if (File.Exists(from))
                {
                    var path = Versions.GetDownloadDataPath(versionName).Replace(name, asset.name);
                    Logger.I("Copy {0} to {1}.", from, path);
                    File.Copy(from, path, true);
                }
            }

            Versions.Override(asset);
        }

        public static ManifestAsset LoadAsync(string name, bool builtin = false)
        {
            var manifestAsset = Versions.CreateManifest(name, builtin);
            manifestAsset.Load();
            return manifestAsset;
        }

        internal static ManifestAsset Create(string name, bool builtin)
        {
            return new ManifestAsset
            {
                name = name,
                builtin = builtin
            };
        }

        public static void UpdateManifestAssets()
        {
            for (var index = 0; index < Unused.Count; index++)
            {
                var item = Unused[index];
                if (Updater.Instance.busy)
                {
                    break;
                }

                if (!item.isDone)
                {
                    continue;
                }

                Unused.RemoveAt(index);
                index--;
                item.Unload();
            }
        }

        private void DownloadAsync(string url, string savePath)
        {
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }

            Logger.I("Load {0}", url);
            request = UnityWebRequest.Get(url);
            request.downloadHandler = new DownloadHandlerFile(savePath);
            request.SendWebRequest();
        }

        private string GetTemporaryPath(string filename)
        {
            return Versions.GetTemporaryPath(string.Format(builtin ? "Builtin/{0}" : "{0}", filename));
        }

        protected override void OnUpdate()
        {
            if (status == LoadableStatus.CheckVersion)
            {
                UpdateVersion();
            }
            else if (status == LoadableStatus.Downloading)
            {
                UpdateDownloading();
            }
            else if (status == LoadableStatus.Loading)
            {
                if (changed && !builtin)
                {
                    var assetPath = GetTemporaryPath(name);
                    asset.Load(assetPath);
                }

                Finish();
            }
        }

        private void UpdateDownloading()
        {
            if (request == null)
            {
                Finish("request == nul with " + status);
                return;
            }

            progress = 0.2f + request.downloadProgress;
            if (!request.isDone)
            {
                return;
            }

            if (!string.IsNullOrEmpty(request.error))
            {
                Finish(request.error);
                return;
            }

            request.Dispose();
            request = null;

            status = LoadableStatus.Loading;
        }

        private void UpdateVersion()
        {
            if (request == null)
            {
                Finish("request == null with " + status);
                return;
            }

            progress = 0.2f * request.downloadProgress;
            if (!request.isDone)
            {
                return;
            }

            if (!string.IsNullOrEmpty(request.error))
            {
                Finish(request.error);
                return;
            }

            request.Dispose();
            request = null;

            var savePath = GetTemporaryPath(Manifest.GetVersionFile(name));
            if (!File.Exists(savePath))
            {
                Finish("version not exist.");
                return;
            }

            assetVersion = ManifestVersion.Load(savePath);
            Logger.I("Read {0} with version {1} crc {2}", name, assetVersion.version, assetVersion.crc);
            var path = GetTemporaryPath(name);
            if (File.Exists(path))
            {
                using (var stream = File.OpenRead(path))
                {
                    if (Utility.ComputeCRC32(stream) == assetVersion.crc)
                    {
                        Logger.I("Skip to download {0}, because nothing to update.", name);
                        status = LoadableStatus.Loading;
                        return;
                    }
                }
            }

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            DownloadAsync(pathOrURL, path);
            status = LoadableStatus.Downloading;
        }
    }
}