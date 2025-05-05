using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SortingOrderByY : MonoBehaviour
{
    [Tooltip("If true, order in layer is set once and script disables itself.")]
    public bool isStatic = true;
    [Tooltip("Multiplier to convert Y position to sorting order (higher smoothness).")]
    public float yMultiplier = 100f;

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        UpdateOrder();
        if (isStatic)
            enabled = false;
    }

    void LateUpdate()
    {
        if (!isStatic)
            UpdateOrder();
    }

    void UpdateOrder()
    {
        sr.sortingOrder = Mathf.RoundToInt(-transform.position.y * yMultiplier);
    }
}