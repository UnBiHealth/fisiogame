using UOS;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MinigameControllerA : MonoBehaviour {

	enum CharacterState { WALK, IDLE, GRAB, CHARGE, ATTACK_WAIT, ATTACK, SCROLL, LEAVE, STOP }

	CharacterState state = CharacterState.WALK;
	bool facingRight = true;
	float speed = 3f;
	float startPosition;
	float timer = 0f;
	float punch = 0.0f; // -1.0f indicates punch can be overwritten
	bool leave = false;

	Animator animator;

	[SerializeField]
	string minigameResource;
	[SerializeField]
	string characterName;
	[SerializeField]
	float targetDistance = -1.4f;
	[SerializeField]
	Target target;
	[SerializeField]
	BackgroundScroller background;
	[SerializeField]
	Impact impact;
	[SerializeField]
	SuperImpact superImpact;
	[SerializeField]
	Button exitButton;
	[SerializeField]
	RewardCounter rewardCounter;

	void Start () {
		// Get the animator and start moving
		animator = GetComponent<Animator>();
		animator.SetBool("walking", true);
		startPosition = transform.position.x;

		exitButton.enabled = false;
		exitButton.gameObject.SetActive(false);

		rewardCounter.SetUp(minigameResource, characterName);

		// Register the punch event 
		GameControl.instance.RegisterListener(OnPunch, "punch");
	}

	void OnDestroy () {
		GameControl.instance.UnregisterListener(OnPunch, "punch");
	}

	void Update () {

		//Debug.Log("" + Time.frameCount + state);

		// When leaving the stage, move the character left until he's out of the screen.
		if (state == CharacterState.LEAVE) {
			if (timer > 0) {
				timer -= Time.deltaTime;
				if (timer < 0) {
					animator.SetTrigger("exitStage");
					Flip();
				}
			}
			else if (transform.position.x > startPosition) {
				transform.Translate(-speed * Time.deltaTime, 0, 0);
			}
			else {
				rewardCounter.Finish();
				state = CharacterState.STOP;
			}
		}

		// While walking, move the character towards the tree. When we reach it, adjust its position and become IDLE.
		else if (state == CharacterState.WALK) {
			//Debug.Log(transform.position.x + " " + (targetTree.transform.position.x + treeDistance) + " "  + (transform.position.x < targetTree.transform.position.x + treeDistance));
			if (transform.position.x < target.transform.position.x + targetDistance) {
				transform.Translate(speed * Time.deltaTime, 0, 0);
			}
			else {
				Vector3 position = transform.position;
				position.x = target.transform.position.x + targetDistance;
				transform.position = position;
				state = CharacterState.IDLE;
				animator.SetBool("walking", false);
				timer = 0.5f;
			}
		}
		// Wait for [timer] seconds before grabbing the axe
		else if (state == CharacterState.IDLE) {
			timer -= Time.deltaTime;
			if (timer < 0.0f) {
				state = CharacterState.GRAB;
				animator.SetTrigger("grab");
			}
		}
		// Wait for the animation to finish (GrabbedAxe will be called)
		else if (state == CharacterState.GRAB) {
		}
		// Wait for the animation to finish (ChargeFinished will be called, enabling the punch value to be listened to)
		else if (state == CharacterState.CHARGE) {
		}
		// Wait for the punch value to change. When it does, an attack is triggered
		else if (state == CharacterState.ATTACK_WAIT) {
			if (punch > -1.0f) {
				// If damage is three, trigger strong attack animation
				if (punch > 0.66f) {
					animator.SetTrigger("strongAttack");
					rewardCounter.AddHighHit();
				}
				else if (punch > 0.33f) {
					animator.SetTrigger("attack");
					rewardCounter.AddMidHit();
				}
				else {
					animator.SetTrigger("attack");
					rewardCounter.AddLowHit();
				}
				state = CharacterState.ATTACK;
				exitButton.enabled = false;
				exitButton.gameObject.SetActive(false);
			}
		}
		// Wait for the animation to finish - AttackHit() and AttackFinished() will be called at appropriate times, triggering chains of events with other objects
		else if (state == CharacterState.ATTACK) {
		}
		// The background and the tree scroll towards the character as it pretends to walk. Once we reach the tree, become IDLE and repeat the cycle
		else if (state == CharacterState.SCROLL) {
			//Debug.Log(transform.position.x + " " + (targetTree.transform.position.x + treeDistance) + " " + (transform.position.x < targetTree.transform.position.x + treeDistance));
			if (!(transform.position.x < target.transform.position.x + targetDistance)) {
				state = CharacterState.IDLE;
				animator.SetBool("walking", false);
				timer = 1.5f;
				target.StopScrolling();
				background.StopScrolling();
			}
		}
	}

	// Callback for when punch changes
	public void OnPunch(object newValue) {
		if (punch < 0)
			punch = float.Parse(newValue.ToString()); 
	}

	// Callback for exercise end
	public void OnExerciseEnd(object newValue) {
		timer = 0.2f; // Time character will take to turn back and leave - DO NOT SET AS ZERO.
		state = CharacterState.LEAVE;
		leave = true;
	}

	// Flips the character around
	void Flip() {
		facingRight = !facingRight;
		Vector3 scale = transform.localScale;
		scale.x *= -1;
		transform.localScale = scale;
	}

	// Called once axe grab animation finishes - only used to keep the state machine in check
	public void Grabbed() {
		state = CharacterState.CHARGE;
	}
	// Called once charge animation finishes - starts listening to punch and waiting
	public void ChargeFinished() {
		exitButton.enabled = true;
		exitButton.gameObject.SetActive(true);
		state = CharacterState.ATTACK_WAIT;
		punch = -1.0f;
	}

	// Called when the axe touches the tree
	public void AttackHit() {
		Debug.Log("Attack hit " + punch);
		int damage = punch < 0.33f ? 1 : punch < 0.66f ? 2 : 3;
		// If damage is 2 or less, use the normal impact animations and damage tree immediately
		if (damage <= 2) {
			target.Damage(damage);
			impact.Animate(damage);
		}
		// If damage is 3, trigger the super attack animation: tree will be damaged later
		else {
			superImpact.SuperAttack();
		}
	}

	// Called once attack animation finishes. Triggers charge animation.
	public void AttackFinished() {
		// If we're set to leave, don't go to the next tree
		if (leave) {
			timer = 1.0f; // Time character will take to turn back and leave - DO NOT SET AS ZERO.
			state = CharacterState.LEAVE;
		}
		// Kludge way to find if the tree has died and moved
		if (target.hp != 3) {
			state = CharacterState.CHARGE;
			animator.SetTrigger("charge");
		}
	}

	// Called once tree is teleported away. Begins scrolling the scenery towards character
	public void MoveToNextTarget() {
		rewardCounter.AddRandomBonus();
		// If we're set to leave, don't go to the next tree
		if (leave) {
			timer = 0.2f; // Time character will take to turn back and leave - DO NOT SET AS ZERO.
			state = CharacterState.LEAVE;
		}
		else { 
			state = CharacterState.SCROLL;
			animator.SetBool("walking", true);
			target.Scroll();
			background.Scroll();
		}
	}
}
