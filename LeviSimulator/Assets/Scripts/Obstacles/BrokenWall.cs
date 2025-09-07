using UnityEngine;

public class BrokenWall : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] Animator anim;
    [SerializeField] Rigidbody rig;
    
    void Start()
    {
        anim=GetComponent<Animator>();
        
        rig = GetComponentInChildren<Rigidbody>();
        rig.useGravity = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            anim.SetBool("Crushed", true);
            rig.useGravity = true;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
