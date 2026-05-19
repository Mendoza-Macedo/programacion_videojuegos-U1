using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

        private void Awake()
        {
            Instancia = this;
            Time.timeScale = 0f;
            if (panelMenu != null) panelMenu.SetActive(false);
            if (panelPausa != null) panelPausa.SetActive(false);
            if (panelVictoria != null) panelVictoria.SetActive(false);
        }

        private void Update()
        {
            Keyboard teclado = Keyboard.current;
            if (teclado == null) return;

            if (juegoIniciado && !terminado && teclado.escapeKey.wasPressedThisFrame)
            {
                CambiarPausa();
            }

            if (!juegoIniciado && panelMenu != null && panelMenu.activeSelf && (teclado.enterKey.wasPressedThisFrame || teclado.spaceKey.wasPressedThisFrame))
            {
                IniciarJuego();
            }

            if ((terminado || pausado) && teclado.rKey.wasPressedThisFrame)
            {
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
    }
}
