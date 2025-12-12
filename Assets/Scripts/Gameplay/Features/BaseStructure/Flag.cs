using UnityEngine;

public class Flag : MonoBehaviour
{
    [SerializeField] private Renderer _flagView;

    private void Awake()
    {
        ThrowIf.Null(_flagView, nameof(_flagView));

        _flagView.material.color = Random.ColorHSV();
    }
}