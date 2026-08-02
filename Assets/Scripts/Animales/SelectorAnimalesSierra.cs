using JuegoCooperativo.Personajes;
using JuegoCooperativo.UI;
using JuegoCooperativo.Campania;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace JuegoCooperativo.Animales
{
    public class SelectorAnimalesSierra : MonoBehaviour
    {
        [SerializeField] private JugadorCooperativo jugadorUno;
        [SerializeField] private JugadorCooperativo jugadorDos;
        [SerializeField] private AnimadorAnimalSierra animadorUno;
        [SerializeField] private AnimadorAnimalSierra animadorDos;
        [SerializeField] private Text textoSeleccion;
        [SerializeField] private GameObject panelSeleccion;
        [SerializeField] private Image vistaPreviaUno;
        [SerializeField] private Image vistaPreviaDos;
        [SerializeField] private Text nombreUno;
        [SerializeField] private Text nombreDos;
        [SerializeField] private Text estadoUno;
        [SerializeField] private Text estadoDos;
        [SerializeField] private Image fondoUno;
        [SerializeField] private Image fondoDos;

        private bool jugadorUnoListo;
        private bool jugadorDosListo;
        private bool seleccionTerminada;
        private bool seleccionActiva;

        private void Start()
        {
            BuscarReferenciasFaltantes();
            AplicarSeleccionGuardada();

            int nivel = ControlCampania.ExtraerNivel(SceneManager.GetActiveScene().name);
            if (nivel > 1 && EstadoCampania.HaySeleccion)
            {
                TerminarSeleccion(false);
                return;
            }

            seleccionTerminada = false;
            seleccionActiva = false;
            if (panelSeleccion != null) panelSeleccion.SetActive(false);
            if (textoSeleccion != null) textoSeleccion.gameObject.SetActive(false);
            if (jugadorUno != null) jugadorUno.enabled = false;
            if (jugadorDos != null) jugadorDos.enabled = false;
        }

        private void Update()
        {
            if (seleccionTerminada) return;

            if (!seleccionActiva)
            {
                if (FlujoJuegoUI.Instancia == null || !FlujoJuegoUI.Instancia.JuegoIniciado) return;
                ActivarSeleccion();
                return;
            }

            Keyboard teclado = Keyboard.current;
            Gamepad mandoUno = Gamepad.all.Count > 0 ? Gamepad.all[0] : null;
            Gamepad mandoDos = Gamepad.all.Count > 1 ? Gamepad.all[1] : null;

            if (!jugadorUnoListo)
            {
                bool izquierda = teclado != null && teclado.aKey.wasPressedThisFrame;
                bool derecha = teclado != null && teclado.dKey.wasPressedThisFrame;
                izquierda |= mandoUno != null && mandoUno.dpad.left.wasPressedThisFrame;
                derecha |= mandoUno != null && mandoUno.dpad.right.wasPressedThisFrame;
                if (izquierda && animadorUno != null) animadorUno.CambiarAnimal(-1);
                if (derecha && animadorUno != null) animadorUno.CambiarAnimal(1);
                if ((teclado != null && teclado.wKey.wasPressedThisFrame) ||
                    (mandoUno != null && mandoUno.buttonSouth.wasPressedThisFrame)) jugadorUnoListo = true;
            }

            if (!jugadorDosListo)
            {
                bool izquierda = teclado != null && teclado.leftArrowKey.wasPressedThisFrame;
                bool derecha = teclado != null && teclado.rightArrowKey.wasPressedThisFrame;
                izquierda |= mandoDos != null && mandoDos.dpad.left.wasPressedThisFrame;
                derecha |= mandoDos != null && mandoDos.dpad.right.wasPressedThisFrame;
                if (izquierda && animadorDos != null) animadorDos.CambiarAnimal(-1);
                if (derecha && animadorDos != null) animadorDos.CambiarAnimal(1);
                if ((teclado != null && teclado.upArrowKey.wasPressedThisFrame) ||
                    (mandoDos != null && mandoDos.buttonSouth.wasPressedThisFrame)) jugadorDosListo = true;
            }

            if (teclado != null && (teclado.enterKey.wasPressedThisFrame || teclado.numpadEnterKey.wasPressedThisFrame))
            {
                jugadorUnoListo = true;
                jugadorDosListo = true;
            }

            ActualizarTexto();

            if (jugadorUnoListo && jugadorDosListo)
            {
                TerminarSeleccion(true);
            }
        }

        private void ActivarSeleccion()
        {
            seleccionActiva = true;
            seleccionTerminada = false;
            jugadorUnoListo = false;
            jugadorDosListo = false;

            if (jugadorUno != null) jugadorUno.enabled = false;
            if (jugadorDos != null) jugadorDos.enabled = false;
            if (textoSeleccion != null) textoSeleccion.gameObject.SetActive(true);
            if (panelSeleccion != null) panelSeleccion.SetActive(true);

            ActualizarTexto();
        }

        private void TerminarSeleccion(bool guardar)
        {
            seleccionTerminada = true;
            seleccionActiva = false;

            if (guardar && animadorUno != null && animadorDos != null)
            {
                EstadoCampania.GuardarSeleccion(animadorUno.IndiceAnimalActual, animadorDos.IndiceAnimalActual);
            }

            if (jugadorUno != null) jugadorUno.enabled = true;
            if (jugadorDos != null) jugadorDos.enabled = true;
            if (textoSeleccion != null) textoSeleccion.gameObject.SetActive(false);
            if (panelSeleccion != null) panelSeleccion.SetActive(false);
            FlujoJuegoUI.Instancia?.IniciarJuego();
            JuegoCooperativo.Juego.GestorJuego.Instancia?.ComenzarCronometro();
        }

        private void AplicarSeleccionGuardada()
        {
            if (!EstadoCampania.HaySeleccion) return;
            animadorUno?.AplicarAnimal(EstadoCampania.AnimalUno);
            animadorDos?.AplicarAnimal(EstadoCampania.AnimalDos);
        }

        private void BuscarReferenciasFaltantes()
        {
            JugadorCooperativo[] jugadores = FindObjectsByType<JugadorCooperativo>(FindObjectsSortMode.None);
            foreach (JugadorCooperativo jugador in jugadores)
            {
                bool esAzul = jugador.NombreJugador.ToLowerInvariant().Contains("azul");
                if (esAzul && jugadorUno == null) jugadorUno = jugador;
                if (!esAzul && jugadorDos == null) jugadorDos = jugador;
            }
            if (animadorUno == null && jugadorUno != null) animadorUno = jugadorUno.GetComponent<AnimadorAnimalSierra>();
            if (animadorDos == null && jugadorDos != null) animadorDos = jugadorDos.GetComponent<AnimadorAnimalSierra>();
        }

        private void ActualizarTexto()
        {
            if (animadorUno == null || animadorDos == null) return;

            string estadoUno = jugadorUnoListo ? "LISTO" : "A/D cambia - W confirma";
            string estadoDos = jugadorDosListo ? "LISTO" : "Flechas cambia - Arriba confirma";

            if (textoSeleccion != null)
            {
                textoSeleccion.text = "ESCOGE TU ANIMAL DE LA SIERRA";
            }

            ActualizarTarjeta(vistaPreviaUno, nombreUno, this.estadoUno, fondoUno, animadorUno, estadoUno, jugadorUnoListo);
            ActualizarTarjeta(vistaPreviaDos, nombreDos, this.estadoDos, fondoDos, animadorDos, estadoDos, jugadorDosListo);
        }

        private void ActualizarTarjeta(Image vistaPrevia, Text nombre, Text estado, Image fondo, AnimadorAnimalSierra animador, string textoEstado, bool listo)
        {
            if (vistaPrevia != null)
            {
                vistaPrevia.sprite = animador.SpriteVistaPrevia;
                vistaPrevia.preserveAspect = true;
            }

            if (nombre != null)
            {
                nombre.text = animador.EtiquetaAnimalActual;
            }

            if (estado != null)
            {
                estado.text = textoEstado;
                estado.color = listo ? new Color(0.45f, 1f, 0.62f) : Color.white;
            }

            if (fondo != null)
            {
                fondo.color = listo
                    ? new Color(0.12f, 0.34f, 0.21f, 0.92f)
                    : new Color(0.12f, 0.16f, 0.20f, 0.88f);
            }
        }
    }
}

