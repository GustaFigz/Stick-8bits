using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Follow")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField] private float smoothTime = 0.12f;
    [SerializeField] private bool useUnscaledTime = true;

    private Vector3 _velocity;

    private void Awake()
    {
        if (target == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = target.position + offset;
        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        // SmoothDamp precisa do deltaTime para suavizar; usando unscaled faz a camera continuar seguindo mesmo pausado.
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, smoothTime, Mathf.Infinity, dt);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
