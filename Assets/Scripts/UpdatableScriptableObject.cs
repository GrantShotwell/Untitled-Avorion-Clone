using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UpdatableScriptableObject : ScriptableObject {

	private bool needsUpdate = false;
	private readonly IList<Action> updateActions = new List<Action>(1);
	public event Action update {
		add => updateActions.Add(value);
		remove => updateActions.Remove(value);
	}

	protected void TryUpdateRequest() {
		if(needsUpdate) {
			OnUpdateRequest();
			foreach(Action action in updateActions) action.Invoke();
			needsUpdate = false;
		}
	}

	protected virtual void OnUpdateRequest() { }

	public void RequestUpdate() {
		needsUpdate = true;
	}

}
