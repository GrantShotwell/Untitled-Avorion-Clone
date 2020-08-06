using UnityEngine;

public class DensityField : MonoBehaviour {

	public Vector3Int size;
	public ComputeBuffer points;

	private bool needsUpdate = false;
	public delegate void UpdateRequest();
	public event UpdateRequest update = () => { };


	void Start() {
		RequestUpdate();
	}

	void Update() {
		if(needsUpdate) {

			needsUpdate = false;
			update.Invoke();
		}
	}

	void OnValidate() {
		needsUpdate = true;
	}

	void OnDestroy() {
		DisposeBuffers();
	}

	void OnDrawGizmosSelected() {

		if(points == null) return;
		
		// Draw points.
		Vector3 btm = Vector3.zero, top = Vector3.zero;
		Vector4[] points1D = new Vector4[points.count];
		points.GetData(points1D);
		foreach(Vector4 point in points1D) {
			if(point.w > 0) {
				Gizmos.color = Color.Lerp(Color.red, Color.blue, point.w);
				Gizmos.DrawWireSphere(new Vector3(point.x, point.y, point.z) + transform.position, 0.1f);
			}
			for(int i = 0; i < 3; i++) {
				if(point[i] < btm[i]) btm[i] = point[i];
				if(point[i] > btm[i]) top[i] = point[i];
			}
		}

		// Draw bounding box.
		Gizmos.color = Color.white;
		Vector3 TTT = new Vector3(top.x, top.y, top.z) + transform.position;
		Vector3 BTT = new Vector3(btm.x, top.y, top.z) + transform.position;
		Vector3 TBT = new Vector3(top.x, btm.y, top.z) + transform.position;
		Vector3 TTB = new Vector3(top.x, top.y, btm.z) + transform.position;
		Vector3 BBB = new Vector3(btm.x, btm.y, btm.z) + transform.position;
		Vector3 TBB = new Vector3(top.x, btm.y, btm.z) + transform.position;
		Vector3 BTB = new Vector3(btm.x, top.y, btm.z) + transform.position;
		Vector3 BBT = new Vector3(btm.x, btm.y, top.z) + transform.position;

		// draw top
		Gizmos.DrawLine(TTT, TTB);
		Gizmos.DrawLine(TTT, BTT);
		Gizmos.DrawLine(BTB, TTB);
		Gizmos.DrawLine(BTB, BTT);
		// draw bottom
		Gizmos.DrawLine(BBB, BBT);
		Gizmos.DrawLine(BBB, TBB);
		Gizmos.DrawLine(TBT, BBT);
		Gizmos.DrawLine(TBT, TBB);
		// draw left
		Gizmos.DrawLine(BBB, BTB);
		Gizmos.DrawLine(BBB, BBT);
		Gizmos.DrawLine(BTT, BTB);
		Gizmos.DrawLine(BTT, BBT);
		// draw right
		Gizmos.DrawLine(TTT, TBT);
		Gizmos.DrawLine(TTT, TTB);
		Gizmos.DrawLine(TBB, TBT);
		Gizmos.DrawLine(TBB, TTB);

	}


	struct Point {
#pragma warning disable 649
		public Vector4 point;
		public Vector3Int index;
#pragma warning restore 649
	}

	public void RequestUpdate() {
		needsUpdate = true;
	}

	public void CreateBuffers() {
		DisposeBuffers();
		points = new ComputeBuffer(size.x * size.y * size.z, sizeof(float) * 4);
	}

	public void DisposeBuffers() {
		points?.Dispose();
	}

}
