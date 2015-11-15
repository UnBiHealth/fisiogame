using UOS;
using UnityEngine;
using System.Collections.Generic;

public class WoodcuttingControl : MonoBehaviour {

    enum LJState { WALK, IDLE, GRAB, CHARGE, ATTACK_WAIT, ATTACK, SCROLL }

    LJState state = LJState.WALK;
    bool facingRight = true;
    float speed = 3f;
    float treeDistance = -1.4f;
    float timer = 0f;
    float pin = -1.0f;
    bool listeningToPin = false;
    Object pinLock = new Object();

    Animator animator;

    public TargetTree targetTree;
    public BackgroundScroller background;
    public TreeImpact impact;
    public TreeSuperImpact superImpact;

	void Start () {
        // Get the animator and start moving
        animator = GetComponent<Animator>();
        animator.SetBool("walking", true);

        // Register the pin event (TODO: Use GameControl instead)
        PinDriver.instance.PinChanged += OnPinChanged;
	}
	
	void Update () {

        //Debug.Log("" + Time.frameCount + state);

        // While walking, move the character towards the tree. When we reach it, adjust its position and become IDLE.
        if (state == LJState.WALK) {
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
                timer = 1.0f;
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
        // Wait for the animation to finish (ChargeFinished will be called, enabling the pin value to be listened to)
        else if (state == LJState.CHARGE) {
        }
        // Wait for the pin value to change. When it does, an attack is triggered
        else if (state == LJState.ATTACK_WAIT) {
            lock (pinLock) {
                if (pin != -1.0f) {
                    animator.SetTrigger("attack");
                    // If damage is three, trigger super attack animation
                    if (pin > 0.66f) 
                        superImpact.SuperAttack();
                    state = LJState.ATTACK;
                    listeningToPin = false;
                }
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
                timer = 1.0f;
                targetTree.StopScrolling();
                background.StopScrolling();
            }
        }
        
	}

    // Flips the character around
    void Flip() {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
   
    // Callback for when pin changes
    public void OnPinChanged(UhpPin pin, object newValue) {
        lock (pinLock) {
            if (listeningToPin)
                this.pin = float.Parse(newValue.ToString()); 
        }
    }
    
    // Called once axe grab animation finishes - only used to keep the state machine in check
    public void GrabbedAxe() {
        Debug.Log("Grab your axe!");
        state = LJState.CHARGE;
    }
    // Called once charge animation finishes - starts listening to pin and waiting
    public void ChargeFinished() {
        Debug.Log("Aaaaaand...");
        state = LJState.ATTACK_WAIT;
        lock (pinLock) {
            pin = -1.0f;
            listeningToPin = true;
        }
    }

    // Called when the axe touches the tree
    public void AttackHit() {
        Debug.Log("Hit!");
        int damage = pin < 0.33f ? 1 : pin < 0.66f ? 2 : 3;
        // If damage is 3, exiting ATTACK_WAIT will trigger the desired animation. Tree will be damaged once it finishes
        if (damage <= 2) {
            targetTree.Damage(damage);
            impact.Animate(damage);
        }
    }

    // Called once attack animation finishes. Triggers charge animation.
    public void AttackFinished() {
        Debug.Log("Whew!");
        animator.SetTrigger("charge");
        // The test is just in case another event changed the state recently.
        if (state == LJState.ATTACK) {
            state = LJState.CHARGE;
        }
    }

    // Called once tree is teleported away. Begins scrolling the scenery towards character
    public void MoveToNextTree() {
        Debug.Log("Get moving!");
        state = LJState.SCROLL;
        animator.SetBool("walking", true);
        targetTree.Scroll();
        background.Scroll();
    }
}
