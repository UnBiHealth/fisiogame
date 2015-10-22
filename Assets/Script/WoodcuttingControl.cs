using UOS;
using UnityEngine;
using System.Collections.Generic;

public class WoodcuttingControl : MonoBehaviour {

    enum LJState { WALK, IDLE, GRAB, CHARGE, ATTACK_WAIT, ATTACK }

    LJState state = LJState.WALK;
    bool facingRight = true;
    float speed = 3f;
    float treeDistance = -1.35f;
    float timer = 0f;
    float pin = -1.0f;
    bool listeningToPin = false;
    Object pinLock = new Object();

    Animator animator;

    public TargetTree targetTree;

	void Start () {
        // Get the animator and start moving
        animator = GetComponent<Animator>();
        animator.SetBool("walking", true);

        // Register the pin event (TODO: Use GameControl instead)
        PinDriver.instance.PinChanged += OnPinChanged;
	}
	
	void Update () {
        if (state == LJState.WALK) {
            Debug.Log(transform.position.x + " " + (targetTree.transform.position.x + treeDistance) + " "  + (transform.position.x < targetTree.transform.position.x + treeDistance));
            if (transform.position.x < targetTree.transform.position.x + treeDistance) {
                Debug.Log("unity pls");
                transform.Translate(speed * Time.deltaTime, 0, 0);
            }
            else {
                state = LJState.IDLE;
                animator.SetBool("walking", false);
                timer = 1.0f;
            }
        }
        else if (state == LJState.IDLE) {
            timer -= Time.deltaTime;
            if (timer < 0.0f) {
                state = LJState.GRAB;
                animator.SetBool("grabbing", true);
            }
        }
        else if (state == LJState.GRAB) {
            // Wait for the animation to finish
        }
        else if (state == LJState.CHARGE) {
            // Wait
        }
        else if (state == LJState.ATTACK_WAIT) {
            lock (pinLock) {
                if (pin != -1.0f) {
                    animator.SetBool("attacking", true);
                    state = LJState.ATTACK;
                    listeningToPin = false;
                }
            }
        }
        
	}

    void Flip() {
        // This function needs to be this complicated to work, apparently.
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    public void OnPinChanged(UhpPin pin, object newValue) {

        lock (pinLock) {
            if (listeningToPin)
                this.pin = float.Parse(newValue.ToString()); 
        }
    }
    
    // Events
    public void GrabbedAxe() {
        Debug.Log("Grab your axe!");
        state = LJState.CHARGE;
    }

    public void ChargeFinished() {
        Debug.Log("Aaaaaand...");
        state = LJState.ATTACK_WAIT;
        lock (pinLock) {
            pin = -1.0f;
            listeningToPin = true;
        }
    }

    public void AttackHit() {
        Debug.Log("Hit!");
        targetTree.Damage(pin < 0.33f ? 1 : pin < 0.66f ? 2 : 3);
    }

    public void AttackFinished() {
        Debug.Log("Whew!");
        // If the state hasn't been changed by other events...
        if (state == LJState.ATTACK) {
            state = LJState.CHARGE;
            animator.SetBool("attacking", false);
        }
    }

    public void MoveToNextTree() {
        Debug.Log("Get moving!");
        state = LJState.WALK;
        animator.SetBool("walking", true);
    }
}
