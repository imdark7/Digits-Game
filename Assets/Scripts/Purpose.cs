using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class Purpose : MonoBehaviour
{
    [FormerlySerializedAs("DissolveEvent")] public UnityEvent dissolveEvent = new UnityEvent();
    private Animator anim;
    private static readonly int Dissolve = Animator.StringToHash("Dissolve");

    void Start()
    {
        anim = GetComponent<Animator>();
        anim.SetBool(Dissolve, false);
        dissolveEvent.AddListener(() => anim.SetBool(Dissolve, true));
    }
}