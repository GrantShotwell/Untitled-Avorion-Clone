using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UpdatableMonoBehaviour : MonoBehaviour {

	private bool needsUpdate = false;
	private readonly IList<Action> updateActions = new List<Action>(1);
	public event Action update {
		add => updateActions.Add(value);
		remove => updateActions.Remove(value);
	}

	protected void TryUpdateRequest() {
		if(needsUpdate) {
			RequestedUpdate();
			foreach(Action action in updateActions) action.Invoke();
			needsUpdate = false;
		}
	}

	protected abstract void RequestedUpdate();

	public void RequestUpdate() {
		needsUpdate = true;
	}

}
