﻿using Strategy.GameObjectControl.Game_Objects.GameSave;
using Strategy.GameObjectControl.RuntimeProperty;

namespace Strategy.GameObjectControl.Game_Objects.GameTargets {
	/// <summary>
	/// Target is destroy the entire team to the last unit.
	/// </summary>
	class EliminateTeamTarget : ITarget {

		[ConstructorField(0, AttributeType.Basic)]
		string teamName;
		Property<string> targetInfo;
		TeamControl.Team team;

		const string text1 = "You must eliminate team ";
		const string text2 = "Target is completed. You eliminated team ";

		/// <summary>
		/// Sets data to initialization: a team-taget to info Property.
		/// </summary>
		/// <param name="args">The arguments should contains just a team name.</param>
		public EliminateTeamTarget(object[] args) {
			teamName = (string)args[0];
			targetInfo = new Property<string>(text1 + teamName);
		}
	
		/// <summary>
		/// Controls if the team-target has any member.
		/// </summary>
		/// <param name="delay">The delay between last two frames (seconds).</param>
		/// <returns>Returns if the team is without members.</returns>
		public bool Check(float delay) {
			if (team.Count == 0) {
				targetInfo.Value = text2 + teamName;
				return true;
			} else {
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
		/// Initializes mission target: gets the team-target.
		/// </summary>
		/// <returns>Returns if initialization was successful.</returns>
		public bool Initialize() {
			team = Game.TeamManager.GetTeam(teamName);
			// Team does't exist = eliminated
			if (team == null) {
				return false;
			} else {
				return true;
			}
		}
	}
}
