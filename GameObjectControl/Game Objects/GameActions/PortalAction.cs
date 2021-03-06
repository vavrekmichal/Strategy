﻿
namespace Strategy.GameObjectControl.Game_Objects.GameActions {
	/// <summary>
	/// Begins travel (show destiantions). Can teleport owner to other SolarSystem as the Gate.
	/// </summary>
	class PortalAction : IGameAction{

		private IGameObject gameObject;

		/// <summary>
		/// Just saves rafarence to owner.
		/// </summary>
		/// <param name="gameObject">The IGameAction owner.</param>
		/// <param name="args">The arguments should be empty.</param>
		public PortalAction(IGameObject gameObject, object[] args) {
			this.gameObject = gameObject;
		}

		/// <summary>
		/// Does nothing on Update.
		/// </summary>
		/// <param name="delay">The delay between last to frames.</param>
		public void Update(float delay) {}

		/// <summary>
		/// Calls travel function and it creates a choice of destination
		/// and may launch travel.
		/// </summary>
		/// <returns>Returns information that the port is ready.</returns>
		public string OnMouseClick() {
			Game.CreateInterstellarTravel(gameObject);
			return "Portal is open.";
		}

		/// <summary>
		/// Returns path to an icon picture.
		/// </summary>
		/// <returns>Returns path to an icon picture.</returns>
		public string IconPath() {
			return "../../media/icons/tunnel.png";
		}
	}
}
