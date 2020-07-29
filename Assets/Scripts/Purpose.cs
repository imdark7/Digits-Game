
using UnityEngine;

public class Purpose : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private static readonly int DissolveNamedId = Shader.PropertyToID("Dissolve");
    public float DissolveState = -0.15f;

    // Start is called before the first frame update
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.material.SetFloat(DissolveNamedId, DissolveState);
        GetComponent<Animator>().Play("PurposeDissolve");
    }

    private void Update()
    {
        var x = spriteRenderer.material;
        var xx = x.GetFloat(DissolveNamedId);
        spriteRenderer.material.SetFloat(DissolveNamedId, DissolveState);
    }
}