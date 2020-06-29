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
    public class Download : System.Collections.IEnumerator
    { 
        public bool isDone { get; private set; }
        public string error { get; private set; }
        public long len { get; set; }
        public uint crc { get; set; }
        public string url { get; set; }
        public long position { get; private set; }

        public string tempPath
        {
            get { return Application.persistentDataPath + "/temp/" + crc; }
        }

        public Action<Download> completed { get; set; }
        private UnityWebRequest _request;
        private FileStream _stream;
        private int _index;
        private bool _started;

        public object Current
        {
            get { return null; }
        }

        private void WriteBuffer()
        {
            var buff = _request.downloadHandler.data;
            if (buff == null) return;
            var length = buff.Length - _index;
            _stream.Write(buff, _index, length);
            _index += length;
            position += length;
        }

        public void Update()
        {
            if (isDone)
            {
                return;
            }

            if (!_started) return;
            
            if (!string.IsNullOrEmpty(_request.error))
            {
                error = _request.error;
                Complete();
                return;
            } 

            if (_request.isDone)
            {
                WriteBuffer();
                Complete();
            }
            else
            {
                WriteBuffer();
            }
        }

        public void Start()
        { 
            _stream = new FileStream(tempPath, FileMode.OpenOrCreate, FileAccess.Write);
            position = _stream.Length;
            if (position < len)
            {
                _stream.Seek(position, SeekOrigin.Begin);
                _request = UnityWebRequest.Get(url);
                _request.SetRequestHeader("Range", "bytes=" + position + "-");
                _request.SendWebRequest(); 
                _index = 0; 
                isDone = false;
                _started = true;
            }
            else
            {
                Complete(); 
                isDone = true;
            }
        }

        private void Complete()
        {
            if (isDone)
            {
                return;
            }

            if (_stream != null)
            {
                _stream.Close();
                _stream.Dispose();
            }

            if (_request != null)
            {
                _request.Dispose();
                _request = null;
            }

            if (completed != null)
            {
                completed.Invoke(this);
            }
            
            isDone = true;
        }

        public bool MoveNext()
        {
            return !isDone;
        }

        public void Reset()
        {
        }
    }
}