﻿using System;
using Strategy.GameObjectControl.Game_Objects.GameSave;
using Strategy.GameObjectControl.RuntimeProperty;

namespace Strategy.GameObjectControl.Game_Objects.GameTargets {
	/// <summary>
	/// Counting down the time required to complete the mission target.
	/// </summary>
	class SurviveTimeTarget : ITarget{

		TimeSpan time;

		Property<string> targetInfo;

		const string text1 = "You must survive ";
		const string text2 = "Target is completed. You survived given time.";

		/// <summary>
		/// Initializes time from argument to info Property.
		/// </summary>
		/// <param name="args">The arguments should have just one member (the time required to complete the mission target).</param>
		public SurviveTimeTarget(object[] args) {
			time = TimeSpan.FromSeconds(Convert.ToInt32(args[0]));
			targetInfo = new Property<string>(text1+ time.ToString());	
		}

		/// <summary>
		/// Subtracts elapsed time from the remaining time.
		/// </summary>
		/// <param name="delay">The delay between last two frames (seconds).</param>
		/// <returns>Returns if the time elapsed.</returns>
		public bool Check(float delay) {
			time -= TimeSpan.FromSeconds(delay);
			if (time < TimeSpan.Zero) {
				targetInfo.Value = text2;
				return true;
			} else {
				targetInfo.Value = text1 + time;
				return false;
			}

		}

		/// <summary>
		/// Return Property with a mission target info.
		/// </summary>
		/// <returns>Return reference to Property with a mission target info.</returns>
		public Property<string> GetTargetInfo() {
			return targetInfo;
		}

		/// <summary>
		/// Does nothing, target is already initialized.
		/// </summary>
		/// <returns>Returns always true.</returns>
		public bool Initialize() {
			return true;
		}

		/// <summary>
		/// Returns remaining time rounded to int.
		/// </summary>
		[ConstructorField(0, AttributeType.Basic)]
		private int RemainingTime {
			get { return (int)time.TotalSeconds; }
		}
	}
}
