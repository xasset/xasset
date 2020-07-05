//
// Download.cs
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
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace libx
{
    public class Download : DownloadHandlerScript, System.Collections.IEnumerator
    {
        private static readonly byte[] PreallocatedBuffer = new byte[1024 * 1024 * 4]; 
        public string error { get; private set; }
        public long len { get; set; }
        public string hash { get; set; }
        public string url { get; set; }
        public long position { get; private set; }

        public string tempPath
        {
            get { return Application.persistentDataPath + "/temp_" + hash; }
        }

        public Action<Download> completed { get; set; }
        private UnityWebRequest _request;
        private FileStream _stream;
        private bool _downloading;

        protected override float GetProgress()
        {
            return position * 1f / len;
        }

        protected override byte[] GetData()
        {
            return null;
        }

        protected override void ReceiveContentLength(int contentLength)
        {
        }

        protected override bool ReceiveData(byte[] buffer, int dataLength)
        {
            if (!string.IsNullOrEmpty(_request.error))
            {
                error = _request.error;
                return true;
            }

            _stream.Write(buffer, 0, dataLength);
            position += dataLength;
            return _downloading;
        }

        protected override void CompleteContent()
        {
            Complete();
        }

        public Download() : base(PreallocatedBuffer)
        {
        }

        public override string ToString()
        {
            return string.Format("{0}, size:{1}, hash:{2}", url, len, hash);
        }

        public void Start()
        {
            _downloading = true;
            _stream = new FileStream(tempPath, FileMode.OpenOrCreate, FileAccess.Write);
            position = _stream.Length;
            if (position < len)
            {
                _stream.Seek(position, SeekOrigin.Begin);
                _request = UnityWebRequest.Get(url);
                _request.SetRequestHeader("Range", "bytes=" + position + "-");
                _request.downloadHandler = this;
                _request.SendWebRequest();
            }
            else
            {
                Complete();
            }
        }

        private void Complete()
        {
            _downloading = false;
            if (_stream != null)
            {
                _stream.Close();
                _stream.Dispose();
                _stream = null;
            }

            if (_request != null)
            {
                _request.Dispose();
                _request = null;
            }

            if (string.IsNullOrEmpty(error))
            {
                if (File.Exists(tempPath))
                {
                    using (var fs = File.OpenRead(tempPath))
                    {
                        if (fs.Length != len)
                        {
                            error = "下载文件长度异常:" + fs.Length;
                        }

                        if (Versions.verifyBy == VerifyBy.Hash)
                        {
                            var compare = StringComparison.OrdinalIgnoreCase;
                            if (!hash.Equals(Utility.GetCRC32Hash(fs), compare))
                            {
                                error = "下载文件哈希异常:" + hash;
                            }
                        }
                    }
                }
                else
                {
                    error = "保存下载失败";
                }
            }

            if (completed == null) return;
            completed.Invoke(this);
            completed = null;
        }

        public bool MoveNext()
        {
            return !isDone;
        }

        public void Reset()
        {
        }

        public object Current
        {
            get { return null; }
        }
    }
}