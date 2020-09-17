/*
	Advanced Animation Programming
	By Jake Ruth

    ClipPool.cs - Hold a list of clips to be used by a controller
*/

namespace AdvAnimation
{
    /// <summary>
    /// A collection of clips
    /// </summary>
    public class ClipPool
    {
        private readonly Clip[] _clips;

        /// <summary>
        /// The amount of clips in this pool
        /// </summary>
        public int Count
        {
            get { return _clips.Length; }
        }

        /// <summary>
        /// Create a ClipPool
        /// </summary>
        /// <param name="clips">an array of clips to be stored</param>
        public ClipPool(params Clip[] clips)
        {
            _clips = clips;
            for (int i = 0; i < _clips.Length; i++)
            {
                _clips[i].index = i;
            }
        }

        /// <summary>
        /// Get a Clip by index
        /// </summary>
        /// <param name="i">Index into the array of clip</param>
        /// <returns>The requested clip</returns>
        private Clip GetClipByIndex(int i)
        {
            return _clips[i];
        }

        /// <summary>
        /// Get a Clip by name
        /// </summary>
        /// <param name="name">The clip's name</param>
        /// <returns>-1 if no clip with the supplied name. Otherwise, the index of the Clip in the array</returns
        public int GetClipIndexByName(string name)
        {
            for (int i = 0; i < Count; i++)
            {
                if (_clips[i].name == name)
                    return i;
            }

            return -1;
        }

        public Clip this[string name]
        {
            get { return _clips[GetClipIndexByName(name)]; }
            set { _clips[GetClipIndexByName(name)] = value; }
        }

        public Clip this[int i]
        {
            get { return _clips[i]; }
            set { _clips[i] = value; }
        }
    }
}
