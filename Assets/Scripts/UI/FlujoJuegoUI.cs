using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using JuegoCooperativo.Campania;

namespace JuegoCooperativo.UI
{
    public class FlujoJuegoUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelMenu;
        [SerializeField] private GameObject panelPausa;
        [SerializeField] private GameObject panelVictoria;
        [SerializeField] private Text textoVictoria;

        private bool juegoIniciado;
        private bool pausado;
        private bool terminado;

        public static FlujoJuegoUI Instancia { get; private set; }
        public bool JuegoIniciado => juegoIniciado;

        private void Awake()
        {
            Instancia = this;
            juegoIniciado = panelMenu == null || !panelMenu.activeSelf;
            Time.timeScale = juegoIniciado ? 1f : 0f;
            if (panelPausa != null) panelPausa.SetActive(false);
            if (panelVictoria != null) panelVictoria.SetActive(false);
            AgregarAyudaCampania();
        }

        private void Update()
        {
            Keyboard teclado = Keyboard.current;
            Gamepad mando = Gamepad.current;
            bool pausar = (teclado != null && teclado.escapeKey.wasPressedThisFrame) ||
                          (mando != null && mando.startButton.wasPressedThisFrame);
            bool empezar = (teclado != null && (teclado.enterKey.wasPressedThisFrame || teclado.spaceKey.wasPressedThisFrame)) ||
                           (mando != null && mando.buttonSouth.wasPressedThisFrame);
            bool reiniciar = (teclado != null && teclado.rKey.wasPressedThisFrame) ||
                              (mando != null && mando.selectButton.wasPressedThisFrame);

            if (juegoIniciado && !terminado && pausar)
            {
                CambiarPausa();
            }

            if (!juegoIniciado && panelMenu != null && panelMenu.activeSelf && empezar)
            {
                if (ControlCampania.ExtraerNivel(SceneManager.GetActiveScene().name) == 1)
                {
                    EstadoCampania.IniciarNuevaCampania();
                }
                IniciarJuego();
            }

            if (!juegoIniciado && panelMenu != null && panelMenu.activeSelf && teclado != null && teclado.cKey.wasPressedThisFrame)
            {
                ControlCampania.Instancia?.ContinuarCampania();
            }

            if ((terminado || pausado) && reiniciar)
            {
                JuegoCooperativo.Audio.SonidosJuego.Instancia?.ReproducirReinicio();
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        public void IniciarJuego()
        {
            juegoIniciado = true;
            pausado = false;
            Time.timeScale = 1f;
            if (panelMenu != null) panelMenu.SetActive(false);
            if (panelPausa != null) panelPausa.SetActive(false);
        }

        public void CambiarPausa()
        {
            pausado = !pausado;
            Time.timeScale = pausado ? 0f : 1f;
            if (panelPausa != null) panelPausa.SetActive(pausado);
        }

        public void MostrarVictoria(float tiempo, float mejorTiempo)
        {
            terminado = true;
            Time.timeScale = 0f;
            if (panelVictoria != null) panelVictoria.SetActive(true);
            if (textoVictoria != null)
            {
                textoVictoria.text = $"META ALCANZADA\nTiempo: {tiempo:0.0}s\nMejor tiempo: {mejorTiempo:0.0}s\nR: reiniciar";
            }
        }

        private void AgregarAyudaCampania()
        {
            if (panelMenu == null || ControlCampania.ExtraerNivel(SceneManager.GetActiveScene().name) != 1) return;
            Text texto = panelMenu.GetComponentInChildren<Text>(true);
            if (texto != null && !texto.text.Contains("C: continuar"))
            {
                texto.text += "\nC: continuar desde el nivel desbloqueado";
            }
        }
    }
}
