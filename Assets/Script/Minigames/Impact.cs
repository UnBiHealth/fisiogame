using UnityEngine;
using System.Collections;

public class Impact : MonoBehaviour {

	Animator animator;

	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator>();
	}

	// Update is called once per frame
	void Update () {

	}

	// Shows either low or medium impact attack
	public void Animate(int attackIntensity) {
		animator.SetInteger("attackIntensity", attackIntensity);
	}
	// Returns to default (empty) animation
	public void AnimationEnd() {
		animator.SetInteger("attackIntensity", 0);
	}
}
