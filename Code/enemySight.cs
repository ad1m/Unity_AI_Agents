using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq; 
using UnityEngine.UI; 

//This script handles the UI for the AI guard
//There are 2 states used (Patrol and Chase). Investigate rarely gets used. 
namespace UnityStandardAssets.Characters.ThirdPerson {

	public class enemySight : MonoBehaviour {
		public NavMeshAgent agent; 
		public ThirdPersonCharacter character; 
		//public BoxCollider guard_BC;

		public enum State {
			PATROL, 
			CHASE, 
			INVESTIGATE
		}

		public State state; 
		public bool alive; 

		//Variables for patrolling 
		public GameObject[] waypoints; 
		private int waypointInd;
		public float patrolSpeed = 0.5f; //The speed of the guard on random patrol

		//Vairables for chasing
		public float chaseSpeed = 0.9f; //The speed of the guard on the chase
		public GameObject target; 

		//Variables for investigatiog
		private Vector3 investigateSpot; 
		private float timer = 0; 
		public float investigateWeight = 2.2f;  

		//Variables for Sight
		public float heightMultiplier; 
		public float sightDist = 18.5f; //How far out the raycast is that the guard can see to detect to chase a player


		//use this for initialization
		void Start() {
			agent = GetComponent<NavMeshAgent> (); 
			character = GetComponent<ThirdPersonCharacter> (); 


			agent.updatePosition = true; 
			agent.updateRotation = false; 

			//Gets all of the cubes which are tagged waypoint. The guard will randomly patrol between these points
			waypoints = GameObject.FindGameObjectsWithTag("waypoint");
			waypointInd = Random.Range(0,waypoints.Length); //Randomly choose a starting waypoint
			state = enemySight.State.PATROL; 
			alive = true; 
			heightMultiplier = 1.36f;  //Place the raycast at the guard's eyes

			//StartCoroutine ("FSM"); 


		}
		void Update() {
			StartCoroutine ("FSM");
		}

		IEnumerator FSM() {
			while (alive) {
				switch (state) { 
				case State.PATROL: 
					Patrol (); 
					break; 
				case State.CHASE: 
					Chase (); 
					break;
				case State.INVESTIGATE: 
					Investigate (); 
					break;
				}
				yield return null;
			}
		}

		void Patrol() {
			agent.speed = patrolSpeed; 
			if (Vector3.Distance (this.transform.position, waypoints [waypointInd].transform.position) >= 2) {
				agent.SetDestination (waypoints [waypointInd].transform.position); 
				character.Move (agent.desiredVelocity, false, false); 
			} else if (Vector3.Distance (this.transform.position, waypoints [waypointInd].transform.position) <= 2) {
				waypointInd = Random.Range (0, waypoints.Length);
				//waypointInd += 1; 
				//if (waypointInd >= waypoints.Length) {
				//waypointInd = 0; 
				//}
			} else { 
				character.Move (Vector3.zero, false, false); 
			}
		}

		void Chase() {
			//Debug.Log ("in chase");
			agent.speed = chaseSpeed; 
			agent.SetDestination (target.transform.position); 
			character.Move (agent.desiredVelocity, false, false); 
			//if (target.GetComponent<Collider> ().isTouching() {
				//Debug.Log ("target in safe zone");
				//state = enemySight.State.PATROL;
			//}
		}

		void Investigate() {
			timer += Time.deltaTime; 
			agent.SetDestination (this.transform.position); 
			character.Move (Vector3.zero, false, false); 
			transform.LookAt (investigateSpot); 
			if (timer >= investigateWeight) {
				state = enemySight.State.PATROL; 
				timer = 0; 
			}
		}
			
		void OnCollision(Collider coll) {
			if (coll.tag == "Player") {
				state = enemySight.State.INVESTIGATE; 
				investigateSpot = coll.gameObject.transform.position; 
			}
		}


		void  OnTriggerEnter (Collider myTrigger)   //this is for the trigger on the spherical collider 
		{


			//Tells guards to stay away from navmesh areas tagged safe_collider, sends them back into the patrol state
			if(myTrigger.gameObject.tag == "safe_collider")
			{
				state = enemySight.State.PATROL;
			}

			//If we touch the player then the player then they have been robbed/tagged so make guard patrol randomly now
			if (myTrigger.gameObject.tag == "Player") {
				target = null;
				waypointInd = Random.Range(0,waypoints.Length);
				agent.SetDestination (waypoints [waypointInd].transform.position);
				state = enemySight.State.PATROL;
				state = enemySight.State.PATROL;
			}
		}


