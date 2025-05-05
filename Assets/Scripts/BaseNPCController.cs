using UnityEngine;

[RequireComponent(typeof(Animator))]
public class BaseNPCController : MonoBehaviour
{
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void OnMouseDown()
    {
        Debug.Log("NPC clicked: " + gameObject.name);
        FacePlayer();
    }

    protected void FacePlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
            return;
        Vector3 dir = playerObj.transform.position - transform.position;
        Vector2 dir2D = new Vector2(dir.x, dir.y).normalized;
        animator.SetFloat("InputX", dir2D.x);
        animator.SetFloat("InputY", dir2D.y);
        animator.SetBool("IsMoving", false);
    }
}