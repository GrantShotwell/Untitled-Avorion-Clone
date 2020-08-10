using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class TestCharacterController : MonoBehaviour {

	float gravity = 9.82f;

	CharacterController character;


	private void Start() {
		character = GetComponent<CharacterController>();
	}

	private void Update() {
		ApplyGravity();
		OrientateToGravity();
	}


	void ApplyGravity() {
		if(!character.isGrounded) {
			Vector3 force = -transform.position.normalized * gravity;
			character.Move(force);
		}
	}

	void OrientateToGravity() {
		Vector3 targetUp = transform.position.normalized;
		transform.up = targetUp;
	}

}
