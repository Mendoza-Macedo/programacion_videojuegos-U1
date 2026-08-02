using System.Collections;
using JuegoCooperativo.Juego;
using JuegoCooperativo.Personajes;
using UnityEngine;

namespace JuegoCooperativo.Campania
{
    public class PortalEntrada : MonoBehaviour
    {
        [SerializeField] private float separacionJugadores = 1.15f;

        private IEnumerator Start()
        {
            yield return null;
            JugadorCooperativo[] jugadores = FindObjectsByType<JugadorCooperativo>(FindObjectsSortMode.None);
            if (jugadores.Length < 2) yield break;

            OrdenarJugadores(jugadores);
            Vector3 centro = transform.position + Vector3.up * 0.15f;
            jugadores[0].Teletransportar(centro + Vector3.left * separacionJugadores);
            jugadores[1].Teletransportar(centro + Vector3.right * separacionJugadores);
            GestorJuego.Instancia?.ConfigurarReaparicionInicial(centro);
        }

        private static void OrdenarJugadores(JugadorCooperativo[] jugadores)
        {
            System.Array.Sort(jugadores, (a, b) => string.CompareOrdinal(a.NombreJugador, b.NombreJugador));
        }
    }
}
