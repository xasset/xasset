//
// BackgroundAdapter.cs
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

using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

namespace libx
{
    [ExecuteInEditMode]
    public class BackgroundAdapter : MonoBehaviour
    {
        private CanvasScaler _scaler;

        public void OnStart()
        {
            UpdateScale();
        }

        private void UpdateScale()
        {
            if (_scaler == null) _scaler = GetComponentInParent<CanvasScaler>();

            var resolution = _scaler.referenceResolution;
            var rt = _scaler.transform as RectTransform;
            if (rt == null) return;
            var screenSize = rt.sizeDelta;
            var factor = Mathf.Max(screenSize.x / resolution.x, screenSize.y / resolution.y);
            var scale = Vector3.one * factor;
            transform.localScale = scale;
        }

        [Conditional("UNITY_EDITOR")]
        private void Update()
        {
            UpdateScale();
        }
    }
}