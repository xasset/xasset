using System.Collections.Generic;
using System.IO;
using System.Text;

namespace xasset.editor
{
    /// <summary>
    ///     清理打包的历史文件
    /// </summary>
    public static class ClearHistory
    {
        public static void Start()
        {
            var usedFiles = new List<string>
            {
                Settings.GetCachePath(Settings.Platform.ToString()),
                Settings.GetCachePath($"{Settings.Platform}.manifest"),
                Settings.GetCachePath(Versions.Filename),
                Settings.GetCachePath(UpdateInfo.Filename)
            };

            var updateInfo = Utility.LoadFromFile<UpdateInfo>(Settings.GetCachePath(UpdateInfo.Filename));
            usedFiles.Add(Settings.GetDataPath(updateInfo.file));
            var versions = Utility.LoadFromFile<Versions>(Settings.GetDataPath(updateInfo.file));

            foreach (var version in versions.data)
            {
                usedFiles.Add(Settings.GetCachePath($"{version.name}.json"));
                usedFiles.Add(Settings.GetDataPath(version.file));
                var manifest = Utility.LoadFromFile<Manifest>(Settings.GetDataPath(version.file));
                foreach (var bundle in manifest.bundles)
                {
                    usedFiles.Add(Settings.GetCachePath(bundle.name));
                    usedFiles.Add(Settings.GetCachePath($"{bundle.name}.manifest"));
                    usedFiles.Add(Settings.GetDataPath(bundle.file));
                }
            }

            var files = new List<string>();
            var dirs = new[] {Settings.PlatformDataPath, Settings.PlatformCachePath};
            foreach (var dir in dirs)
                if (Directory.Exists(dir))
                    files.AddRange(Directory.GetFiles(dir, "*", SearchOption.AllDirectories));

            var sb = new StringBuilder();
            sb.AppendLine("Delete files:");
            foreach (var file in files)
            {
                var path = file.Replace("\\", "/");
                if (usedFiles.Exists(path.Equals))
                    continue;

                File.Delete(path);
                sb.AppendLine(path);
            }

            Logger.I(sb);
        }
    }
}