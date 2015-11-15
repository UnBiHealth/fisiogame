using UnityEngine;
using System.Collections;

public class TreeSuperImpact : MonoBehaviour {

    Animator animator;
    public TargetTree targetTree;
    public WoodcuttingControl character;

    void Start() {
        animator = GetComponent<Animator>();
    }

    void Update() {

    }
    // Triggers the super attack animation that covers the screen
    public void SuperAttack() {
        animator.SetTrigger("superAttack");
    }
    // Called when super attack animation ends. Triggers tree damage and subsequent death
    public void SuperAttackEnd() {
        targetTree.Damage(3);
    }
    // Triggers the cloud animation where the tree is destroyed
    // This animation really ought not to be in this class, but it's convenient, due to its similarities with the super attack
    public void TreeDeath() {
        animator.SetTrigger("die");
    }
    // Called during the cloud animation, at the moment the tree is covered by it. Moves tree away
    public void TreeFall() {
        targetTree.TreeFall();
    }
    // Called after the animation finishes. Character releases axe and the screen scrolls.
    public void TreeDeathFinished() {
        character.MoveToNextTree();
    }
}
