using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AdvAnimation
{
    public class Keyframe<T>
    {
        private int _index;
        private float _duration;
        private float _inverseDuration;
        private T _data;

        public int Index
        {
            get { return _index; }
            set { _index = value; }
        }

        public float Duration
        {
            get { return _duration; }
            set
            {
                _duration = value;
                _inverseDuration = 1 / _duration;
            }
        }

        public float InverseDuration
        {
            get { return _inverseDuration; }
            set
            {
                _inverseDuration = value;
                _duration = 1 / _inverseDuration;
            }
        }

        public T Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public Keyframe(int index, float duration, T data)
        {
            Index = index;
            Duration = duration;
            Data = data;
        }
    }
}