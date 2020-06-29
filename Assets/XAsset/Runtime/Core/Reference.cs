//
// Reference.cs
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
using System.Collections.Generic;
using UnityEngine;

namespace libx
{  
	public class Reference
	{
		private List<Object> _requires;

		public bool IsUnused ()
		{
			if (_requires != null)
			{
				for (var i = 0; i < _requires.Count; i++)
				{
					var item = _requires[i];
					if (item != null)
						continue;
					Release();
					_requires.RemoveAt(i);
					i--;
				}
				if (_requires.Count == 0)
					_requires = null;
			}
			return refCount <= 0;
		}

		public int refCount;

		public void Retain ()
		{
			refCount++;
		}

		public void Release ()
		{
			refCount--;
		} 
		
		private bool checkRequires
		{
			get { return _requires != null; }
		}

		public void Require(Object obj)
		{
			if (_requires == null)
				_requires = new List<Object>();

			_requires.Add(obj);
			Retain();
		}

		public void Dequire(Object obj)
		{
			if (_requires == null)
				return;

			if (_requires.Remove(obj))
				Release();
		} 
	} 
}
