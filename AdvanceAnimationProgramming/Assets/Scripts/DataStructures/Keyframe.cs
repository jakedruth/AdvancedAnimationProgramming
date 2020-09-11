/*
	Advanced Animation Programming
	By Jake Ruth

    Keyframe.cs - Hold all the data for a key frame
*/

namespace AdvAnimation
{
    /// <summary>
    /// A discrete sample of data lasting a duration of time
    /// </summary>
    public class Keyframe
    {
        public int index;
        public float data;
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
        /// <param name="duration">How long in seconds the keyframe will last</param>
        /// <param name="data">The value of this keyframe</param>
        public Keyframe(float duration, float data)
        {
            index = -1;
            Duration = duration;
            this.data = data;
        }
    }
}