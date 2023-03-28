using System;
// using System.Runtime;
using MelonLoader;
using UnityEngine;
using HomingProjectiles;
using CustomList; // Can't get lists to work so this will do for now
// using Il2CppSystem.Collections.Generic;
using AoTNetworking.Game.Projectiles;
using AoTNetworking.Players;
[assembly: MelonInfo(typeof(HomingProjectilesMod), "HomingProjectiles", "0.1", "Lvl3Mage")]
[assembly: MelonGame("RoarkInc", "raot")]
namespace HomingProjectiles
{
	public class HomingProjectilesMod : MelonMod
	{
		//This is the optimal solution but dicts ain't working
		// Dictionary<RangedState,Func<MirrorProjectile, HomingProjectilesMod, Projectile>> projectileCreators = new Dictionary<RangedState,Func<MirrorProjectile, HomingProjectilesMod, Projectile>>
		// {
		// 	{RangedState.Ranged, (_projectile, _mod) => new MovingProjectile(_projectile, _mod)},
		// 	{RangedState.Homing, (_projectile, _mod) => new HomingProjectile(_projectile, _mod)},
		// 	{RangedState.RangedHoming, (_projectile, _mod) => new RangedHomingProjectile(_projectile, _mod)},
		// 	{RangedState.Hitscan, (_projectile, _mod) => new HitscanProjectile(_projectile, _mod)},
		// 	{RangedState.Wired, (_projectile, _mod) => new WiredProjectile(_projectile, _mod)},
		// 	{RangedState.LaserGuided, (_projectile, _mod) => new LaserGuidedProjectile(_projectile, _mod)}
		// };
		public enum RangedState{
			None,
			Ranged,
			Homing,
			RangedHoming,
			Hitscan,
			Wired,
			LaserGuided
		}
		RangedState rangedState = RangedState.None;
		List<Projectile> projectiles = new List<Projectile>{};
		public override void OnUpdate()
		{
			UpdateState();
			UpdateProjectiles();
			
		}
		void UpdateState(){

			//Writing this hurts my brain but I don't have the time to get dicts working
			if(Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt)){
				if(Input.GetKey(KeyCode.Alpha0)){
					rangedState = RangedState.None;
				}
				else if(Input.GetKey(KeyCode.Alpha1)){
					rangedState = RangedState.Ranged;
				}
				else if(Input.GetKey(KeyCode.Alpha2)){
					rangedState = RangedState.Homing;
				}
				else if(Input.GetKey(KeyCode.Alpha3)){
					rangedState = RangedState.RangedHoming;
				}
				else if(Input.GetKey(KeyCode.Alpha4)){
					rangedState = RangedState.Hitscan;
				}
				else if(Input.GetKey(KeyCode.Alpha5)){
					rangedState = RangedState.Wired;
				}
				else if(Input.GetKey(KeyCode.Alpha6)){
					rangedState = RangedState.LaserGuided;
				}
			}
		}
		void UpdateProjectiles(){
			if(rangedState == RangedState.None){
				return;
			}
			//Check and remove null
			for(int i = 0; i < projectiles.Count; i++){
				if(projectiles[i].projectile == null){
					projectiles.RemoveAt(i);
				}
			}
			//Update all projectiles
			for(int i = 0; i < projectiles.Count; i++){
				projectiles[i].UpdateProjectile();
			}

			//get all new projectiles
			List<MirrorProjectile> newProjectiles = new List<MirrorProjectile>(UnityEngine.Object.FindObjectsOfType<MirrorProjectile>());

			List<Projectile> projectileComparison = new List<Projectile>(projectiles);
			//Remove duplicates from newProjectiles
			for(int i = 0; i < newProjectiles.Count; i++){
				for(int j = 0; j < projectileComparison.Count; j++){
					if(projectileComparison[j].projectile == newProjectiles[i]){
						projectileComparison.RemoveAt(j);
						newProjectiles.RemoveAt(i);
						i--;
						break;
					}
				}
			}

			//add all new projectiles to projectiles
			for(int i = 0; i < newProjectiles.Count; i++){
				// projectiles.Add(projectileCreators[rangedState](newProjectiles[i],this));
				switch(rangedState) 
				{
					case RangedState.Ranged:
						projectiles.Add(new MovingProjectile(newProjectiles[i], this));
						break;
					case RangedState.Homing:
						projectiles.Add(new HomingProjectile(newProjectiles[i], this));
						break;
					case RangedState.RangedHoming:
						projectiles.Add(new RangedHomingProjectile(newProjectiles[i], this));
						break;
					case RangedState.Hitscan:
						projectiles.Add(new HitscanProjectile(newProjectiles[i], this));
						break;
					case RangedState.Wired:
						projectiles.Add(new WiredProjectile(newProjectiles[i], this));
						break;
					case RangedState.LaserGuided:
						projectiles.Add(new LaserGuidedProjectile(newProjectiles[i], this));
						break;
					default:
						break;
				}
				
			}
			RefreshDataBuffer();
		}

