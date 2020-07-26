using System;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class Circle : MonoBehaviour
{
    internal int Digit;
    internal Vector3 TargetPosition = new Vector3(0, 0, 1);
    internal bool IsMoving => transform.position != TargetPosition;
    public Color[] colors = new Color[16];

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, TargetPosition, 20f * Time.deltaTime);
    }

    public void Move(Vector3 position) => TargetPosition = position;

    public Circle Init(int value = default)
    {
        Digit = value == default ? Random.Range(1, GameManager.DigitCap) : value;
        GetComponent<SpriteRenderer>().color = colors[Digit - 1];
        GetComponentInChildren<TextMeshPro>().text = Digit.ToString();
        GetComponent<Animation>().Play("CircleInit");
        TargetPosition = transform.position;
        return this;
    }
}