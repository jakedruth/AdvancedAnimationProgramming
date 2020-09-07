using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AdvAnimation
{
    public class KeyframePool<T>
    {
        public Keyframe<T>[] keyframes;

        public int Count
        {
            get { return keyframes.Length; }
        }

        public KeyframePool(params Keyframe<T>[] frames)
        {
            keyframes = frames;
        }
    }
}
