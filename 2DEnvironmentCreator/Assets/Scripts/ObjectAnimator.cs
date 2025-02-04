using UnityEngine;

public class ObjectAnimator : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetTrigger("OnPlace"); // Speel af bij plaatsen
    }

    private void OnMouseDown()
    {
        animator.SetTrigger("OnClick"); // Speel af bij klikken
    }
}
