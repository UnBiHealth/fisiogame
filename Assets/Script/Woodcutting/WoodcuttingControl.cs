using UOS;
using UnityEngine;
using System.Collections.Generic;

public class WoodcuttingControl : MonoBehaviour {

    enum LJState { WALK, IDLE, GRAB, CHARGE, ATTACK_WAIT, ATTACK, SCROLL, LEAVE }

    LJState state = LJState.WALK;
    bool facingRight = true;
    float speed = 3f;
    float treeDistance = -1.4f;
    float startPosition;
    float timer = 0f;
    float punch = 0.0f; // -1.0f indicates punch can be overwritten
    bool leave = false;

    Animator animator;

    public TargetTree targetTree;
    public BackgroundScroller background;
    public TreeImpact impact;
    public TreeSuperImpact superImpact;

	void Start () {
        // Get the animator and start moving
        animator = GetComponent<Animator>();
        animator.SetBool("walking", true);
        startPosition = transform.position.x;

        // Register the punch event 
        GameControl.instance.RegisterListener(OnPunch, "punch");
        GameControl.instance.RegisterListener(OnExerciseEnd, "exerciseEnd");
	}

    void OnDestroy () {
        GameControl.instance.UnregisterListener(OnPunch, "punch");
        GameControl.instance.UnregisterListener(OnExerciseEnd, "exerciseEnd");
    }
	
	void Update () {

        //Debug.Log("" + Time.frameCount + state);

        // When leaving the stage, move the character left until he's out of the screen.
        if (state == LJState.LEAVE) {
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
                Application.LoadLevel("Initial");
            }
        }

        // While walking, move the character towards the tree. When we reach it, adjust its position and become IDLE.
        else if (state == LJState.WALK) {
            //Debug.Log(transform.position.x + " " + (targetTree.transform.position.x + treeDistance) + " "  + (transform.position.x < targetTree.transform.position.x + treeDistance));
            if (transform.position.x < targetTree.transform.position.x + treeDistance) {
                transform.Translate(speed * Time.deltaTime, 0, 0);
            }
            else {
                Vector3 position = transform.position;
                position.x = targetTree.transform.position.x + treeDistance;
                transform.position = position;
                state = LJState.IDLE;
                animator.SetBool("walking", false);
                timer = 1.5f;
            }
        }
        // Wait for [timer] seconds before grabbing the axe
        else if (state == LJState.IDLE) {
            timer -= Time.deltaTime;
            if (timer < 0.0f) {
                state = LJState.GRAB;
                animator.SetBool("grabbing", true);
            }
        }
        // Wait for the animation to finish (GrabbedAxe will be called)
        else if (state == LJState.GRAB) {
        }
        // Wait for the animation to finish (ChargeFinished will be called, enabling the punch value to be listened to)
        else if (state == LJState.CHARGE) {
        }
        // Wait for the punch value to change. When it does, an attack is triggered
        else if (state == LJState.ATTACK_WAIT) {
            if (punch != -1.0f) {
                // If damage is three, trigger strong attack animation
                if (punch > 0.66f)
                    animator.SetTrigger("strongAttack");
                else
                    animator.SetTrigger("attack");
                state = LJState.ATTACK;
            }
        }
        // Wait for the animation to finish - AttackHit() and AttackFinished() will be called at appropriate times, triggering chains of events with other objects
        else if (state == LJState.ATTACK) {
        }
        // The background and the tree scroll towards the character as it pretends to walk. Once we reach the tree, become IDLE and repeat the cycle
        else if (state == LJState.SCROLL) {
            //Debug.Log(transform.position.x + " " + (targetTree.transform.position.x + treeDistance) + " " + (transform.position.x < targetTree.transform.position.x + treeDistance));
            if (!(transform.position.x < targetTree.transform.position.x + treeDistance)) {
                state = LJState.IDLE;
                animator.SetBool("walking", false);
                timer = 1.5f;
                targetTree.StopScrolling();
                background.StopScrolling();
            }
        }
        
	}
   
    // Callback for when punch changes
    public void OnPunch(object newValue) {
        if (punch == -1.0f)
            punch = float.Parse(newValue.ToString()); 
    }

    // Callback for exercise end
    public void OnExerciseEnd(object newValue) {
        Debug.Log("We're done here!");
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
    public void GrabbedAxe() {
        Debug.Log("Grab your axe!");
        state = LJState.CHARGE;
    }
    // Called once charge animation finishes - starts listening to punch and waiting
    public void ChargeFinished() {
        Debug.Log("Aaaaaand...");
        state = LJState.ATTACK_WAIT;
        punch = -1.0f;
    }

    // Called when the axe touches the tree
    public void AttackHit() {
        Debug.Log("Hit!");
        int damage = punch < 0.33f ? 1 : punch < 0.66f ? 2 : 3;
        // If damage is 2 or less, use the normal impact animations and damage tree immediately
        if (damage <= 2) {
            targetTree.Damage(damage);
            impact.Animate(damage);
        }
        // If damage is 3, trigger the super attack animation: tree will be damaged later
        else {
            superImpact.SuperAttack();
        }
    }

    // Called once attack animation finishes. Triggers charge animation.
    public void AttackFinished() {
        Debug.Log("Whew!");
        // If we're set to leave, don't go to the next tree
        if (leave) {
            timer = 1.0f; // Time character will take to turn back and leave - DO NOT SET AS ZERO.
            state = LJState.LEAVE;
        }
        // If the state has yet to be changed, it means we can safely start charging the next attack;.
        if (state == LJState.ATTACK) {
            state = LJState.CHARGE;
            animator.SetTrigger("charge");
        }
    }

    // Called once tree is teleported away. Begins scrolling the scenery towards character
    public void MoveToNextTree() {
        // If we're set to leave, don't go to the next tree
        if (leave) {
            timer = 0.2f; // Time character will take to turn back and leave - DO NOT SET AS ZERO.
            state = LJState.LEAVE;
        }
        else { 
            Debug.Log("Get moving!");
            state = LJState.SCROLL;
            animator.SetBool("walking", true);
            targetTree.Scroll();
            background.Scroll();
        }
    }
}
