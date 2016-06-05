using UnityEngine;
using System.Collections;

public class Target : MonoBehaviour {

	public int hp { get; private set; }
	bool scrolling = false;
	float scrollSpeed = 1.0f;
	int parallaxLayer;

	Animator animator;

	public MinigameControllerA character;
	public SuperImpact superImpact;

	void Start () {
		hp = 3;
		animator = GetComponent<Animator>();
		animator.SetInteger("hp", hp);
		parallaxLayer = GetComponent<SpriteRenderer>().sortingOrder;
	}

	void Update () {
		// The target scrolls together with the background when told to 
		if (scrolling) {
			transform.Translate(-scrollSpeed * parallaxLayer * Time.deltaTime, 0, 0, Space.Self);
		}
	}

	public void Damage(int damage) {
		hp -= damage;
		animator.SetInteger("hp", hp);
		// If not, trigger the animation for death (character will release their tool after animation finishes)
		if (hp <= 0) {
			// SuperImpact class simply handles the death animation, do not confuse it for the super attack animation
			// Yes, I know it's stupid, but it seemed like a good idea at the time.
			superImpact.KillTarget();
		}
	}

	public void MoveToNextPosition() {
		// Move tree away and restore its HP (makes it seem like it's a new tree)
		float distance = Random.Range(10.0f, 15.0f);
		transform.Translate(distance, 0, 0);
		hp = 3;
		animator.SetInteger("hp", 3);
	}

	public void Scroll() {
		scrolling = true;
	}

	public void StopScrolling() {
		scrolling = false;
	}
}
