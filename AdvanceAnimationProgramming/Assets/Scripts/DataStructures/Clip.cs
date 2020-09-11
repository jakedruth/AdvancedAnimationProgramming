/*
	Advanced Animation Programming
	By Jake Ruth

    Clip.cs - Hold all the data for a clip in an animation
*/

namespace AdvAnimation
{
    /// <summary>
    /// A collection of sequenced data of keyframes
    /// </summary>
    public class Clip
    {
        private readonly KeyframePool _pool;
        private float _duration;
        private float _inverseDuration;

        public string name;
        public int index;
        public int firstKeyframe;
        public int lastKeyframe;

        /// <summary>
        /// The duration of the clip
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
        /// The inverse duration of the clip
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
        /// Create a Clip
        /// </summary>
        /// <param name="name">The name of the clip</param>
        /// <param name="keyframePool">the keyframe pool to be used by this clip</param>
        /// <param name="firstKey">the first index of the clip</param>
        /// <param name="lastKey">the last index of the clip (inclusive!)</param>
        public Clip(string name, KeyframePool keyframePool, int firstKey, int lastKey)
        {
            this.name = name;
            index = -1;
            _pool = keyframePool;
            
            firstKeyframe = firstKey;
            if (firstKeyframe < 0)
                firstKeyframe = 0;
            else if (firstKeyframe > _pool.Count - 1)
                firstKeyframe = _pool.Count - 1;
            
            lastKeyframe = lastKey;
            if (lastKeyframe < 0)
                lastKeyframe = 0;
            else if (lastKeyframe > _pool.Count - 1)
                lastKeyframe = _pool.Count - 1;

            CalculateDuration();
        }

        /// <summary>
        /// Calculate the duration of this clip by summing up all keyframes in the pool from firstKeyframe to lastKeyframe
        /// </summary>
        private void CalculateDuration()
        {
            float duration = 0;
            for (int i = firstKeyframe; i <= lastKeyframe; i++)
            {
                duration += _pool.GetKeyframe(i).Duration;
            }

            Duration = duration;
        }

        /// <summary>
        /// Get a Keyframe
        /// </summary>
        /// <param name="i">Index into the array of keyframes</param>
        /// <returns>the requested keyframe</returns>
        public Keyframe GetKeyframe(int i)
        {
            return _pool.GetKeyframe(i);
        }
    }
}