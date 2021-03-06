﻿using Miyagi.UI.Controls;

namespace Strategy.GameGUI {
	/// <summary>
	/// Extension of the CloseButton which has reference to TextBox to get text from it. 
	/// TextBox text is used as name of the saving game.
	/// </summary>
	class SaveGameButton : CloseButton {

		TextBox textBox;
		string panelToClose;

		/// <summary>
		/// Creates instance of the CloseButton and stored textBox reference. Also adds MouseClick action SaveGame.
		/// </summary>
		/// <param name="name">The name of the panel to close.</param>
		/// <param name="textBox">The text box with name of the save game.</param>
		public SaveGameButton(string name, TextBox textBox)
			: base(name) {
			this.textBox = textBox;
			panelToClose = name;
			MouseClick += SaveGame;
		}

		/// <summary>
		/// Gets text from textBox and calls Save with the text from textBox. After that closes panel.
		/// </summary>
		/// <param name="sender">The sender of the action.</param>
		/// <param name="e">The arguments of the action.</param>
		private void SaveGame(object sender, Miyagi.Common.Events.MouseButtonEventArgs e) {
			var saveName = textBox.Text;
			if (saveName == "") {
				saveName = "NoName";
			}
			Game.Save(saveName + ".save");
			Game.IGameGUI.ClosePanel(panelToClose);

		}
	}
}
