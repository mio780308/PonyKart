﻿using Ponykart.Stuff;

namespace Ponykart.Players {
	public class HumanPlayer : Player {
		KeyBindingManager bindings;

		public HumanPlayer(int id) : base(id) {

			// hook up to input events
			bindings = LKernel.Get<KeyBindingManager>();

			bindings.PressEventsDict[LKey.Accelerate] += OnPressAccelerate;
			bindings.ReleaseEventsDict[LKey.Accelerate] += OnReleaseAccelerate;
			bindings.PressEventsDict[LKey.Brake] += OnPressBrake;
			bindings.ReleaseEventsDict[LKey.Brake] += OnReleaseBrake;
			bindings.PressEventsDict[LKey.Reverse] += OnPressReverse;
			bindings.ReleaseEventsDict[LKey.Reverse] += OnReleaseReverse;
			bindings.PressEventsDict[LKey.TurnLeft] += OnPressTurnLeft;
			bindings.ReleaseEventsDict[LKey.TurnLeft] += OnReleaseTurnLeft;
			bindings.PressEventsDict[LKey.TurnRight] += OnPressTurnRight;
			bindings.ReleaseEventsDict[LKey.TurnRight] += OnReleaseTurnRight;
		}

		#region key events
		protected void OnPressAccelerate(LKey k) {
			// do nothing if the brake is pressed
			if (!bindings.IsKeyPressed(LKey.Brake)) {
				// if we have both forward and reverse pressed at the same time, do nothing
				if (bindings.IsKeyPressed(LKey.Reverse))
					Kart.Accelerate(0);
				// otherwise go forwards
				else
					Kart.Accelerate(Kart.ForwardSpeed);
			}
		}
		protected void OnReleaseAccelerate(LKey k) {
			// if reverse is still held down, then we start reversing
			if (bindings.IsKeyPressed(LKey.Reverse))
				Kart.Accelerate(Kart.ReverseSpeed);
			// otherwise we just stop accelerating
			else
				Kart.Accelerate(0);
		}


		protected void OnPressBrake(LKey k) {
			Kart.Brake();
		}
		protected void OnReleaseBrake(LKey k) {
			if (bindings.IsKeyPressed(LKey.Accelerate)) {
				if (bindings.IsKeyPressed(LKey.Reverse))
					Kart.Accelerate(0);
				else
					Kart.Accelerate(Kart.ForwardSpeed);
			}
			else if (bindings.IsKeyPressed(LKey.Reverse))
				Kart.Accelerate(Kart.ReverseSpeed);
			else
				Kart.Accelerate(0);
		}


		protected void OnPressReverse(LKey k) {
			// do nothing if the brake is pressed
			if (!bindings.IsKeyPressed(LKey.Brake)) {
				// if we have both forward and reverse pressed at the same time, do nothing
				if (bindings.IsKeyPressed(LKey.Accelerate))
					Kart.Accelerate(0);
				// otherwise go backwards
				else
					Kart.Accelerate(Kart.ReverseSpeed);
			}
		}
		protected void OnReleaseReverse(LKey k) {
			// if forward is still held down, then we start going forwards
			if (bindings.IsKeyPressed(LKey.Accelerate))
				Kart.Accelerate(Kart.ForwardSpeed);
			// otherwise we just stop accelerating
			else
				Kart.Accelerate(0);
		}


		protected void OnPressTurnLeft(LKey k) {
			// if both turns are pressed, we go straight
			if (bindings.IsKeyPressed(LKey.TurnRight))
				Kart.Turn(0);
			// otherwise go left
			else
				Kart.Turn(Kart.TurnAngle);
		}
		protected void OnReleaseTurnLeft(LKey k) {
			// if right is still pressed, turn right
			if (bindings.IsKeyPressed(LKey.TurnRight))
				Kart.Turn(-Kart.TurnAngle);
			// otherwise go straight
			else
				Kart.Turn(0);
		}


		protected void OnPressTurnRight(LKey k) {
			// if both turns are pressed, we go straight
			if (bindings.IsKeyPressed(LKey.TurnLeft))
				Kart.Turn(0);
			// otherwise go right
			else
				Kart.Turn(-Kart.TurnAngle);
		}
		protected void OnReleaseTurnRight(LKey k) {
			// if left is still pressed, turn left
			if (bindings.IsKeyPressed(LKey.TurnLeft))
				Kart.Turn(Kart.TurnAngle);
			// otherwise go straight
			else
				Kart.Turn(0);
		}
		#endregion


		protected override void UseItem() {
			throw new System.NotImplementedException();
		}


		public override void Dispose() {
			bindings.PressEventsDict[LKey.Accelerate] -= OnPressAccelerate;
			bindings.ReleaseEventsDict[LKey.Accelerate] -= OnReleaseAccelerate;
			bindings.PressEventsDict[LKey.Brake] -= OnPressBrake;
			bindings.ReleaseEventsDict[LKey.Brake] -= OnReleaseBrake;
			bindings.PressEventsDict[LKey.Reverse] -= OnPressReverse;
			bindings.ReleaseEventsDict[LKey.Reverse] -= OnReleaseReverse;
			bindings.PressEventsDict[LKey.TurnLeft] -= OnPressTurnLeft;
			bindings.ReleaseEventsDict[LKey.TurnLeft] -= OnReleaseTurnLeft;
			bindings.PressEventsDict[LKey.TurnRight] -= OnPressTurnRight;
			bindings.ReleaseEventsDict[LKey.TurnRight] -= OnReleaseTurnRight;

			base.Dispose();
		}
	}
}