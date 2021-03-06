﻿using Strategy.GameObjectControl.Game_Objects;
using Strategy.GameObjectControl.Game_Objects.Bullet;

namespace Strategy.FightMgr {
	/// <summary>
	/// Transmits information when the bullet hits the target.
	/// </summary>
	public interface IBulletStopReciever {
		/// <summary>
		/// Calls when the bullet hit the object.
		/// </summary>
		/// <param name="bullet">The bullet which hitted the object.</param>
		/// <param name="hittedObject">The hitted object.</param>
		void BulletHit(IBullet bullet, IGameObject hittedObject);
	}
}
