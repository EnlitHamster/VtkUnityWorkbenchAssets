using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonBigSmall : MonoBehaviour {

	private Animator _animator;
	//public AudioSource Audio;
	//public AudioClip PlayClip;
	//public AudioClip PauseClip;
	//public bool Play = false;
	private Toggle _toggleScript;

	//[Range(0.5f, 5.0f)]
	//public float Speed = 5.0f;

	// Use this for initialization
	void Start ()
	{
		_animator = GetComponentInChildren<Animator>();
		_animator.speed = 6.0f;
		_toggleScript = GetComponent<Toggle>();
		//Audio = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		//if (!_animator)
		//{
		//	return;
		//}

		//_animator.speed = Speed;
		//if (Input.GetKeyDown("u"))
		//{
		//	_animator.Play("Bigger");
		//}
		//else if (Input.GetKeyDown("y"))
		//{
		//	_animator.Play("Smaller");
		//}
	}

	public void OnTriggerEnter(Collider other)
	{
		if (!_animator)
		{
			return;
		}

		_animator.Play("DownToUp", -1, 0.0f);
		_animator.Play("SmallToBig", -1, 0.0f);

		if (_toggleScript)
		{
			if (!_toggleScript.isOn)
			{
				_animator.Play("DarkToBright", -1, 0.0f);
			}
		}
		else
		{
			_animator.Play("DarkToBright", -1, 0.0f);
		}
	}

	public void OnTriggerExit(Collider other)
	{
		if (!_animator)
		{
			return;
		}

		_animator.Play("UpToDown", -1, 0.0f);
		_animator.Play("BigToSmall", -1, 0.0f);

		if (_toggleScript)
		{
			if (!_toggleScript.isOn)
			{
				_animator.Play("BrightToDark", -1, 0.0f);
			}
		}
		else
		{
			_animator.Play("BrightToDark", -1, 0.0f);
		}
	}

	public void OnClick()
	{
		if (!_animator)
		{
			return;
		}

		_animator.Play("UpToDownToUpQuick", -1, 0.0f);
		_animator.Play("BrightToWhiteToBrightQuick", -1, 0.0f);
	}

	public void OnToggle()
	{
		if (!_toggleScript || !_animator)
		{
			return;
		}

		_animator.Play("UpToDownToUpQuick", -1, 0.0f);

		if (_toggleScript.isOn)
		{
			_animator.Play("BrightToWhiteQuick", -1, 0.0f);
		}
		else
		{
			_animator.Play("WhiteToBrightQuick", -1, 0.0f);
		}

		//if (_animator == null)
		//{
		//	return;
		//}

		//_animator.Play("PressBrighten", -1, 0.0f);
	}
}
