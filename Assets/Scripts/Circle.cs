using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class Circle : MonoBehaviour
{
    internal int Digit;
    private Vector3 targetPosition = new Vector3(0, 0, 1);
    internal bool IsMoving => transform.position != targetPosition;
    public Color[] colors = new Color[16];

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, 20f * Time.deltaTime);
    }

    public void Move(Vector2 position) => targetPosition = position;

    public Circle Init(int value = default)
    {
        Digit = value == default 
            ? Random.Range(1, GameManager.DigitCap - (int)(GameManager.DigitCap * 0.2)) 
            : value;
        GetComponent<SpriteRenderer>().color = colors[Digit - 1];
        GetComponentInChildren<TextMeshPro>().text = Digit.ToString();
        GetComponent<Animation>().Play("CircleInit");
        targetPosition = transform.position;
        return this;
    }
}