using System.Collections;
using UnityEngine;

public class TankDeath : MonoBehaviour
{
    public float force = 500f;
    public float radius = 20;

    [SerializeField] private float timeUntilDissolve = 2f;
    [SerializeField] private float dissolveTime = 0.7f;

    private Renderer[] _renderers;
    private bool _shouldDissolve;
    private float _dissolveValue;
    private readonly string _dissolveString = "_Dissolve";

    private void Start()
    {
        var rbs = GetComponentsInChildren<Rigidbody>();
        _renderers = GetComponentsInChildren<Renderer>();
        foreach (var rb in rbs)
        {
            rb.AddExplosionForce(force, transform.position + Vector3.up, radius);
        }
        StartCoroutine(HandleDestruction());
    }

    private void Update()
    {
        if (!_shouldDissolve) return;
        _dissolveValue = Mathf.Lerp(_dissolveValue, 1f, dissolveTime * Time.deltaTime);
        foreach (var rnd in _renderers)
        {
            rnd.material.SetFloat(_dissolveString, _dissolveValue);
        }
    }

    private IEnumerator HandleDestruction()
    {
        yield return new WaitForSeconds(timeUntilDissolve);
        _shouldDissolve = true;
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }
}
