﻿using System;
using System.Collections.Generic;
using Strategy.GameObjectControl;
using Strategy.GameObjectControl.Game_Objects;
using Strategy.GameObjectControl.Game_Objects.MovableGameObjectBox;
using Strategy.GameObjectControl.GroupMgr;

namespace Strategy.FightMgr {

	/// <summary>
	/// Controls every fight and occupation. Creating them, updating them and stopping them.
	/// </summary>
	class FightManager : IFightManager {

		// Dictionary with information about group action (Occupy/Attack)
		private Dictionary<GroupMovables, ActionAnswer> offensiveActionDict;

		// Occupations
		private List<Occupation> occupationList;

		// Fights
		private List<Fight> fightList;

		// Objects on the way
		private Dictionary<IMovableGameObject, GroupMovables> onWayToTargetDict;

		// Informations about which object attack an other
		private Dictionary<IMovableGameObject, IGameObject> attackersTarget;

		/// <summary>
		/// Creates instance of the FightManager and initializes objects. (FightManager is not initialized.)
		/// </summary>
		public FightManager() {
			attackersTarget = new Dictionary<IMovableGameObject, IGameObject>();
			onWayToTargetDict = new Dictionary<IMovableGameObject, GroupMovables>();
			offensiveActionDict = new Dictionary<GroupMovables, ActionAnswer>();
			occupationList = new List<Occupation>();
			fightList = new List<Fight>();
		}

		#region Public

		/// <summary>
		/// Initializes the FightManager from two given lists (occupation and fights).
		/// </summary>
		/// <param name="loadedOcc">The list with occupations.</param>
		/// <param name="loadedFights">The list with fights.</param>
		public void Initialize(List<Tuple<List<string>, string, int>> loadedOcc, List<Tuple<List<string>, List<string>>> loadedFights) {
			// Occupations
			if (loadedOcc != null) {
				foreach (var occupation in loadedOcc) {
					IMovableGameObject firstObj = null;
					foreach (var gameObject in occupation.Item1) {
						if (Game.HitTest.IsObjectControllable(gameObject) && Game.HitTest.IsObjectMovable(gameObject)) {
							firstObj = Game.HitTest.GetIMGO(gameObject);
							break;
						}
					}
					if (firstObj == null) {
						// Invalid data
						continue;
					}
					GroupMovables group = Game.GroupManager.GetGroup(firstObj);

					// Creates group
					for (int i = 1; i < occupation.Item1.Count; i++) {
						if (Game.HitTest.IsObjectControllable(occupation.Item1[i]) && Game.HitTest.IsObjectMovable(occupation.Item1[i])) {
							Game.GroupManager.AddToGroup(group, Game.HitTest.GetIMGO(occupation.Item1[i]));
						}
					}

					if (group.Count == 0) {
						// Invalid data
						continue;
					}
					IGameObject target;
					if (Game.HitTest.IsObjectControllable(occupation.Item2)) {
						target = Game.HitTest.GetGameObject(occupation.Item2);
					} else {
						// Invalid data
						continue;
					}

					if (target.OccupyTime == occupation.Item3) {
						Occupy(group, target);
					} else {
						// Create Occupation
						occupationList.Add(new Occupation(group, target, TimeSpan.FromSeconds(occupation.Item3)));
					}

				}
			}

			// Fights
			if (loadedFights != null) {
				foreach (var fight in loadedFights) {
					var firstObj = Game.HitTest.GetIMGO(fight.Item1[0]);
					GroupMovables group1 = Game.GroupManager.GetGroup(firstObj);
					// Creates group1
					for (int i = 1; i < fight.Item1.Count; i++) {
						if (Game.HitTest.IsObjectControllable(fight.Item1[i]) && Game.HitTest.IsObjectMovable(fight.Item1[i])) {
							Game.GroupManager.AddToGroup(group1, Game.HitTest.GetIMGO(fight.Item1[i]));
						}
					}

					if (group1.Count == 0) {
						continue;
					}

					// Second group can contains the static members
					IGameObject firstObj1 = null;
					GroupMovables group2 = null;
					foreach (var gameObject in fight.Item2) {
						if (Game.HitTest.IsObjectControllable(gameObject)) {
							firstObj1 = Game.HitTest.GetGameObject(gameObject);
							group2 = new GroupMovables(firstObj1.Team);
							break;
						}
					}
					GroupStatics group3 = new GroupStatics();

					if (Game.HitTest.IsObjectControllable(fight.Item2[0])) {
						if (Game.HitTest.IsObjectMovable(fight.Item2[0])) {
							var firstObj2 = Game.HitTest.GetIMGO(fight.Item2[0]);
							group2 = Game.GroupManager.GetGroup(firstObj2);
						} else {
							var firstObj3 = Game.HitTest.GetISGO(fight.Item2[0]);
							group3 = new GroupStatics(firstObj3.Team);
							group3.InsertMemeber(firstObj3);
						}
					}

					if (group2 == null || group3 == null || group2.Count == 0 && group3.Count == 0) {
						// Invalid data
						continue;
					}

					for (int i = 1; i < fight.Item2.Count; i++) {
						if (Game.HitTest.IsObjectMovable(fight.Item2[i])) {
							Game.GroupManager.AddToGroup(group2, Game.HitTest.GetIMGO(fight.Item2[i]));
						} else {
							group3.InsertMemeber(Game.HitTest.GetISGO(fight.Item2[i]));
						}
					}
					fightList.Add(new Fight(group1, group2, group3));
				}
			}
		}

