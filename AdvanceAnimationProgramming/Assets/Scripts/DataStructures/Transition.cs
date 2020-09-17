﻿/*
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
            float deltaOutOfBounds = Math.Abs(keyframeTime);

            // TODO: separate all the pauses from there non-pause counterparts

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
                case TransitionType.FORWARD_PAUSE:
                    clipIndex = nextClip.index;
                    keyframeIndex = nextClip.firstKeyframe;
                    keyframeTime = deltaOutOfBounds;
                    playbackDirection = (transitionType == TransitionType.FORWARD_PLAYBACK)
                        ? PlaybackDirection.FORWARD
                        : PlaybackDirection.PAUSE;
                    break;
                case TransitionType.BACKWARD_PAUSE:
                case TransitionType.BACKWARD_PLAYBACK:
                    clipIndex = nextClip.index;
                    keyframeIndex = nextClip.lastKeyframe;
                    keyframeTime = nextClip[keyframeIndex].Duration - deltaOutOfBounds;
                    playbackDirection = (transitionType == TransitionType.BACKWARD_PLAYBACK)
                        ? PlaybackDirection.REVERSE
                        : PlaybackDirection.PAUSE;
                    break;
                case TransitionType.FORWARD_SKIP:
                case TransitionType.FORWARD_SKIP_PAUSE:
                    clipIndex = nextClip.index;
                    keyframeIndex = nextClip.firstKeyframe + 1;
                    keyframeTime = deltaOutOfBounds;
                    playbackDirection = (transitionType == TransitionType.FORWARD_SKIP)
                        ? PlaybackDirection.FORWARD
                        : PlaybackDirection.PAUSE;
                    break;
                case TransitionType.BACKWARD_SKIP:
                case TransitionType.BACKWARD_SKIP_PAUSE:
                    clipIndex = nextClip.index;
                    keyframeIndex = nextClip.lastKeyframe - 1;
                    keyframeTime = nextClip.Duration - deltaOutOfBounds;
                    playbackDirection = (transitionType == TransitionType.BACKWARD_SKIP)
                        ? PlaybackDirection.REVERSE
                        : PlaybackDirection.PAUSE;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
