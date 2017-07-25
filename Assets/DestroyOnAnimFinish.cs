using UnityEngine;

public class DestroyOnAnimFinish : MonoBehaviour
{

    private Animator _animator;

    void Start()
    {
        _animator = transform.GetChild(0).GetComponent<Animator>();
    }

	// Update is called once per frame
	void Update () {
	    if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !_animator.IsInTransition(0))
	    {
	        Destroy(this.gameObject);
	    }
	}
}