		void FixedUpdate() {
			RaycastHit hit; 
			//uncheck to visualize and remove the enemy sight raylines.
			//Debug.DrawRay(transform.position+Vector3.up*heightMultiplier,transform.forward*sightDist, Color.green); 
			//Debug.DrawRay(transform.position+Vector3.up*heightMultiplier,(transform.forward + transform.right).normalized*sightDist, Color.green); 
			//Debug.DrawRay(transform.position+Vector3.up*heightMultiplier,(transform.forward - transform.right).normalized*sightDist, Color.green); 
			if (Physics.Raycast (transform.position + Vector3.up * heightMultiplier, transform.forward, out hit, sightDist)) {
				if (hit.collider.gameObject.tag == "Player") {
					state = enemySight.State.CHASE; 
					target = hit.collider.gameObject; 
				} else if (hit.collider.gameObject.tag == "safe_collider") {
					state = enemySight.State.PATROL; 
				} else if (hit.collider.gameObject.tag == "OOB") {
					state = enemySight.State.PATROL;
				} 
			}


			//The below is for multiple guards 
			//Guard too close to each other they should navigate somewhere else unless they are in chase mode
			if (Physics.Raycast (transform.position + Vector3.up * heightMultiplier, transform.forward, out hit, 2f) && state != enemySight.State.CHASE) {
				if (hit.collider.gameObject.tag == "Guard") {
					waypointInd = Random.Range(0,waypoints.Length);
					state = enemySight.State.PATROL; 
				}
			}
			else if (Physics.Raycast (transform.position + Vector3.up * heightMultiplier, (transform.forward + transform.right).normalized, out hit, 2f) && state != enemySight.State.CHASE) {
				if (hit.collider.gameObject.tag == "Guard") {
					waypointInd = Random.Range(0,waypoints.Length);
					state = enemySight.State.PATROL; 
				}
			}
			else if (Physics.Raycast (transform.position + Vector3.up * heightMultiplier, (transform.forward - transform.right).normalized, out hit, 2f) && state != enemySight.State.CHASE) {
				if (hit.collider.gameObject.tag == "Guard") {
					waypointInd = Random.Range(0,waypoints.Length);
					state = enemySight.State.PATROL; 
				}
			}
			// end of guard too close to each other


			if (Physics.Raycast (transform.position + Vector3.up * heightMultiplier, (transform.forward + transform.right).normalized, out hit, sightDist)) {
				if (hit.collider.gameObject.tag == "Player") {
					state = enemySight.State.CHASE; 
					target = hit.collider.gameObject; 
			} else if (hit.collider.gameObject.tag == "safe_collider") {
				state = enemySight.State.PATROL; 
				} else if (hit.collider.gameObject.tag == "OOB") {
					state = enemySight.State.PATROL;
				}
			}
			if (Physics.Raycast (transform.position + Vector3.up * heightMultiplier, (transform.forward - transform.right).normalized, out hit, sightDist)) {
				if (hit.collider.gameObject.tag == "Player") {
					state = enemySight.State.CHASE; 
					target = hit.collider.gameObject; 
			} else if (hit.collider.gameObject.tag == "safe_collider") {
				state = enemySight.State.PATROL; 
				} else if (hit.collider.gameObject.tag == "OOB") {
					state = enemySight.State.PATROL;
				}
			}
			//Patrol if the guard sees the player but the player runs much faster than him
			if (!Physics.Raycast (transform.position + Vector3.up * heightMultiplier, transform.forward, out hit, sightDist+3) && !Physics.Raycast (transform.position + Vector3.up * heightMultiplier, (transform.forward + transform.right).normalized, out hit, sightDist+3) && !Physics.Raycast (transform.position + Vector3.up * heightMultiplier, (transform.forward - transform.right).normalized, out hit, sightDist+3)) {
				//Debug.Log ("No enemy in sight");
				state = enemySight.State.PATROL; 
			}
		}
	}
}
