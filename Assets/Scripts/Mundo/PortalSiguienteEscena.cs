using System.Collections;
using System.Collections.Generic;
using JuegoCooperativo.Personajes;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JuegoCooperativo.Mundo
{
    [RequireComponent(typeof(Collider2D))]
    public class PortalSiguienteEscena : MonoBehaviour
    {
        [SerializeField] private float esperaCambio = 0.7f;
        [SerializeField] private int escenaFallback = 1;

        private readonly HashSet<JugadorCooperativo> jugadoresDentro = new HashSet<JugadorCooperativo>();
        private bool activado;
        private SpriteRenderer render;
        private Vector3 escalaInicial;
        private Transform visualRaiz;
        private LineRenderer anilloExterior;
        private LineRenderer anilloInterior;
        private LineRenderer espiral;
        private ParticleSystem particulas;

        private void Awake()
        {
            render = GetComponent<SpriteRenderer>();
            escalaInicial = transform.localScale;
            CrearVisualPortal();

            Collider2D colision = GetComponent<Collider2D>();
            colision.isTrigger = true;
        }

        private void Update()
        {
            float pulso = 1f + Mathf.Sin(Time.time * 5.5f) * 0.08f;
            transform.localScale = escalaInicial * pulso;

            if (visualRaiz != null)
            {
                visualRaiz.localRotation = Quaternion.Euler(0f, 0f, Time.time * 48f);
            }

            if (anilloExterior != null)
            {
                anilloExterior.widthMultiplier = 0.07f + Mathf.Sin(Time.time * 8f) * 0.018f;
                anilloExterior.transform.localRotation = Quaternion.Euler(0f, 0f, -Time.time * 90f);
            }

            if (anilloInterior != null)
            {
                anilloInterior.widthMultiplier = 0.045f + Mathf.Sin(Time.time * 11f) * 0.014f;
                anilloInterior.transform.localRotation = Quaternion.Euler(0f, 0f, Time.time * 135f);
            }

            if (espiral != null)
            {
                espiral.transform.localRotation = Quaternion.Euler(0f, 0f, -Time.time * 170f);
            }

            if (render != null)
            {
                float brillo = 0.25f + Mathf.Sin(Time.time * 4f) * 0.08f;
                render.color = new Color(0.05f, brillo, 0.45f, 0.25f);
            }
        }

        private void OnTriggerEnter2D(Collider2D otro)
        {
            JugadorCooperativo jugador = ObtenerJugador(otro);
            if (jugador == null || activado) return;

            jugadoresDentro.Add(jugador);
            if (jugadoresDentro.Count >= 2)
            {
                StartCoroutine(CambiarEscena());
            }
        }

        private void OnTriggerExit2D(Collider2D otro)
        {
            JugadorCooperativo jugador = ObtenerJugador(otro);
            if (jugador == null || activado) return;

            jugadoresDentro.Remove(jugador);
        }

        private IEnumerator CambiarEscena()
        {
            activado = true;
            yield return new WaitForSeconds(esperaCambio);

            int siguiente = SceneManager.GetActiveScene().buildIndex + 1;
            if (siguiente >= SceneManager.sceneCountInBuildSettings)
            {
                siguiente = Mathf.Clamp(escenaFallback, 0, SceneManager.sceneCountInBuildSettings - 1);
            }

            SceneManager.LoadScene(siguiente);
        }

        private static JugadorCooperativo ObtenerJugador(Collider2D otro)
        {
            return otro.GetComponentInParent<JugadorCooperativo>();
        }

        private void CrearVisualPortal()
        {
            if (render != null)
            {
                render.sortingOrder = 24;
            }

            visualRaiz = transform.Find("Visual Portal");
            if (visualRaiz == null)
            {
                visualRaiz = new GameObject("Visual Portal").transform;
                visualRaiz.SetParent(transform, false);
            }

            anilloExterior = CrearAnillo("Anillo exterior", 0.72f, 1.05f, 0.07f, new Color(0.10f, 0.95f, 1f, 0.95f), 34);
            anilloInterior = CrearAnillo("Anillo interior", 0.45f, 0.72f, 0.045f, new Color(0.72f, 0.30f, 1f, 0.92f), 35);
            espiral = CrearEspiral("Espiral portal", new Color(0.95f, 1f, 1f, 0.78f), 36);
            particulas = CrearParticulas();
        }

        private LineRenderer CrearAnillo(string nombre, float radioX, float radioY, float ancho, Color color, int orden)
        {
            Transform existente = visualRaiz.Find(nombre);
            GameObject objeto = existente != null ? existente.gameObject : new GameObject(nombre);
            objeto.transform.SetParent(visualRaiz, false);

            LineRenderer linea = objeto.GetComponent<LineRenderer>();
            if (linea == null) linea = objeto.AddComponent<LineRenderer>();

            linea.useWorldSpace = false;
            linea.loop = true;
            linea.positionCount = 72;
            linea.widthMultiplier = ancho;
            linea.sortingOrder = orden;
            linea.numCapVertices = 4;
            linea.material = new Material(Shader.Find("Sprites/Default"));
            linea.startColor = color;
            linea.endColor = color;

            for (int i = 0; i < linea.positionCount; i++)
            {
                float angulo = i / (float)linea.positionCount * Mathf.PI * 2f;
                linea.SetPosition(i, new Vector3(Mathf.Cos(angulo) * radioX, Mathf.Sin(angulo) * radioY, 0f));
            }

            return linea;
        }

        private LineRenderer CrearEspiral(string nombre, Color color, int orden)
        {
            Transform existente = visualRaiz.Find(nombre);
            GameObject objeto = existente != null ? existente.gameObject : new GameObject(nombre);
            objeto.transform.SetParent(visualRaiz, false);

            LineRenderer linea = objeto.GetComponent<LineRenderer>();
            if (linea == null) linea = objeto.AddComponent<LineRenderer>();

            linea.useWorldSpace = false;
            linea.loop = false;
            linea.positionCount = 56;
            linea.widthMultiplier = 0.035f;
            linea.sortingOrder = orden;
            linea.numCapVertices = 4;
            linea.material = new Material(Shader.Find("Sprites/Default"));
            linea.startColor = color;
            linea.endColor = new Color(0.30f, 0.95f, 1f, 0.05f);

            for (int i = 0; i < linea.positionCount; i++)
            {
                float progreso = i / (float)(linea.positionCount - 1);
                float angulo = progreso * Mathf.PI * 5.5f;
                float radio = Mathf.Lerp(0.08f, 0.68f, progreso);
                linea.SetPosition(i, new Vector3(Mathf.Cos(angulo) * radio, Mathf.Sin(angulo) * radio * 1.35f, 0f));
            }

            return linea;
        }

        private ParticleSystem CrearParticulas()
        {
            Transform existente = visualRaiz.Find("Chispas portal");
            GameObject objeto = existente != null ? existente.gameObject : new GameObject("Chispas portal");
            objeto.transform.SetParent(visualRaiz, false);

            ParticleSystem sistema = objeto.GetComponent<ParticleSystem>();
            if (sistema == null) sistema = objeto.AddComponent<ParticleSystem>();

            ParticleSystem.MainModule main = sistema.main;
            main.loop = true;
            main.startLifetime = 0.75f;
            main.startSpeed = 0.25f;
            main.startSize = 0.08f;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = 80;
            main.startColor = new Color(0.35f, 0.95f, 1f, 0.9f);

            ParticleSystem.EmissionModule emission = sistema.emission;
            emission.rateOverTime = 36f;

            ParticleSystem.ShapeModule shape = sistema.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.78f;
            shape.radiusThickness = 0.18f;

            ParticleSystem.ColorOverLifetimeModule color = sistema.colorOverLifetime;
            color.enabled = true;
            Gradient gradiente = new Gradient();
            gradiente.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(0.40f, 1f, 1f), 0f),
                    new GradientColorKey(new Color(0.86f, 0.42f, 1f), 0.55f),
                    new GradientColorKey(new Color(0.20f, 0.40f, 1f), 1f),
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.95f, 0.18f),
                    new GradientAlphaKey(0f, 1f),
                });
            color.color = gradiente;

            ParticleSystemRenderer rendererParticulas = sistema.GetComponent<ParticleSystemRenderer>();
            rendererParticulas.sortingOrder = 37;
            rendererParticulas.material = new Material(Shader.Find("Sprites/Default"));

            if (!sistema.isPlaying) sistema.Play();
            return sistema;
        }
    }
}
