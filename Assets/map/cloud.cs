using UnityEngine;

public class cloud : MonoBehaviour
{
	[SerializeField] private float minX = -3f;
	[SerializeField] private float maxX = 3f;
	[SerializeField] private float moveSpeed = 0.65f;
	[SerializeField] private bool useLocalSpace = true;

	private int _direction = 1;
	private float _absInitialScaleX = 1f;

	private void Awake()
	{
		if (minX > maxX)
		{
			float tmp = minX;
			minX = maxX;
			maxX = tmp;
		}

		_absInitialScaleX = Mathf.Abs(transform.localScale.x);
		if (_absInitialScaleX < 0.001f)
			_absInitialScaleX = 1f;
	}

	private void Update()
	{
		Vector3 pos = useLocalSpace ? transform.localPosition : transform.position;
		pos.x += moveSpeed * _direction * Time.deltaTime;

		bool reachedMax = _direction > 0 && pos.x >= maxX;
		bool reachedMin = _direction < 0 && pos.x <= minX;

		if (reachedMax)
		{
			pos.x = maxX;
			ReverseDirection();
		}
		else if (reachedMin)
		{
			pos.x = minX;
			ReverseDirection();
		}

		if (useLocalSpace)
			transform.localPosition = pos;
		else
			transform.position = pos;
	}

	private void ReverseDirection()
	{
		_direction *= -1;
		Vector3 s = transform.localScale;
		s.x = _absInitialScaleX * (_direction >= 0 ? 1f : -1f);
		transform.localScale = s;
	}
}