		void RefreshDataBuffer(){
			players = null;
		}

		MirrorNetworkedPlayer[] players = null;
		public MirrorNetworkedPlayer[] GetPlayers(){// returns all curretnly connected players, refreshes every frame
			if(players == null){
				players = UnityEngine.Object.FindObjectsOfType<MirrorNetworkedPlayer>();
			}
			return players;
		}
	}


	public abstract class Projectile // The base class for all projectiles to inherit from
	{
		protected HomingProjectilesMod mod;
		public readonly MirrorProjectile projectile;
		protected Transform transform;
		public Projectile(MirrorProjectile _projectile, HomingProjectilesMod _mod){
			mod = _mod;
			projectile = _projectile;
			transform = projectile.gameObject.transform;
		}
		public abstract void UpdateProjectile();
	}

	public class MovingProjectile : Projectile
	{
		float lifetime = 0;
		const float speed = 200;
		Vector3 startPosition, movementDirection;
		public MovingProjectile(MirrorProjectile _projectile, HomingProjectilesMod _mod) : base(_projectile, _mod)
		{
			startPosition = transform.position;
		}
		public override void UpdateProjectile(){
			if(movementDirection == Vector3.zero){
				movementDirection = transform.forward;
			}
			lifetime += Time.deltaTime;
			Vector3 newPosition = startPosition + movementDirection * (lifetime * speed);
			transform.position = newPosition;
		}
	}

	public class HomingProjectile : Projectile
	{
		Vector3 prevPosition;
		Quaternion prevRotation;
		const float speed = 50;
		const float rotationSpeed = 100f;
		public HomingProjectile(MirrorProjectile _projectile, HomingProjectilesMod _mod) : base(_projectile, _mod)
		{
			prevPosition = transform.position;
			prevRotation = transform.rotation;
		}
		public override void UpdateProjectile(){
			Transform player = FindClosestPlayer(mod.GetPlayers());
			Quaternion lookRotation = Quaternion.LookRotation(player.position - transform.position, Vector3.up);
			transform.rotation = Quaternion.RotateTowards(prevRotation, lookRotation, Time.deltaTime * rotationSpeed);
			//transform.rotation = lookRotation;
			prevRotation = transform.rotation;
			transform.position = prevPosition + transform.forward * 50 * Time.deltaTime;
			prevPosition = transform.position;
		}
		Transform FindClosestPlayer(MirrorNetworkedPlayer[] players){
			//MirrorNetworkedPlayer[] players = 
			Transform closestPlayer = null;
			float closestAngle = 181;
			for(int i = 0; i < players.Length; i++){
				Transform player = players[i].gameObject.transform;
				Quaternion lookRotation = Quaternion.LookRotation(player.position - transform.position, Vector3.up);
				float angle = Quaternion.Angle(lookRotation, transform.rotation);
				if(angle < closestAngle){
					closestAngle = angle;
					closestPlayer = player;
				}
			}
			return closestPlayer;
		}
	}
	public class RangedHomingProjectile : Projectile
	{
		const float limitAngle = 10;
		const float speed = 500;
		Transform lockedTarget = null;
		Vector3 movementDirection, currentPosition;
		public RangedHomingProjectile(MirrorProjectile _projectile, HomingProjectilesMod _mod) : base(_projectile, _mod)
		{
			currentPosition = transform.position;
		}
		MirrorNetworkedPlayer FindClosestPlayer(MirrorNetworkedPlayer[] players){
			//MirrorNetworkedPlayer[] players = 
			MirrorNetworkedPlayer closestPlayer = null;
			float closestAngle = 181;
			for(int i = 0; i < players.Length; i++){
				Transform playerTransform = players[i].gameObject.transform;
				Quaternion lookRotation = Quaternion.LookRotation(playerTransform.position - transform.position, Vector3.up);
				float angle = Quaternion.Angle(lookRotation, transform.rotation);
				if(angle < closestAngle){
					closestAngle = angle;
					closestPlayer = players[i];
				}
			}
			return closestPlayer;
		}
		public override void UpdateProjectile(){
			if(movementDirection == Vector3.zero){
				movementDirection = transform.forward;
			}
			
			if(lockedTarget != null){
				LockAtTarget();
				return;
			}
			MirrorNetworkedPlayer networkedPlayer = FindClosestPlayer(mod.GetPlayers());
			if(networkedPlayer == null){
				return;
			}
			Transform player = networkedPlayer.gameObject.transform;
			Quaternion lookRotation = Quaternion.LookRotation(player.position - transform.position, Vector3.up);
			float angle = Quaternion.Angle(lookRotation, transform.rotation);

			float stepSize = Time.deltaTime * speed;

			Vector3 movementVector;
			if(angle <= limitAngle){
				Vector3 playerVelocity = new Vector3(networkedPlayer._syncVelocity.direction.x,networkedPlayer._syncVelocity.direction.y,networkedPlayer._syncVelocity.direction.z)/5000;
				Vector3 predictedPlayerPosition = player.position + playerVelocity*Time.deltaTime;
				Vector3 targetVector = predictedPlayerPosition - currentPosition;
				if(targetVector.magnitude < stepSize){ // player in range
					lockedTarget = player;
					movementVector = targetVector;
				}
				else{
					movementVector = Vector3.ClampMagnitude(targetVector, stepSize); // move towards player
				}
				//update direction
				movementDirection = movementVector.normalized;
			}
			else{ // move forward
				movementVector = movementDirection * stepSize;
			}
			currentPosition = currentPosition + movementVector;
			transform.position = currentPosition;
		}
		void LockAtTarget(){
			Vector3 playerVelocity = Vector3.zero;
			Rigidbody body = lockedTarget.gameObject.GetComponent<Rigidbody>();
			playerVelocity = body.velocity;
			transform.position = lockedTarget.position + playerVelocity*Time.deltaTime;
		}
	}//Update velocity using _syncedVelocity
	public class HitscanProjectile : Projectile
	{
		const float limitAngle = 10;
		Transform lockedTarget = null;
		public HitscanProjectile(MirrorProjectile _projectile, HomingProjectilesMod _mod) : base(_projectile, _mod)
		{
		}
		MirrorNetworkedPlayer FindClosestPlayer(MirrorNetworkedPlayer[] players){
			//MirrorNetworkedPlayer[] players = 
			MirrorNetworkedPlayer closestPlayer = null;
			float closestAngle = 181;
			for(int i = 0; i < players.Length; i++){
				Transform playerTransform = players[i].gameObject.transform;
				Quaternion lookRotation = Quaternion.LookRotation(playerTransform.position - transform.position, Vector3.up);
				float angle = Quaternion.Angle(lookRotation, transform.rotation);
				if(angle < closestAngle){
					closestAngle = angle;
					closestPlayer = players[i];
				}
			}
			return closestPlayer;
		}
		public override void UpdateProjectile(){
			if(lockedTarget != null){
				LockAtTarget();
				return;
			}
			else{
				MirrorNetworkedPlayer networkedPlayer = FindClosestPlayer(mod.GetPlayers());
				if(networkedPlayer == null){
					return;
				}
				Transform player = networkedPlayer.gameObject.transform;
				Quaternion lookRotation = Quaternion.LookRotation(player.position - transform.position, Vector3.up);
				float angle = Quaternion.Angle(lookRotation, transform.rotation);
				
				Vector3 playerVelocity = new Vector3(networkedPlayer._syncVelocity.direction.x,networkedPlayer._syncVelocity.direction.y,networkedPlayer._syncVelocity.direction.z)/5000;
				Vector3 playerPosition = player.position + playerVelocity*Time.deltaTime;
				if(angle <= limitAngle){
					lockedTarget = player;
					transform.position = playerPosition;
				}	
			}
			
		}
		void LockAtTarget(){
			Vector3 playerVelocity = Vector3.zero;
			Rigidbody body = lockedTarget.gameObject.GetComponent<Rigidbody>();
			playerVelocity = body.velocity;
			transform.position = lockedTarget.position + playerVelocity*Time.deltaTime;
		}
	}//Update velocity using _syncedVelocity
	public class WiredProjectile : Projectile
	{
		float lifetime = 0;
		const float speed = 75;
		const float maxSnapDistance = 6f;
		const float snapGrowSpeed = 1.05f; //(1 - inf)
		const float minSnapDistance = 0.5f;
		Transform originalPlayer;
		MirrorNetworkedPlayer originalNetworkedPlayer;
		Vector3 currentPosition;
		bool snapped = false;
		public WiredProjectile(MirrorProjectile _projectile, HomingProjectilesMod _mod) : base(_projectile, _mod)
		{
			currentPosition = transform.position;
			originalNetworkedPlayer = FindClosestPlayer(mod.GetPlayers());
			originalPlayer = originalNetworkedPlayer.gameObject.transform;
		}
		MirrorNetworkedPlayer FindClosestPlayer(MirrorNetworkedPlayer[] players, MirrorNetworkedPlayer[] exclude = null){
			if(exclude == null){
				exclude = new MirrorNetworkedPlayer[0];
			}
			MirrorNetworkedPlayer closestPlayer = null;
			float closestDistanceSquared = Single.PositiveInfinity;
			for(int i = 0; i < players.Length; i++){
				if(!Array.Exists(exclude, element => element == players[i])){
					Transform player = players[i].gameObject.transform;
					Vector3 delta = player.position - transform.position;
					float distanceSquared = delta.x*delta.x + delta.y*delta.y + delta.z*delta.z;
					if(distanceSquared < closestDistanceSquared){
						closestDistanceSquared = distanceSquared;
						closestPlayer = players[i];
					}
				}
				
			}
			return closestPlayer;
		}
		public override void UpdateProjectile(){
			if(!snapped){
				Move();
			}
			SnapToPlayer();
			UpdatePosition();
		}
		void Move(){
			if(snapped){
				return;
			}
			if(originalPlayer == null){
				currentPosition += transform.forward*speed*Time.deltaTime;
				return;
			}
			lifetime += Time.deltaTime;
			Vector3 forwardVector = new Vector3(originalNetworkedPlayer._syncView.x,originalNetworkedPlayer._syncView.y,originalNetworkedPlayer._syncView.z);
			Vector3 currentTarget = originalPlayer.position + forwardVector*lifetime*speed;
			Vector3 movementDirection = currentTarget - currentPosition;
			Vector3 velocity = movementDirection.normalized*speed;
			transform.rotation = Quaternion.LookRotation(velocity);
			currentPosition += velocity*Time.deltaTime;
			// currentPosition = currentTarget;
		}
		void SnapToPlayer(){

			MirrorNetworkedPlayer closestPlayerObj = FindClosestPlayer(mod.GetPlayers(), new MirrorNetworkedPlayer[]{originalNetworkedPlayer});
			if(closestPlayerObj == null){
				return;
			}
			Transform closestPlayer = closestPlayerObj.gameObject.transform;
			Vector3 velocity = new Vector3(closestPlayerObj._syncVelocity.direction.x,closestPlayerObj._syncVelocity.direction.y,closestPlayerObj._syncVelocity.direction.z)/5000;
			// velocity = velocity.normalized * closestPlayerObj._syncVelocity.scaledMagnitude;
			Vector3 otherPosition = velocity*Time.deltaTime + closestPlayer.position;
			Vector3 otherPlayerDelta = otherPosition - currentPosition;

			Vector3 currentPlayerDelta = originalPlayer.position - currentPosition;
			
			float snapDistance = (maxSnapDistance - minSnapDistance) * (-(float)Mathf.Pow(snapGrowSpeed, - currentPlayerDelta.magnitude) + 1) + minSnapDistance;
			if(otherPlayerDelta.magnitude <= snapDistance){
				currentPosition = otherPosition;
				snapped = true;
			}
		}
		void UpdatePosition(){
			transform.position = currentPosition;
		}
	}
	public class LaserGuidedProjectile : Projectile
	{
		const float speed = 75;
		const float maxSnapDistance = 6f;
		const float snapGrowSpeed = 1.05f; //(1 - inf)
		const float minSnapDistance = 0.5f;
		Transform originalPlayer;
		MirrorNetworkedPlayer originalNetworkedPlayer;
		Vector3 currentPosition;
		bool snapped = false;
		public LaserGuidedProjectile(MirrorProjectile _projectile, HomingProjectilesMod _mod) : base(_projectile, _mod)
		{
			currentPosition = transform.position;
			originalNetworkedPlayer = FindClosestPlayer(mod.GetPlayers());
			originalPlayer = originalNetworkedPlayer.gameObject.transform;
		}
		MirrorNetworkedPlayer FindClosestPlayer(MirrorNetworkedPlayer[] players, MirrorNetworkedPlayer[] exclude = null){
			if(exclude == null){
				exclude = new MirrorNetworkedPlayer[0];
			}
			MirrorNetworkedPlayer closestPlayer = null;
			float closestDistanceSquared = Single.PositiveInfinity;
			for(int i = 0; i < players.Length; i++){
				if(!Array.Exists(exclude, element => element == players[i])){
					Transform player = players[i].gameObject.transform;
					Vector3 delta = player.position - transform.position;
					float distanceSquared = delta.x*delta.x + delta.y*delta.y + delta.z*delta.z;
					if(distanceSquared < closestDistanceSquared){
						closestDistanceSquared = distanceSquared;
						closestPlayer = players[i];
					}
				}
				
			}
			return closestPlayer;
		}
		public override void UpdateProjectile(){
			if(!snapped){
				Move();
			}
			
			SnapToPlayer();
			UpdatePosition();
		}
		void Move(){
			if(originalPlayer == null){
				currentPosition += transform.forward*speed*Time.deltaTime;
				return;
			}
			Vector3 forwardVector = new Vector3(originalNetworkedPlayer._syncView.x,originalNetworkedPlayer._syncView.y,originalNetworkedPlayer._syncView.z);
			RaycastHit hit;
			if (Physics.Raycast(originalPlayer.position, forwardVector, out hit))
			{
				Vector3 currentTarget = hit.point;
				Vector3 movementDirection = currentTarget - currentPosition;
				Vector3 velocity = movementDirection.normalized*speed;
				transform.rotation = Quaternion.LookRotation(velocity);
				currentPosition += velocity*Time.deltaTime;	
			}
			else{
				currentPosition += transform.forward*speed*Time.deltaTime;
			}
		}
		void SnapToPlayer(){

			MirrorNetworkedPlayer closestPlayerObj = FindClosestPlayer(mod.GetPlayers()/*, new MirrorNetworkedPlayer[]{originalNetworkedPlayer}*/);
			if(closestPlayerObj == null){
				return;
			}
			Transform closestPlayer = closestPlayerObj.gameObject.transform;
			Vector3 velocity = new Vector3(closestPlayerObj._syncVelocity.direction.x,closestPlayerObj._syncVelocity.direction.y,closestPlayerObj._syncVelocity.direction.z)/5000;
			// velocity = velocity.normalized * closestPlayerObj._syncVelocity.scaledMagnitude;
			Vector3 otherPosition = velocity*Time.deltaTime + closestPlayer.position;
			Vector3 otherPlayerDelta = otherPosition - currentPosition;

			Vector3 currentPlayerDelta = originalPlayer.position - currentPosition;
			
			float snapDistance = (maxSnapDistance - minSnapDistance) * (-(float)Mathf.Pow(snapGrowSpeed, - currentPlayerDelta.magnitude) + 1) + minSnapDistance;
			if(otherPlayerDelta.magnitude <= snapDistance){
				currentPosition = otherPosition;
				snapped = true;
				// AoTGameEngine.Strikeables.Strikeable strikeable = closestPlayerObj.gameObject.GetComponent<AoTGameEngine.Strikeables.Strikeable>();
				// AoTGameEngine.Strikeables.StrikePoint point = strikeable.GetFirstPointOfType(AttackType.Everything);
				
				// AoTGameEngine.Strikeables.StrikeableHit hit = new AoTGameEngine.Strikeables.StrikeableHit(point);
				// mod.LoggerInstance.Msg(point == null); // try creating a simple struct and see if the same thing happens (like the StrikeableHit)
				// mod.LoggerInstance.Msg(hit.GetPlanarDistance());
				// strikeable.Hit(null,AttackType.Everything,true,projectile);


			}
		}
		void UpdatePosition(){
			transform.position = currentPosition;
		}
	}
}

///Wired projectile -- follows trajectory of player forward vector but adds local offset vector multiplied by distance traveled that increments over time
