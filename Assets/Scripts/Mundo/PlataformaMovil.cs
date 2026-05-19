using UnityEngine;

namespace JuegoCooperativo.Mundo
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlataformaMovil : MonoBehaviour
    {
        [SerializeField] private Vector2 desplazamiento = new Vector2(3f, 0f);
        [SerializeField] private float duracion = 2.5f;

        private Rigidbody2D cuerpo;
        private Vector2 inicio;

        private void Awake()
        {
            cuerpo = GetComponent<Rigidbody2D>();
            cuerpo.bodyType = RigidbodyType2D.Kinematic;
            inicio = transform.position;
        }

        private void FixedUpdate()
        {
            float t = Mathf.PingPong(Time.time / Mathf.Max(0.1f, duracion), 1f);
            Vector2 destino = Vector2.Lerp(inicio, inicio + desplazamiento, t);
            cuerpo.MovePosition(destino);
        }
    }
}
