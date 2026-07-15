using UnityEngine;

namespace JuegoCooperativo.Mundo
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class BloqueEmpujable : MonoBehaviour
    {
        [SerializeField] private float velocidadMaxima = 4f;

        private Rigidbody2D cuerpo;

        private void Awake()
        {
            cuerpo = GetComponent<Rigidbody2D>();
            cuerpo.gravityScale = 1f;
            cuerpo.freezeRotation = true;
            cuerpo.mass = Mathf.Max(cuerpo.mass, 3.5f);
            cuerpo.linearDamping = 0.8f;
        }

        private void FixedUpdate()
        {
            if (cuerpo.linearVelocity.magnitude <= velocidadMaxima) return;
            cuerpo.linearVelocity = cuerpo.linearVelocity.normalized * velocidadMaxima;
        }
    }
}
