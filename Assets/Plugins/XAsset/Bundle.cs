//
// Bundle.cs
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

using System.Collections.Generic;
using UnityEngine;

namespace Plugins.XAsset
{
    public class Bundle : Asset
    {
        public readonly List<Bundle> dependencies = new List<Bundle>();

        public AssetBundle assetBundle
        {
            get { return asset as AssetBundle; }
            internal set { asset = value; }
        }

        internal override void Load()
        {
            asset = AssetBundle.LoadFromFile(name);
            if (assetBundle == null) error = name + " LoadFromFile failed.";
        }

        internal override void Unload()
        {
            if (assetBundle == null) return;
            assetBundle.Unload(true);
            assetBundle = null;
        }
    }

    public class BundleAsync : Bundle
    {
        private AssetBundleCreateRequest _request;

        public override bool isDone
        {
            get { return _request == null || _request.isDone; }
        }

        public override float progress
        {
            get { return _request.progress; }
        }

        internal override void Load()
        {
            _request = AssetBundle.LoadFromFileAsync(name);
            if (_request == null)
            {
                error = name + " LoadFromFile failed.";
                return;
            }

            if (_request != null) _request.completed += Request_completed;
        }

        private void Request_completed(AsyncOperation obj)
        {
            asset = _request.assetBundle;
        }

        internal override void Unload()
        {
            if (_request != null)
            {
                _request.completed -= Request_completed;
                _request = null;
            }

            base.Unload();
        }
    }

    public class WebBundle : Bundle
    {
        private WWW _request;
        public bool cache;
        public Hash128 hash;

        public override string error
        {
            get { return _request.error; }
        }

        public override bool isDone
        {
            get
            {
                if (_request == null) return true;
                if (_request.isDone) assetBundle = _request.assetBundle;
                return _request.isDone;
            }
        }

        public override float progress
        {
            get { return _request.progress; }
        }

        internal override void Load()
        {
            _request = cache ? WWW.LoadFromCacheOrDownload(name, hash) : new WWW(name);
        }

        internal override void Unload()
        {
            if (_request != null)
            {
                _request.Dispose();
                _request = null;
            }

            base.Unload();
        }
    }
}