using UnityEngine;

public class SphereObject : MonoBehaviour
{
    private Renderer _renderer;
    private MaterialPropertyBlock _propertyBlock;

    public bool IsColliding { get; private set; }

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _propertyBlock = new MaterialPropertyBlock();
    }

    private void SetColor(Color color)
    {
        _propertyBlock.SetColor("_Color", color);
        _renderer.SetPropertyBlock(_propertyBlock);
    }

    public void SetColliding(bool isColliding)
    {
        if (IsColliding == isColliding) return;

        IsColliding = isColliding;
        SetColor(IsColliding ? Color.red : Color.white);
    }
}