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
        [SerializeField] private string claveMejorTiempo = "MejorTiempoNivel";

        private float tiempoInicio;
        private bool nivelTerminado;

        public static GestorJuego Instancia { get; private set; }

        private void Awake()
        {
            Instancia = this;
            Time.timeScale = 1f;
            tiempoInicio = Time.time;

            claveMejorTiempo = "MejorTiempo_" + SceneManager.GetActiveScene().name;
        }

        private void Update()
        {
            var teclado = UnityEngine.InputSystem.Keyboard.current;

            if (teclado != null && teclado.rKey.wasPressedThisFrame)
            {
                ReiniciarNivel();
                return;
            }

            if (nivelTerminado)
            {
                if (teclado != null && teclado.enterKey.wasPressedThisFrame)
                {
                    CargarSiguienteNivel();
                }

                return;
            }

            ActualizarTextoEstado();
        }

        private void ActualizarTextoEstado()
        {
            if (textoEstado == null) return;

            float tiempo = Time.time - tiempoInicio;
            float mejor = PlayerPrefs.GetFloat(claveMejorTiempo, 0f);
            string textoMejor = mejor > 0f ? $"Mejor: {mejor:0.0}s" : "Mejor: --";

            textoEstado.text =
                $"Tiempo: {tiempo:0.0}s  |  {textoMejor}\n" +
                "Esc: Pausa  |  R: Reiniciar\n" +
                "WASD/Shift: Azul  |  Flechas/ShiftDer: Rojo";
        }

        private void ReiniciarNivel()
        {
            JuegoCooperativo.Audio.SonidosJuego.Instancia?.ReproducirReinicio();
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void CargarSiguienteNivel()
        {
            Time.timeScale = 1f;

            int escenaActual = SceneManager.GetActiveScene().buildIndex;
            int siguienteEscena = escenaActual + 1;

            if (siguienteEscena < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(siguienteEscena);
            }
            else
            {
                SceneManager.LoadScene(0);
            }
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
                textoEstado.text =
                    "META ALCANZADA\n" +
                    $"Tiempo final: {tiempo:0.0}s\n" +
                    "ENTER: Siguiente nivel\n" +
                    "R: Reiniciar";
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
