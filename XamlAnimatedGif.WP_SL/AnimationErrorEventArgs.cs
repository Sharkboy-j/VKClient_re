using System;

namespace XamlAnimatedGif
{
    public class AnimationErrorEventArgs : EventArgs
    {
        public Exception Exception { get; set; }

        public AnimationErrorKind Kind { get; set; }

        public AnimationErrorEventArgs(Exception exception, AnimationErrorKind kind)
        {
            this.Exception = exception;
            this.Kind = kind;
        }
    }
}
