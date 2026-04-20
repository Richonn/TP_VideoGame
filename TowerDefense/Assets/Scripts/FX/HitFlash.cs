using System.Collections;
using UnityEngine;

public class HitFlash : MonoBehaviour
{
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float duration = 0.1f;

    private SpriteRenderer[] _renderers;
    private Color[] _baseColors;
    private Coroutine _current;

    void Awake()
    {
        _renderers = GetComponentsInChildren<SpriteRenderer>();
        _baseColors = new Color[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
            _baseColors[i] = _renderers[i].color;
    }

    public void Flash()
    {
        if (_current != null) StopCoroutine(_current);
        _current = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        for (int i = 0; i < _renderers.Length; i++)
            if (_renderers[i] != null) _renderers[i].color = flashColor;

        yield return new WaitForSeconds(duration);

        for (int i = 0; i < _renderers.Length; i++)
            if (_renderers[i] != null) _renderers[i].color = _baseColors[i];

        _current = null;
    }
}
