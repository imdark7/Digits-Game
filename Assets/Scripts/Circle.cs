using System.Collections;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class Circle : MonoBehaviour
{
    public int digit;
    internal Vector3 targetPosition = new Vector3(0, 0, 1);

    private void Update()
    {
        if (targetPosition != transform.position)
        {
            Move();
        }
    }

    public IEnumerator MoveCoroutine(Vector3 position)
    {
        targetPosition = position;
        var t = 0f;
        const float animationDuration = 0.2f;

        while (t < 1)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, t);
            t += Time.deltaTime / animationDuration;
            yield return null;
        }
    }

    public void Move(Vector3 position)
    {
        targetPosition = position;
        Move();
    }

    private void Move()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, 8f * Time.deltaTime);
    }

    public Circle Init(int value = default)
    {
        digit = value == default ? Random.Range(1, GameManager.DigitCap) : value;
        GetComponentInChildren<TextMeshPro>().text = digit.ToString();
        return this;
    }
}