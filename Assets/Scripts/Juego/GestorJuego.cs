using JuegoCooperativo.Personajes;
using JuegoCooperativo.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JuegoCooperativo.Juego
{
    public class GestorJuego : MonoBehaviour
    {
        [SerializeField] private JugadorCooperativo jugadorUno;
        [SerializeField] private JugadorCooperativo jugadorDos;
        [SerializeField] private Vector3 puntoReaparicionUno = new Vector3(-1.2f, 1.5f, 0f);
        [SerializeField] private Vector3 puntoReaparicionDos = new Vector3(1.2f, 1.5f, 0f);
        [SerializeField] private float separacionReaparicion = 1.2f;
        [SerializeField] private UnityEngine.UI.Text textoEstado;
        [SerializeField] private string claveMejorTiempo = "MejorTiempoNivelSierra";

        private float tiempoInicio;
        private bool nivelTerminado;

        public static GestorJuego Instancia { get; private set; }

        private void Awake()
        {
            Instancia = this;
            tiempoInicio = Time.time;
        }

        private void Update()
        {
            if (KeyboardDisponibleReinicio())
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }

            if (textoEstado != null && !nivelTerminado)
            {
                float tiempo = Time.time - tiempoInicio;
                float mejor = PlayerPrefs.GetFloat(claveMejorTiempo, 0f);
                string textoMejor = mejor > 0f ? $"Mejor: {mejor:0.0}s" : "Mejor: --";
                textoEstado.text = $"Tiempo: {tiempo:0.0}s  |  {textoMejor}\nEsc: Pausa  |  R: Reiniciar\nWASD/Shift: Azul  |  Flechas/ShiftDer: Rojo";
            }
        }

        private bool KeyboardDisponibleReinicio()
        {
            var teclado = UnityEngine.InputSystem.Keyboard.current;
            return teclado != null && teclado.rKey.wasPressedThisFrame && Time.timeScale > 0f;
        }

        public void RegistrarPuntoControl(Vector3 centro)
        {
            puntoReaparicionUno = centro + Vector3.left * separacionReaparicion;
            puntoReaparicionDos = centro + Vector3.right * separacionReaparicion;
            MostrarMensajeTemporal("Punto de control guardado");
            JuegoCooperativo.Audio.SonidosJuego.Instancia?.ReproducirCheckpoint();
        }

        public void ReaparecerJugadores()
        {
            if (jugadorUno != null) jugadorUno.Teletransportar(puntoReaparicionUno);
            if (jugadorDos != null) jugadorDos.Teletransportar(puntoReaparicionDos);
            MostrarMensajeTemporal("Vuelvan a intentarlo");
            JuegoCooperativo.Audio.SonidosJuego.Instancia?.ReproducirDano();
            FindFirstObjectByType<JuegoCooperativo.Camara.CamaraCooperativa>()?.Sacudir(0.18f, 0.2f);
        }

        public void TerminarNivel()
        {
            if (nivelTerminado) return;
            nivelTerminado = true;
            float tiempo = Time.time - tiempoInicio;
            float mejorTiempo = PlayerPrefs.GetFloat(claveMejorTiempo, 0f);
            if (mejorTiempo <= 0f || tiempo < mejorTiempo)
            {
                mejorTiempo = tiempo;
                PlayerPrefs.SetFloat(claveMejorTiempo, mejorTiempo);
                PlayerPrefs.Save();
            }

            if (textoEstado != null)
            {
                textoEstado.text = $"META ALCANZADA\nTiempo final: {tiempo:0.0}s\nR: Reiniciar";
            }

            JuegoCooperativo.Audio.SonidosJuego.Instancia?.ReproducirVictoria();
            FlujoJuegoUI.Instancia?.MostrarVictoria(tiempo, mejorTiempo);
        }

        private void MostrarMensajeTemporal(string mensaje)
        {
            if (textoEstado == null || nivelTerminado) return;
            textoEstado.text = mensaje;
        }
    }
}
