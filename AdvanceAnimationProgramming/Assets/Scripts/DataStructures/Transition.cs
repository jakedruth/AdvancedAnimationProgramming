/*
	Advanced Animation Programming
	By Jake Ruth

    Transition.cs - Holds info for a transition to another clip
*/

using System;

namespace AdvAnimation
{
    public enum TransitionType
    {
        PAUSE,                  // Pause on current clip
        FORWARD_PLAYBACK,       // Play from the start of the first frame of the supplied clip
        FORWARD_PAUSE,          // Pause at start of the first frame of the supplied clip
        BACKWARD_PLAYBACK,      // Rewind from end of the last frame of the supplied clip
        BACKWARD_PAUSE,         // Pause at end of the last frame of the supplied clip
        FORWARD_SKIP,           // Play from end of the first frame of the supplied clip
        FORWARD_SKIP_PAUSE,     // Pause at the end of the first frame of supplied clip
        BACKWARD_SKIP,          // Rewind from the start of the last frame of the supplied clip
        BACKWARD_SKIP_PAUSE,    // pause at the start of the last frame of the supplied clip
    }

    public class Transition
    {
        public TransitionType transitionType;
        public Clip transitionClip;

        public Transition(TransitionType type, Clip clip = null)
        {
            transitionType = type;
            transitionClip = clip;
        }

        public void HandleTransition(ClipPool pool, ref int clipIndex, ref int keyframeIndex, 
            ref float keyframeTime, ref PlaybackDirection playbackDirection)
        {
            Clip nextClip = transitionClip ?? pool[clipIndex];

            switch (transitionType)
            {
                case TransitionType.PAUSE:

                    switch (playbackDirection)
                    {
                        case PlaybackDirection.FORWARD:
                            keyframeIndex = nextClip.lastKeyframe;
                            keyframeTime = nextClip[keyframeIndex].Duration;
                            break;
                        case PlaybackDirection.REVERSE:
                            keyframeIndex = nextClip.firstKeyframe;
                            keyframeTime = 0;
                            break;
                        case PlaybackDirection.PAUSE:
                            throw new Exception("A paused clip shouldn't be able to transition...");
                        default:
                            throw new ArgumentOutOfRangeException(nameof(playbackDirection), playbackDirection, null);
                    }

                    playbackDirection = PlaybackDirection.PAUSE;
                    break;
                case TransitionType.FORWARD_PLAYBACK:
                    break;
                case TransitionType.FORWARD_PAUSE:
                    break;
                case TransitionType.BACKWARD_PLAYBACK:
                    break;
                case TransitionType.BACKWARD_PAUSE:
                    break;
                case TransitionType.FORWARD_SKIP:
                    break;
                case TransitionType.FORWARD_SKIP_PAUSE:
                    break;
                case TransitionType.BACKWARD_SKIP:
                    break;
                case TransitionType.BACKWARD_SKIP_PAUSE:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
