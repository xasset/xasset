//
// Downloader.cs
//
// Author:
//       fjy <jiyuan.feng@live.com>
//
// Copyright (c) 2020 fjy
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace libx
{
    public class Downloader : MonoBehaviour
    {
        private const float BYTES_2_MB = 1f / (1024 * 1024);
        
        public int maxDownloads = 3;
        
        private readonly List<Download> _downloads = new List<Download>();
        private readonly List<Download> _tostart = new List<Download>();
        private readonly List<Download> _progressing = new List<Download>();
        public Action<long, long, float> onUpdate;
        public Action onFinished;

        private int _finishedIndex;
        private int _downloadIndex;
        private float _startTime;
        private float _lastTime;
        private long _lastSize;

        public long size { get; private set; }

        public long position { get; private set; }

        public float speed { get; private set; }

        public List<Download> downloads { get { return _downloads; } }

        private long GetDownloadSize()
        {
            var len = 0L;
            var downloadSize = 0L;
            foreach (var download in _downloads)
            {
                downloadSize += download.position;
                len += download.len;
            } 
            return downloadSize - (len - size);
        }

        private bool _started;
        [SerializeField]private float sampleTime = 0.5f;

        public void StartDownload()
        {
            _tostart.Clear(); 
            _finishedIndex = 0; 
            _lastSize = 0L;
            Restart();
        }

        public void Restart()
        {
            _startTime = Time.realtimeSinceStartup;
            _lastTime = 0;
            _started = true;
            _downloadIndex = _finishedIndex;
            var max = Math.Min(_downloads.Count, maxDownloads);
            for (var i = _finishedIndex; i < max; i++)
            {
                var item = _downloads[i];
                _tostart.Add(item);
                _downloadIndex++;
            }
        }

        public void Stop()
        {
            _tostart.Clear();
            foreach (var download in _progressing)
            {
                download.Complete(true); 
                _downloads[download.id] = download.Clone() as Download;

            } 
            _progressing.Clear();
            _started = false;
        }

        public void Clear()
        {
            size = 0;
            position = 0;
            
            _downloadIndex = 0;
            _finishedIndex = 0;
            _lastTime = 0f;
            _lastSize = 0L;
            _startTime = 0;
            _started = false; 
            foreach (var item in _progressing)
            {
                item.Complete(true);
            }
            _progressing.Clear();
            _downloads.Clear();
            _tostart.Clear();
        }

        public void AddDownload(string url, string filename, string savePath, string hash, long len)
        {
            var download = new Download
            {
                id = _downloads.Count,
                url = url,
                name = filename,
                hash = hash,
                len = len,
                savePath = savePath,
                completed = OnFinished
            };
            _downloads.Add(download);
            var info = new FileInfo(download.tempPath);
            if (info.Exists)
            {
                size += len - info.Length; 
            }
            else
            {
                size += len; 
            }
        }

        private void OnFinished(Download download)
        {
            if (_downloadIndex < _downloads.Count)
            {
                _tostart.Add(_downloads[_downloadIndex]);
                _downloadIndex++;    
            } 
            _finishedIndex++;
            Debug.Log(string.Format("OnFinished:{0}, {1}", _finishedIndex, _downloads.Count));
            if (_finishedIndex != downloads.Count)
                return;
            if (onFinished != null)
            {
                onFinished.Invoke(); 
            } 
            _started = false;
        }

        public static string GetDisplaySpeed(float downloadSpeed)
        {
            if (downloadSpeed >= 1024 * 1024)
            {
                return string.Format("{0:f2}MB/s", downloadSpeed * BYTES_2_MB);
            }
            if (downloadSpeed >= 1024)
            {
                return string.Format("{0:f2}KB/s", downloadSpeed / 1024);
            }
            return string.Format("{0:f2}B/s", downloadSpeed);
        }

        public static string GetDisplaySize(long downloadSize)
        {
            if (downloadSize >= 1024 * 1024)
            {
                return string.Format("{0:f2}MB", downloadSize * BYTES_2_MB);
            }
            if (downloadSize >= 1024)
            {
                return string.Format("{0:f2}KB", downloadSize / 1024);
            }
            return string.Format("{0:f2}B", downloadSize);
        }


        private void Update()
        {
            if (!_started)
                return; 
            
            if (_tostart.Count > 0)
            {
                for (var i = 0; i < Math.Min(maxDownloads, _tostart.Count); i++)
                {
                    var item = _tostart[i];
                    item.Start();
                    _tostart.RemoveAt(i);
                    _progressing.Add(item);
                    i--;
                }
            }

            for (var index = 0; index < _progressing.Count; index++)
            {
                var download = _progressing[index];
                download.Update();
                if (!download.finished)
                    continue;
                _progressing.RemoveAt(index);
                index--;
            }

            position = GetDownloadSize(); 
            
            var elapsed = Time.realtimeSinceStartup - _startTime;
            if (elapsed - _lastTime < sampleTime)
                return;
            
            var deltaTime = elapsed - _lastTime; 
            speed = (position - _lastSize) / deltaTime;
            if (onUpdate != null)
            {
                onUpdate(position, size, speed);
            }
            
            _lastTime = elapsed;  
            _lastSize = position;
        }
    }
}