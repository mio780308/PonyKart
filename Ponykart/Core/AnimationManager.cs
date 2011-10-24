﻿using System.Collections.Generic;
using Mogre;
using Ponykart.Levels;

namespace Ponykart.Core {
	/// <summary>
	/// Instead of each ModelComponent hooking up to FrameStarted to update any animation it might have, we just give it 
	/// to this class to update it for us. This means we only have to hook one FrameStarted method up instead of loads of them.
	/// </summary>
	public class AnimationManager {
		private IList<AnimationState> states;

		public AnimationManager() {
			LevelManager.OnLevelLoad += new LevelEvent(OnLevelLoad);
			LevelManager.OnLevelUnload += new LevelEvent(OnLevelUnload);

			states = new List<AnimationState>();
		}

		/// <summary>
		/// hook up to the frame started event
		/// </summary>
		void OnLevelLoad(LevelChangedEventArgs eventArgs) {
			LKernel.GetG<Root>().FrameStarted += FrameStarted;
		}

		/// <summary>
		/// clear our states list and disconnect from the frame started event
		/// </summary>
		void OnLevelUnload(LevelChangedEventArgs eventArgs) {
			states.Clear();
			LKernel.GetG<Root>().FrameStarted -= FrameStarted;
		}

		/// <summary>
		/// update all of our animations, but only if we aren't paused
		/// </summary>
		bool FrameStarted(FrameEvent evt) {
			foreach (AnimationState state in states) {
				if (!Pauser.IsPaused)
					state.AddTime(evt.timeSinceLastFrame);
			}
			return true;
		}

		/// <summary>
		/// Add an animation to be automatically updated
		/// </summary>
		public void Add(AnimationState state) {
			states.Add(state);
		}

		/// <summary>
		/// Remove an animation from being automatically updated
		/// </summary>
		public void Remove(AnimationState state) {
			states.Remove(state);
		}
	}
}
