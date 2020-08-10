using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class UpdatableMonoBehaviour : MonoBehaviour {

	protected int maxUpdateActions = 100;
	protected Action requestUpdate => _requestUpdate ?? (_requestUpdate = RequestUpdate);
	private Action _requestUpdate = null;
	private bool needsUpdate = false;
	private readonly ICollection<Action> updateActions = new List<Action>();
	public event Action update {
		add => updateActions.Add(value);
		remove => updateActions.Remove(value);
	}

	protected void TryUpdateRequest() {
		if(needsUpdate) {
			OnUpdateRequest();
			if(updateActions.Count >= maxUpdateActions) {
				Debug.LogWarning($"{nameof(UpdatableScriptableObject)} reached it's maximum number of update actions.");
				updateActions.Clear();
				return;
			}
			foreach(Action action in updateActions.ToArray()) action.Invoke();
			needsUpdate = false;
		}
	}

	protected virtual void OnUpdateRequest() { }

	public void RequestUpdate() {
		needsUpdate = true;
	}

}
