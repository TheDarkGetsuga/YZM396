using UnityEngine;

public class StompAttack : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "WeakPoint"){
            Destroy(collision.gameObject);
        }
    }
}
