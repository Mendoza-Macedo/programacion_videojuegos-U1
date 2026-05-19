using JuegoCooperativo.Juego;
using UnityEngine;

namespace JuegoCooperativo.Mundo
{
    public class MetaNivel : MonoBehaviour
    {
        private int jugadoresDentro;

        private void OnTriggerEnter2D(Collider2D otro)
        {
            if (!otro.CompareTag("Jugador")) return;
            jugadoresDentro++;
            if (jugadoresDentro >= 2)
            {
                GestorJuego.Instancia?.TerminarNivel();
            }
        }

        private void OnTriggerExit2D(Collider2D otro)
        {
            if (!otro.CompareTag("Jugador")) return;
            jugadoresDentro = Mathf.Max(0, jugadoresDentro - 1);
        }
    }
}
