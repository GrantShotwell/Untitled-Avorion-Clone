
using UnityEngine;

public struct FaceAxes {

	static readonly Vector3[] directions = new Vector3[6] {
		Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back
	};

	public Vector3 up, right, forward;

	public FaceAxes(int i) {
		up = directions[i];
		forward = new Vector3(up.y, up.z, up.x);
		right = Vector3.Cross(up, forward);
	}

}
