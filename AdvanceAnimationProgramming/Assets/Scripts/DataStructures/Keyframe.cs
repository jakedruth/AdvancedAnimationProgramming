/*
	Advanced Animation Programming
	By Jake Ruth

    Keyframe.cs - Hold all the value for a key frame
*/

namespace AdvAnimation
{
    /// <summary>
    /// A discrete sample of value lasting a duration of time
    /// </summary>
    public struct Keyframe
    {
        public float time;
        public float value;
        public int index;
        private float _duration;
        private float _inverseDuration;

        /// <summary>
        /// The duration of this keyframe
        /// </summary>
        public float Duration
        {
            get { return _duration; }
            set
            {
                _duration = value;
                _inverseDuration = 1 / _duration;
            }
        }

        /// <summary>
        /// The inverse duration of this keyframe
        /// </summary>
        public float InverseDuration
        {
            get { return _inverseDuration; }
            set
            {
                _inverseDuration = value;
                _duration = 1 / _inverseDuration;
            }
        }

        /// <summary>
        /// Creates a keyframe
        /// </summary>
        /// <param name="start">The keyframe start time</param>
        /// <param name="end">The keyframe end time</param>
        /// <param name="value">The keyframe value</param>
        public Keyframe(float start, float end, float value)
        {
            this.value = value;
            time = start;

            _duration = end - start;
            _inverseDuration = 1 / _duration;
            
            index = -1;
        }
    }
}