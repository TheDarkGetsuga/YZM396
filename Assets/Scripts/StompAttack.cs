using UnityEngine;

public class StompAttack : MonoBehaviour //siktir et mario yapmıyoruz
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "WeakPoint"){
            Destroy(collision.gameObject);
        }
    }
}
