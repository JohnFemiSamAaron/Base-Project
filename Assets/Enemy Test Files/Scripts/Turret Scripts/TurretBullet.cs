using UnityEngine;

public class TurretBullet : MonoBehaviour
{
    public Rigidbody2D rb;
    public int life = 3;
    public float speed = 30f;
    private Vector2 direction;
    public GameObject Owner { get; set; }
    // Start is called before the first frame update
    void Start()
    {
        this.direction = direction;
        Destroy(gameObject, 3f);
    }

    public void Shoot(Vector2 shootDirection)
    {
        this.direction = shootDirection;
        rb.velocity = this.direction * speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        life--;
        if (life<0)
        {
            Destroy(gameObject);
            return;
        }

        var firstContact = collision.contacts[0];
        Vector2 newVelocity = Vector2.Reflect(direction.normalized, firstContact.normal);
        Shoot(newVelocity.normalized);
    }
}
