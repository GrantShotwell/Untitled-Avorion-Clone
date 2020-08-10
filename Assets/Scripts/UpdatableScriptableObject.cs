using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class UpdatableScriptableObject : ScriptableObject {

	protected int maxUpdateActions = 100;
	protected Action requestUpdateNow => _requestUpdateNow ?? (_requestUpdateNow = RequestUpdateNow);
	private Action _requestUpdateNow;
	private readonly ICollection<Action> updateActions = new List<Action>();
	public event Action update {
		add => updateActions.Add(value);
		remove => updateActions.Remove(value);
	}

	public void RequestUpdateNow() {
		OnUpdateRequest();
		if(updateActions.Count >= maxUpdateActions) {
			Debug.LogWarning($"{GetType().Name} \"{name}\" reached it's maximum number of update actions.");
			updateActions.Clear();
			return;
		}
		foreach(Action action in updateActions.ToArray()) action.Invoke();
	}

	protected abstract void OnUpdateRequest();

}
