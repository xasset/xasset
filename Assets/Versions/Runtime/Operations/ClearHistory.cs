using System.Collections.Generic;
using System.IO;

namespace Versions
{
    public sealed class ClearHistory : Operation
    {
        public readonly List<string> files = new List<string>();
        private int count;

        public override void Start()
        {
            base.Start();
            files.AddRange(Directory.GetFiles(Versions.DownloadDataPath));
            var usedFiles = new List<string>();
            var manifest = Versions.Manifest;
            usedFiles.Add(Manifest.GetVersionFile(manifest.name));
            usedFiles.Add(manifest.name);
            foreach (var bundle in manifest.bundles)
            {
                if (string.IsNullOrEmpty(bundle.nameWithAppendHash))
                {
                    continue;
                }

                usedFiles.Add(bundle.nameWithAppendHash);
            }

            files.RemoveAll(file =>
            {
                var name = Path.GetFileName(file);
                return usedFiles.Contains(name);
            });
            count = files.Count;
        }

        protected override void Update()
        {
            if (status == OperationStatus.Processing)
            {
                while (files.Count > 0)
                {
                    progress = (count - files.Count) * 1f / count;
                    var file = files[0];
                    if (!File.Exists(file))
                    {
                        File.Delete(file);
                    }

                    files.RemoveAt(0);
                    if (Updater.Instance.busy)
                    {
                        break;
                    }
                }

                Finish();
            }
        }
    }
}