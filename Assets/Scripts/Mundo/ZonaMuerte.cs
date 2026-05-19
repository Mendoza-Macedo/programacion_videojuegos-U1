using JuegoCooperativo.Juego;
using UnityEngine;

namespace JuegoCooperativo.Mundo
{
    public class ZonaMuerte : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D otro)
        {
            if (otro.CompareTag("Jugador"))
            {
                GestorJuego.Instancia?.ReaparecerJugadores();
            }
        }
    }
}
