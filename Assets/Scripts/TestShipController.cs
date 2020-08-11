using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestShipController : MonoBehaviour {

	[Range(0, 10)]
	public float moveForce = 1;
	[Range(0, 10)]
	public float lookForce = 1;
	[Range(0, 20)]
	public float gravity = 10;

	Vector3 move = Vector3.zero;
	Vector3 look = Vector3.zero;

	new Rigidbody rigidbody;

	// Start is called before the first frame update
	private void Start() {
		rigidbody = GetComponent<Rigidbody>();
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

	private void OnDestroy() {
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}

	private void Update() {
		ApplyInputForce();
		//ApplyGravityForce();
	}

	void ApplyGravityForce() {
		Vector3 g = -transform.position.normalized * gravity;
		rigidbody.AddForce(g, ForceMode.Acceleration);
	}

	void ApplyInputForce() {
		// Movement
		Vector3 f = transform.TransformDirection(move * moveForce);
		rigidbody.AddForce(f, ForceMode.Force);
		// Rotation
		Vector3 r = transform.TransformDirection(look * lookForce);
		rigidbody.AddTorque(r, ForceMode.Force);
	}

	public void OnMoveX(InputValue value) => move.x = +value.Get<float>();
	public void OnMoveY(InputValue value) => move.y = +value.Get<float>();
	public void OnMoveZ(InputValue value) => move.z = +value.Get<float>();

	public void OnLookX(InputValue value) => look.x = -value.Get<float>();
	public void OnLookY(InputValue value) => look.y = +value.Get<float>();
	public void OnLookZ(InputValue value) => look.z = -value.Get<float>();

}
