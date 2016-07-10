/*  
* AllJoyn Device Service Bridge for Simulated devices
*  
* Copyright (c) Morten Nielsen
* All rights reserved.  
*  
* MIT License  
*  
* Permission is hereby granted, free of charge, to any person obtaining a copy of this  
* software and associated documentation files (the "Software"), to deal in the Software  
* without restriction, including without limitation the rights to use, copy, modify, merge,  
* publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons  
* to whom the Software is furnished to do so, subject to the following conditions:  
*  
* The above copyright notice and this permission notice shall be included in all copies or  
* substantial portions of the Software.  
*  
* THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,  
* INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR  
* PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE  
* FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR  
* OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER  
* DEALINGS IN THE SOFTWARE.  
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Storage;
using BridgeRT;

namespace AdapterLib
{
    internal class AdapterIcon : BridgeRT.IAdapterIcon
    {
        byte[] _image = null;
        public AdapterIcon(string url)
        {
            if (url.StartsWith("ms-appx:///"))
            {
                var s = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromUri(new Uri(url)).OpenReadAsync().AsTask();
                s.Wait();
                using (MemoryStream ms = new MemoryStream())
                {
                    s.Result.AsStreamForRead().CopyTo(ms);
                    _image = ms.ToArray();
                }
            }
            else
            {
                Url = url;
            }
        }

        public string MimeType { get; } = "image/png";

        public string Url { get; } = "";

        public byte[] GetImage()
        {
            return _image;
        }
    }
}