		/// <summary>
		/// Updates all fights and occupations.
		/// </summary>
		/// <param name="delay">The delay between last two frames (seconds).</param>
		public void Update(float delay) {
			var occupCopy = new List<Occupation>(occupationList);
			foreach (var occ in occupCopy) {
				if (occ.Check(delay)) {
					occupationList.Remove(occ);
				}
			}

			var fightCopy = new List<Fight>(fightList);
			foreach (var fight in fightCopy) {
				if (fight.CheckDistance()) {
					fightList.Remove(fight);
				}
			}
		}

		/// <summary>
		/// Sends attacker to a fight destiantion when it is possible (CheckActionPossibility).
		/// </summary>
		/// <param name="group">The attacking group.</param>
		/// <param name="gameObject">The attacked object.</param>
		public void Attack(GroupMovables group, IGameObject gameObject) {

			// Check if target is already under attack of the this group or if the target is under attack so
			// group is added to deffenders.
			if (!CheckActionPossibility(group, gameObject)) {
				return;
			}

			Game.IMoveManager.GoToTarget(group, gameObject, this);
			offensiveActionDict.Add(group, ActionAnswer.Attack);

			foreach (IMovableGameObject imgo in group) {
				attackersTarget.Add(imgo, gameObject);
				onWayToTargetDict.Add(imgo, group);
			}
		}

		/// <summary>
		/// Controls if target can be occupied or if is now currently occupied.
		/// If it is possible so attackers are sent to the destination and are registered to the FightManager.
		/// </summary>
		/// <param name="group">The attacking group.</param>
		/// <param name="gameObject">The occupation target.</param>
		public void Occupy(GroupMovables group, IGameObject gameObject) {

			if (gameObject.OccupyTime < 0) {
				return;
			}

			// The object is occupied by this group 
			if (offensiveActionDict.ContainsKey(group) &&
				offensiveActionDict[group] == ActionAnswer.Occupy &&
				attackersTarget[group[0]] == gameObject) {
				return;
			}

			if (!CheckActionPossibility(group, gameObject)) {
				return;
			}

			// Object can be occupied
			Game.IMoveManager.GoToTarget(group, gameObject, this);
			offensiveActionDict.Add(group, ActionAnswer.Occupy);

			foreach (IMovableGameObject imgo in group) {
				attackersTarget.Add(imgo, gameObject);
				onWayToTargetDict.Add(imgo, group);
			}
		}

