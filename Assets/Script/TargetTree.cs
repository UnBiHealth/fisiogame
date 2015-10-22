using UnityEngine;
using System.Collections;

public class TargetTree : MonoBehaviour {

    int hp = 0;

    Animator animator;

    public WoodcuttingControl character;
    public TreeImpact impact;

	// Use this for initialization
	void Start () {
        hp = 3;
        animator = GetComponent<Animator>();
        animator.SetInteger("hp", hp);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Damage(int damage) {
        Debug.Log("Take " + damage);
        hp -= damage;
        animator.SetInteger("hp", hp);
        impact.Animate(damage);
    }

    public void TreeFall() {
        // Move tree away and restore its HP
        Vector3 position = transform.position;
        position.x += Random.Range(10.0f, 15.0f);
        transform.position = position;
        hp = 3;
        animator.SetInteger("hp", hp);

        character.MoveToNextTree();
    }
}
