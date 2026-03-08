using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    private const float Lifetime = 1f;
    private const float FloatSpeed = 1f;

    private TextMeshPro textMesh;
    private float timer;

    public static void Create(Vector3 position, int damage)
    {
        Debug.Log($"DamagePopup created: {damage}");

        GameObject popupObject = new GameObject("DamagePopup");
        popupObject.transform.position = new Vector3(position.x, position.y, -1f);

        DamagePopup popup = popupObject.AddComponent<DamagePopup>();
        popup.Initialize(damage);
    }

    private void Initialize(int damage)
    {
        textMesh = gameObject.AddComponent<TextMeshPro>();
        textMesh.text = damage.ToString();
        textMesh.color = Color.red;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.fontSize = 5f;
        textMesh.sortingOrder = 100;

        transform.localScale = Vector3.one * 0.25f;
    }

    private void Update()
    {
        transform.position += Vector3.up * FloatSpeed * Time.deltaTime;

        timer += Time.deltaTime;

        if (timer >= Lifetime)
        {
            Destroy(gameObject);
        }
    }
}