using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AdvAnimation
{
    public class Clip<T>
    {
        private string _name;
        private int _index;
        private float _duration;
        private float _inverseDuration;
        public readonly KeyframePool<T> pool;

        public Keyframe<T> First
        {
            get { return pool.keyframes[0]; }
        }

        public Keyframe<T> Last
        {
            get { return pool.keyframes[pool.Count - 1]; }
        }

        public Clip(KeyframePool<T> keyframePool)
        {
            pool = keyframePool;
            CalculateDuration();
        }

        private void CalculateDuration()
        {
            float duration = 0;
            for (int i = 0; i < pool.Count; i++)
            {
                duration += pool.keyframes[i].Duration;
            }

            _duration = duration;
            _inverseDuration = 1 / duration;
        }
    }

}