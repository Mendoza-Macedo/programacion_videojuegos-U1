using JuegoCooperativo.Personajes;
using JuegoCooperativo.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

        private void Start()
        {
            ActivarSeleccion();
        }

        private void Update()
        {
            if (seleccionTerminada) return;

            Keyboard teclado = Keyboard.current;
            if (teclado == null) return;

            if (!jugadorUnoListo)
            {
                if (teclado.aKey.wasPressedThisFrame) animadorUno.CambiarAnimal(-1);
                if (teclado.dKey.wasPressedThisFrame) animadorUno.CambiarAnimal(1);
                if (teclado.wKey.wasPressedThisFrame) jugadorUnoListo = true;
            }

            if (!jugadorDosListo)
            {
                if (teclado.leftArrowKey.wasPressedThisFrame) animadorDos.CambiarAnimal(-1);
                if (teclado.rightArrowKey.wasPressedThisFrame) animadorDos.CambiarAnimal(1);
                if (teclado.upArrowKey.wasPressedThisFrame) jugadorDosListo = true;
            }

            if (teclado.enterKey.wasPressedThisFrame || teclado.numpadEnterKey.wasPressedThisFrame)
            {
                jugadorUnoListo = true;
                jugadorDosListo = true;
            }

            ActualizarTexto();

            if (jugadorUnoListo && jugadorDosListo)
            {
                TerminarSeleccion();
            }
        }

        private void ActivarSeleccion()
        {
            seleccionTerminada = false;
            jugadorUnoListo = false;
            jugadorDosListo = false;

            if (jugadorUno != null) jugadorUno.enabled = false;
            if (jugadorDos != null) jugadorDos.enabled = false;
            if (textoSeleccion != null) textoSeleccion.gameObject.SetActive(true);
            if (panelSeleccion != null) panelSeleccion.SetActive(true);

            ActualizarTexto();
        }

        private void TerminarSeleccion()
        {
            seleccionTerminada = true;

            if (jugadorUno != null) jugadorUno.enabled = true;
            if (jugadorDos != null) jugadorDos.enabled = true;
            if (textoSeleccion != null) textoSeleccion.gameObject.SetActive(false);
            if (panelSeleccion != null) panelSeleccion.SetActive(false);
            FlujoJuegoUI.Instancia?.IniciarJuego();
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
                nombre.text = animador.NombreAnimalActual;
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


