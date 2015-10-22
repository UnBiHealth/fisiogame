﻿using UnityEngine;
using System.Collections;

public class TreeImpact : MonoBehaviour {

    Animator animator;

	// Use this for initialization
	void Start () {
        animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Animate(int attackIntensity) {
        animator.SetInteger("attackIntensity", attackIntensity);
    }
    public void AnimationEnd() {
        animator.SetInteger("attackIntensity", 0);
    }
}