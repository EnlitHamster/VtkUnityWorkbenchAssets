﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using System.Linq;

public class ViveControllerToolMove : ViveControllerToolBase {

    protected GameObject _collidingObject;
	protected GameObject _objectInHand;
	protected Transform _objectInHandOldParent;
	protected List<Transform> _colliderStack = new List<Transform>();

	protected enum ColliderSource
	{
		Enter,
		Stay,
		Exit
	};

	public override bool Busy()
	{
		return (_objectInHand);
	}

	// to be overridden by derived classes
	public override void Activate()
	{
		base.Activate();
	}

	protected override void OnTriggerEnterImpl(Collider other)
    {
        if (!other.CompareTag("Movable"))
        {
            return;
        }

        // push into stack
        if (_colliderStack.Count > 0)
        {
            // if  the new collider's child is already in the stack
            if(_colliderStack.Last().IsChildOf(other.transform))
            {
                // insert it before the child
                _colliderStack.Insert(_colliderStack.Count -1, other.transform);
                return;
            }
        }

        _colliderStack.Add(other.transform);

        SetCollidingObject(other.gameObject, ColliderSource.Enter);
    }


	protected override void OnTriggerExitImpl(Collider other)
    {
		//Debug.Log("OnTriggerExitImpl");

        if (!other.CompareTag("Movable"))
        {
            return;
        }

        if (!_collidingObject)
        {
            _colliderStack.Remove(other.transform);
            return;
        }

        // pop from stack
        if (_colliderStack.Count == 0)
        {
            return;
        }
        
        if (_colliderStack.Last() == other.transform)
        {
            _colliderStack.Remove(_colliderStack.Last());
        }
        else if(_colliderStack.Contains(other.transform))
        {
            _colliderStack.Remove(other.transform);
            return;
        }

        SetGameObjectIndicationState(_collidingObject, IndicatorBase.IndicateState.Off);

        if (!_objectInHand)
        {
            StandardHapticBuzz();
        }

        if (_colliderStack.Count > 0)
        {
            SetCollidingObject(_colliderStack.Last().gameObject, ColliderSource.Exit);
        }
        else
        {
            _collidingObject = null;
        }
    }

    protected override void OnInteractPressedImpl(
        SteamVR_Action_Boolean fromAction, 
        SteamVR_Input_Sources fromSource)
    {
        if (_collidingObject)
        {
            GrabObject();
        }
    }

    protected override void OnInteractReleasedImpl(
        SteamVR_Action_Boolean fromAction, 
        SteamVR_Input_Sources fromSource)
    {
        if (_objectInHand)
        {
            ReleaseObject();
        }
    }

	protected virtual void SetCollidingObject(
		GameObject col, 
		ColliderSource fromWhere)
	{
		GameObject colGameObject = col;
		// does the colliding object want its parent picked up instead of itself?
		colGameObject = GrabParentCheck(colGameObject);

		if (_collidingObject == colGameObject)
		{
			return;
		}

		// if we already have an object we've collided with
		if (_collidingObject)
		{

			if (!_objectInHand)
			{
				SetGameObjectIndicationState(_collidingObject, IndicatorBase.IndicateState.Off);
				if (ColliderSource.Enter == fromWhere)
				{
                    StandardHapticBuzz();
				}
			}
		}

		_collidingObject = colGameObject;

		if (!_objectInHand)
		{
			SetGameObjectIndicationState(_collidingObject, IndicatorBase.IndicateState.Highlight);
			if (ColliderSource.Enter == fromWhere)
			{
                StandardHapticBuzz();
			}
		}
	}

	protected void GrabObject()
	{
		// if the colliding object's parent is a controller, get the controller to release it
		var colldingObjectParentMove = _collidingObject.GetComponentInParent<ViveControllerToolMove>();
		if (colldingObjectParentMove)
		{
			return;
		}

		_objectInHand = _collidingObject;
		_collidingObject = null;

		GrabActionsUtils.Instance.GrabOrRelease(_objectInHand, GrabActionsBase.GrabReleaseActions.OnPreGrab);

		_objectInHandOldParent = _objectInHand.transform.parent;
		_objectInHand.transform.parent = transform;

		SetGameObjectIndicationState(_objectInHand, IndicatorBase.IndicateState.Active);

		GrabActionsUtils.Instance.GrabOrRelease(_objectInHand, GrabActionsBase.GrabReleaseActions.OnPostGrab);
	}

	protected void ReleaseObject()
	{
		GrabActionsUtils.Instance.GrabOrRelease(_objectInHand, GrabActionsBase.GrabReleaseActions.OnPreRelease);

		_objectInHand.transform.parent = _objectInHandOldParent;
		_objectInHandOldParent = null;

		SetGameObjectIndicationState(_objectInHand, IndicatorBase.IndicateState.Highlight);

		GrabActionsUtils.Instance.GrabOrRelease(_objectInHand, GrabActionsBase.GrabReleaseActions.OnPostRelease);
		Debug.Log("PostRelease action");

		var thisCollider = GetComponent<BoxCollider>();
		var otherCollider = _objectInHand.GetComponent<Collider>();
		var direction = Vector3.zero;
		float distance = 0.0f;
		if (thisCollider && otherCollider && false == Physics.ComputePenetration(
				thisCollider,
				transform.TransformPoint(thisCollider.center),
				transform.rotation,
				otherCollider,
				otherCollider.transform.position,
				otherCollider.transform.rotation,
				out direction,
				out distance))
		//{
		//	Debug.Log("Release Penetration Test - TRUE");
		//}
		//else
		{
			Debug.Log("Release Penetration Test - FALSE");
			SetGameObjectIndicationState(_objectInHand, IndicatorBase.IndicateState.Off);
		}

		_objectInHand = null;
	}

	protected virtual void SetGameObjectIndicationState(
		GameObject gameObject, IndicatorBase.IndicateState state)
    {
        var indicators = gameObject.GetComponents<IndicatorBase>();

        foreach (var indicator in indicators)
        {
            indicator.Indicate = state;
        }
    }

    private GameObject GrabParentCheck(GameObject colGameObjectIn)
    {
        while (colGameObjectIn.gameObject.GetComponent<MoveParent>())
        {
            var parentTransform = colGameObjectIn.transform.parent;
            if (parentTransform)
            {
                colGameObjectIn = parentTransform.gameObject;
            }
        }

        return colGameObjectIn;
    }
}
