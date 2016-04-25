using UnityEngine;
using System.Collections;

public class TargetTree : MonoBehaviour {

    int hp = 0;
    bool scrolling = false;
    float scrollSpeed = 1.0f;
    int parallaxLayer;

    Animator animator;

    public WoodcuttingControl character;
    public TreeSuperImpact superImpact;

	void Start () {
        hp = 3;
        animator = GetComponent<Animator>();
        animator.SetInteger("hp", hp);
        parallaxLayer = GetComponent<SpriteRenderer>().sortingOrder;
	}
	
	void Update () {
        // The tree scrolls together with the background when told to 
        if (scrolling) {
            transform.Translate(-scrollSpeed * parallaxLayer * Time.deltaTime, 0, 0, Space.Self);
        }
	}

    public void Damage(int damage) {
        hp -= damage;
        animator.SetInteger("hp", hp);
        // If the tree won't fall, let the character charge another attack.
        if (hp > 0) { 
            character.AttackFinished();
        }
        // If not, trigger the animation for tree death (character will release the axe after animation finishes)
        else {
            superImpact.TreeDeath();
        }
    }

    public void TreeFall() {
        // Move tree away and restore its HP (makes it seem like it's a new tree)
        float distance = Random.Range(10.0f, 15.0f);
        transform.Translate(distance, 0, 0);
        hp = 3;
        animator.SetInteger("hp", hp);
    }

    public void Scroll() {
        scrolling = true;
    }

    public void StopScrolling() {
        scrolling = false;
    }
}
