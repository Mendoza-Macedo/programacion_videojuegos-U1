using JuegoCooperativo.Juego;
using UnityEngine;

namespace JuegoCooperativo.Mundo
{
    public class PuntoControl : MonoBehaviour
    {
        [SerializeField] private Color colorInactivo = new Color(0.9f, 0.7f, 0.25f);
        [SerializeField] private Color colorActivo = new Color(0.3f, 1f, 0.55f);

        private bool activado;
        private SpriteRenderer sprite;

        private void Awake()
        {
            sprite = GetComponent<SpriteRenderer>();
            if (sprite != null) sprite.color = colorInactivo;
        }

        private void OnTriggerEnter2D(Collider2D otro)
        {
            if (activado || !otro.CompareTag("Jugador")) return;

            activado = true;
            if (sprite != null) sprite.color = colorActivo;
            GestorJuego.Instancia?.RegistrarPuntoControl(transform.position + Vector3.up * 0.5f);
        }
    }
}
