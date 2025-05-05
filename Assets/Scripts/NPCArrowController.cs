using UnityEngine;

[RequireComponent(typeof(BaseNPCController))]
public class NPCArrowController : MonoBehaviour
{
    public float showDistance = 2f; // Arrow’un görüneceği mesafe
    private Transform player;
    private GameObject arrow;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        arrow = transform.Find("NPCArrow")?.gameObject;
        if (arrow != null) arrow.SetActive(false);
    }

    void Update()
    {
        if (player == null || arrow == null) return;
        float dist = Vector2.Distance(player.position, transform.position);
        arrow.SetActive(dist <= showDistance);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, showDistance);
    }
}