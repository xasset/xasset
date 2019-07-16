//
// AssetDownload.cs
//
// Author:
//       fjy <jiyuan.feng@live.com>
//
// Copyright (c) 2019 fjy
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

namespace Plugins.XAsset
{
    public class Download
    {
        public enum State
        {
            HeadRequest,
            BodyRequest,
            FinishRequest,
            Completed,
        }

        public Action<float> onProgress;

        public Action completed;

        public float progress { get; private set; }

        public bool isDone { get; private set; }

        public string error;

        public long maxlen { get; private set; }

        public long len { get; private set; }

        public int index { get; private set; }

        public string url { get; set; }

        public string path { get; set; }

        public string savePath { get; set; }

        public string version { get; set; }

        public State state { get; private set; }

        private UnityWebRequest request { get; set; }

        private FileStream fs { get; set; }

        void WriteBuffer()
        {
            var buff = request.downloadHandler.data;
            if (buff != null)
            {
                var length = buff.Length - index;
                fs.Write(buff, index, length);
                index += length;
                len += length;
                progress = len / (float)maxlen;
            }
        }

        public void Update()
        {
            if (isDone)
            {
                return;
            }

            switch (state)
            {
                case State.HeadRequest:
                    if (request.error != null)
                    {
                        error = request.error;
                    }

                    if (request.isDone)
                    {
                        maxlen = long.Parse(request.GetResponseHeader("Content-Length"));
                        request.Dispose();
                        request = null;
                        var dir = Path.GetDirectoryName(savePath);
                        if (!Directory.Exists(dir))
                        {
                            // ReSharper disable once AssignNullToNotNullAttribute
                            Directory.CreateDirectory(dir);
                        } 
                        fs = new FileStream(savePath, FileMode.OpenOrCreate, FileAccess.Write);
                        len = fs.Length;
                        var emptyVersion = string.IsNullOrEmpty(version);
                        var oldVersion = Versions.Get(savePath);
                        var emptyOldVersion = string.IsNullOrEmpty(oldVersion);
                        if (emptyVersion || emptyOldVersion || !oldVersion.Equals(version))
                        {
                            Versions.Set(savePath, version); 
                            len = 0; 
                        }
                        if (len < maxlen)
                        { 
                            fs.Seek(len, SeekOrigin.Begin);
                            request = UnityWebRequest.Get(url);
                            request.SetRequestHeader("Range", "bytes=" + len + "-" + maxlen);
                            #if UNITY_2017_1_OR_NEWER
                            request.SendWebRequest();
                            #else
                            request.Send();
                            #endif
                            index = 0;
                            state = State.BodyRequest;
                        }
                        else
                        {
                            state = State.FinishRequest;
                        }
                    }

                    break;
                case State.BodyRequest:
                    if (request.error != null)
                    {
                        error = request.error;
                    }

                    if (!request.isDone)
                    {
                        WriteBuffer();
                    }
                    else
                    {
                        WriteBuffer();

                        if (fs != null)
                        {
                            fs.Close();
                            fs.Dispose();
                        }

                        request.Dispose();
                        state = State.FinishRequest;
                    }

                    break;

                case State.FinishRequest:
                    if (completed != null)
                    {
                        completed.Invoke();
                    }

                    isDone = true;
                    state = State.Completed;
                    break;
            }
        }

        public void Start()
        {
            request = UnityWebRequest.Head(url);
            #if UNITY_2017_1_OR_NEWER
            request.SendWebRequest();
            #else
            request.Send();
            #endif
            progress = 0;
            isDone = false;
        }

        public void Stop()
        {
            isDone = true;
        }
    }
}