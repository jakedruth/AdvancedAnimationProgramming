/*
	Advanced Animation Programming
	By Jake Ruth

    KeyframePool.cs - Hold a list of keyframes that will be accessed from a clip
*/

namespace AdvAnimation
{

    /// <summary>
    /// An unordered, unsorted collection of keyframes
    /// </summary>
    public struct KeyframePool
    {
        private readonly Keyframe[] _keyframes;

        /// <summary>
        /// Get the number of keyframes in this pool
        /// </summary>
        public int Count
        {
            get { return _keyframes.Length; }
        }

        /// <summary>
        /// Create a keyframe pool
        /// </summary>
        /// <param name="frames">an array of keyframes to be stored</param>
        public KeyframePool(params Keyframe[] frames)
        {
            _keyframes = frames;
            for (int i = 0; i < frames.Length; i++)
            {
                _keyframes[i].index = i;
            }
        }

        /// <summary>
        /// Get a key frame by index
        /// </summary>
        /// <param name="i">Index into the array of keyframes</param>
        /// <returns>The requested keyframe</returns>
        public Keyframe GetKeyframe(int i)
        {
            return _keyframes[i];
        }

        public Keyframe this[int i]
        {
            get { return _keyframes[i]; }
            set { _keyframes[i] = value; }
        }
    }
}
