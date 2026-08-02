using System.Collections.Generic;
using JuegoCooperativo.Personajes;
using UnityEngine;

namespace JuegoCooperativo.Campania
{
    [RequireComponent(typeof(Collider2D))]
    public class PortalSalida : MonoBehaviour
    {
        private readonly HashSet<JugadorCooperativo> jugadoresDentro = new HashSet<JugadorCooperativo>();
        private bool activado;

        private void OnTriggerEnter2D(Collider2D otro)
        {
            JugadorCooperativo jugador = otro.GetComponentInParent<JugadorCooperativo>();
            if (jugador == null) return;
            jugadoresDentro.Add(jugador);
            IntentarAvanzar();
        }

        private void OnTriggerExit2D(Collider2D otro)
        {
            JugadorCooperativo jugador = otro.GetComponentInParent<JugadorCooperativo>();
            if (jugador != null) jugadoresDentro.Remove(jugador);
        }

        private void IntentarAvanzar()
        {
            if (activado || jugadoresDentro.Count < 2 || ControlCampania.Instancia == null) return;
            activado = true;
            JuegoCooperativo.Audio.SonidosJuego.Instancia?.ReproducirPortal();
            ControlCampania.Instancia.AvanzarNivel();
        }
    }
}
