using System;
using System.Runtime.CompilerServices;

namespace XamlAnimatedGif.Decoding
{
    internal struct GifColor
    {
        public byte R;// { get; set; }

        public byte G;// { get; set; }

        public byte B;// { get; set; }

        internal GifColor(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public override string ToString()
        {
            return string.Format("#{0:x2}{1:x2}{2:x2}", this.R, this.G, this.B);
        }
    }
}
