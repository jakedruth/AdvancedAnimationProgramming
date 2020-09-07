using System.Collections;
using System.Collections.Generic;
using AdvAnimation;

namespace AdvAnimation
{
    public class ClipPool<T>
    {
        public Clip<T>[] clips;

        public int Count
        {
            get { return clips.Length; }
        }

        public ClipPool(params Clip<T>[] clips)
        {
            this.clips = clips;
        }

        public int GetClip(string name)
        {
            return -1;
        }
    }
}