		/// <summary>
		/// Recieves information when object reached the destination. Checks if object is attacking or occupying and 
		/// creates the appropriate class.
		/// </summary>
		/// <param name="imgo">The object in destiantion.</param>
		public void MovementFinished(IMovableGameObject imgo) {
			var onWayCopy = new Dictionary<IMovableGameObject, GroupMovables>(onWayToTargetDict);
			var group = onWayCopy[imgo];
			var gameObject = attackersTarget[imgo];
			var moveMgr = Game.IMoveManager;
			// Remove all object which goint to the same target from a watch lists.
			foreach (IMovableGameObject item in onWayCopy[imgo]) {
				onWayToTargetDict.Remove(item);
				attackersTarget.Remove(item);
				moveMgr.UnlogFromFinishMoveReciever(item);
			}

			if (offensiveActionDict[onWayCopy[imgo]] == ActionAnswer.Attack) {
				// Creates a Fight
				fightList.Add(new Fight(group, gameObject));
			} else {
				// Creates an Occupation
				occupationList.Add(new Occupation(group, gameObject));
			}
			offensiveActionDict.Remove(onWayCopy[imgo]);
		}

		/// <summary>
		/// Unlogs given object from a watch lists.
		/// </summary>
		/// <param name="imgo">The object which interrupted the movement.</param>
		public void MovementInterupted(IMovableGameObject imgo) {
			if (onWayToTargetDict.ContainsKey(imgo)) {
				offensiveActionDict.Remove(onWayToTargetDict[imgo]);
				onWayToTargetDict.Remove(imgo);
				attackersTarget.Remove(imgo);
			}
		}

		/// <summary>
		/// Creates and returns a new list with all current occupations. 
		/// </summary>
		/// <returns>Returns the new list with all current occupations.</returns>
		public List<Tuple<List<IMovableGameObject>, IGameObject, int>> GetOccupations() {
			var list = new List<Tuple<List<IMovableGameObject>, IGameObject, int>>();

			foreach (var item in occupationList) {
				list.Add(new Tuple<List<IMovableGameObject>, IGameObject, int>(item.GetAttackers(), item.GetTarget(), (int)item.GetTime().TotalSeconds));
			}

			foreach (var item in offensiveActionDict) {
				if (item.Value == ActionAnswer.Occupy) {
					var list1 = new List<IMovableGameObject>();
					foreach (IMovableGameObject item1 in item.Key) {
						list1.Add(item1);
					}
					list.Add(new Tuple<List<IMovableGameObject>, IGameObject, int>(list1, attackersTarget[item.Key[0]], attackersTarget[item.Key[0]].OccupyTime));
				}
			}

			return list;
		}

		/// <summary>
		/// Creates and returns a new list with all current fights.
		/// </summary>
		/// <returns>Returns the new list with all current fights.</returns>
		public List<Tuple<List<IGameObject>, List<IGameObject>>> GetFights() {
			var list = new List<Tuple<List<IGameObject>, List<IGameObject>>>();

			foreach (var item in fightList) {
				list.Add(new Tuple<List<IGameObject>, List<IGameObject>>(item.GetAttackers(), item.GetDeffenders()));
			}

			foreach (var item in offensiveActionDict) {
				if (item.Value == ActionAnswer.Attack) {
					var list1 = new List<IGameObject>();
					foreach (IMovableGameObject item1 in item.Key) {
						list1.Add(item1);
					}
					list.Add(new Tuple<List<IGameObject>, List<IGameObject>>(list1, new List<IGameObject>() { attackersTarget[(IMovableGameObject)list1[0]] }));
				}
			}

			return list;
		}
		#endregion

		/// <summary>
		/// Checks if given group is already fighting or occupying given object.
		/// </summary>
		/// <param name="group">The checking group.</param>
		/// <param name="gameObject">The target of the given group</param>
		/// <returns>Return if the group can do its action.</returns>
		private bool CheckActionPossibility(GroupMovables group, IGameObject gameObject) {
			foreach (var item in fightList) {
				if (item.ContainsAttackingGroup(group, gameObject)) {
					// Attack on other target.
					return true;
				}
				if (item.Contains(gameObject)) {
					item.AddGroup(group);
					return false;
				}
			}
			foreach (var item in occupationList) {
				if (item.Contains(gameObject)) {
					item.AddGroup(group);
					return false;

				}
			}
			return true;
		}
	}
}
