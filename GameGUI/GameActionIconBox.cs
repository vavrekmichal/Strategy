﻿using Miyagi.UI.Controls;
using Strategy.GameObjectControl.Game_Objects.GameActions;

namespace Strategy.GameGUI {
	/// <summary>
	/// Extension of the PictureBox which have reference on the action and calls OnMouseClick when user clicks on it.
	/// </summary>
	class GameActionIconBox : PictureBox {
		private IGameAction action;

		/// <summary>
		/// Creates instance of the GameActionIconBox and store a reference on action. Also adds MouseClick action GameActionClicked.
		/// </summary>
		/// <param name="action"></param>
		public GameActionIconBox(IGameAction action) {
			this.action = action;
			Load(action.IconPath());
			Size = new Miyagi.Common.Data.Size(25, 25);
			MouseClick += GameActionClicked;
		}

		/// <summary>
		/// MouseClick action which calls OnMouseClick() and prints answer to the game console.
		/// </summary>
		/// <param name="sender">The sender of the action.</param>
		/// <param name="e">The arguments of the action.</param>
		private void GameActionClicked(object sender, Miyagi.Common.Events.MouseButtonEventArgs e) {
			Game.PrintToGameConsole(action.OnMouseClick());
		}
	}
}
