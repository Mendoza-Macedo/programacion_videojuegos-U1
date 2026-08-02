using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace JuegoCooperativo.Campania
{
    [RequireComponent(typeof(TransicionEscenas))]
    public class ControlCampania : MonoBehaviour
    {
        private TransicionEscenas transicion;
        private int nivelActual;
        private bool finalMostrado;
        private GUIStyle estiloNivel;
        private GUIStyle estiloFinal;

        public static ControlCampania Instancia { get; private set; }
        public int NivelActual => nivelActual;
        public bool FinalMostrado => finalMostrado;

        private void Awake()
        {
            Instancia = this;
            transicion = GetComponent<TransicionEscenas>();
            nivelActual = ExtraerNivel(SceneManager.GetActiveScene().name);
            if (nivelActual > 0) EstadoCampania.DesbloquearNivel(nivelActual);
        }

        private void Update()
        {
            if (!finalMostrado) return;
            Keyboard teclado = Keyboard.current;
            bool aceptar = teclado != null && (teclado.enterKey.wasPressedThisFrame || teclado.spaceKey.wasPressedThisFrame);
            if (Gamepad.current != null) aceptar |= Gamepad.current.buttonSouth.wasPressedThisFrame;
            if (aceptar) VolverAlInicio();
        }

        public void AvanzarNivel()
        {
            if (transicion.EstaCargando || finalMostrado) return;

            if (nivelActual >= 8)
            {
                finalMostrado = true;
                Time.timeScale = 0f;
                return;
            }

            int siguiente = Mathf.Clamp(nivelActual + 1, 1, 8);
            EstadoCampania.DesbloquearNivel(siguiente);
            transicion.Cargar("Scene" + siguiente);
        }

        public void ContinuarCampania()
        {
            transicion.Cargar("Scene" + EstadoCampania.NivelDesbloqueado);
        }

        private void VolverAlInicio()
        {
            Time.timeScale = 1f;
            finalMostrado = false;
            transicion.Cargar("Scene1");
        }

        private void OnGUI()
        {
            if (estiloNivel == null)
            {
                estiloNivel = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 22,
                    fontStyle = FontStyle.Bold
                };
                estiloNivel.normal.textColor = Color.white;

                estiloFinal = new GUIStyle(estiloNivel)
                {
                    fontSize = 34,
                    wordWrap = true
                };
            }

            if (nivelActual > 0 && !finalMostrado)
            {
                Rect sombra = new Rect(Screen.width - 225, 21, 200, 42);
                GUI.color = new Color(0f, 0f, 0f, 0.65f);
                GUI.Box(sombra, GUIContent.none);
                GUI.color = Color.white;
                GUI.Label(sombra, $"NIVEL {nivelActual} / 8", estiloNivel);
            }

            if (!finalMostrado) return;
            GUI.color = new Color(0f, 0f, 0f, 0.86f);
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none);
            GUI.color = Color.white;
            Rect mensaje = new Rect(Screen.width * 0.15f, Screen.height * 0.27f, Screen.width * 0.7f, Screen.height * 0.45f);
            GUI.Label(mensaje,
                "¡CUMBRE ALCANZADA!\n\nCompletaron los 8 niveles\n" +
                $"Monedas reunidas: {EstadoCampania.Monedas}\n\nENTER / A: volver al inicio",
                estiloFinal);
        }

        public static int ExtraerNivel(string nombreEscena)
        {
            Match coincidencia = Regex.Match(nombreEscena ?? string.Empty, @"^Scene([1-8])$");
            return coincidencia.Success ? int.Parse(coincidencia.Groups[1].Value) : 0;
        }
    }
}
