using UnityEngine;
using System.Collections;

public class NPCMovement : MonoBehaviour
{
    [Header("Roaming Settings")]
    public float roamRadius = 3f;
    public float speed = 1f;
    public float minWaitTime = 1f;
    public float maxWaitTime = 3f;

    private Vector3 startPos;
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
        startPos = transform.position;
    }

    void Start()
    {
        StartCoroutine(RoamRoutine());
    }

    private IEnumerator RoamRoutine()
    {
        while (true)
        {
            // Rastgele nokta seç
            Vector2 offset = Random.insideUnitCircle * roamRadius;
            Vector3 target = startPos + new Vector3(offset.x, offset.y, 0f);

            // Hedefe hareket et
            while ((transform.position - target).sqrMagnitude > 0.01f)
            {
                Vector3 dir = (target - transform.position).normalized;
                animator.SetFloat("InputX", dir.x);
                animator.SetFloat("InputY", dir.y);
                animator.SetBool("IsMoving", true);

                transform.position += dir * speed * Time.deltaTime;
                yield return null;
            }

            animator.SetBool("IsMoving", false);

            // Rastgele bekleme süresi
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTime);
        }
    }
    
    void OnMouseDown()
    {
        FacePlayer();
    }

    private void FacePlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;
        Vector3 dir = playerObj.transform.position - transform.position;
        Vector2 dir2D = new Vector2(dir.x, dir.y).normalized;
        animator.SetFloat("InputX", dir2D.x);
        animator.SetFloat("InputY", dir2D.y);
        animator.SetBool("IsMoving", false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, roamRadius);
    }
}