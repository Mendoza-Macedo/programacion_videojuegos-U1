using JuegoCooperativo.Juego;
using UnityEngine;

namespace JuegoCooperativo.Mundo
{
    public class ObstaculoGiratorio : MonoBehaviour
    {
        [SerializeField] private float velocidadGiro = 110f;

        private void Update()
        {
            transform.Rotate(0f, 0f, velocidadGiro * Time.deltaTime);
        }

        private void OnCollisionEnter2D(Collision2D colision)
        {
            if (colision.collider.CompareTag("Jugador"))
            {
                GestorJuego.Instancia?.ReaparecerJugadores();
            }
        }
    }
}
