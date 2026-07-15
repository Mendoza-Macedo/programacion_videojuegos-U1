using JuegoCooperativo.Personajes;
using UnityEngine;

namespace JuegoCooperativo.Mundo
{
    [RequireComponent(typeof(Collider2D))]
    public class MonedaPotenciadora : MonoBehaviour
    {
        [SerializeField] private float velocidadGiro = 120f;
        [SerializeField] private float amplitudFlotacion = 0.18f;
        [SerializeField] private float velocidadFlotacion = 3f;

        private Vector3 posicionInicial;
        private SpriteRenderer render;

        private void Awake()
        {
            posicionInicial = transform.position;
            render = GetComponent<SpriteRenderer>();

            Collider2D colision = GetComponent<Collider2D>();
            colision.isTrigger = true;
        }

        private void Update()
        {
            transform.Rotate(0f, 0f, velocidadGiro * Time.deltaTime);
            transform.position = posicionInicial + Vector3.up * (Mathf.Sin(Time.time * velocidadFlotacion) * amplitudFlotacion);

            if (render != null)
            {
                float pulso = 0.75f + Mathf.Sin(Time.time * 7f) * 0.2f;
                render.color = new Color(0.18f, 0.72f, 1f, pulso);
            }
        }

        private void OnTriggerEnter2D(Collider2D otro)
        {
            JugadorCooperativo jugador = otro.GetComponentInParent<JugadorCooperativo>();
            if (jugador == null) return;

            jugador.ActivarPotenciadorHabilidad();
            Destroy(gameObject);
        }
    }
}
