using JuegoCooperativo.Personajes;
using UnityEngine;

namespace JuegoCooperativo.Cuerda
{
    [RequireComponent(typeof(LineRenderer))]
    public class CuerdaCooperativa : MonoBehaviour
    {
        [Header("Jugadores")]
        [SerializeField] private JugadorCooperativo jugadorUno;
        [SerializeField] private JugadorCooperativo jugadorDos;

        [Header("Fisica de cuerda")]
        [SerializeField] private float longitudMaxima = 4.2f;
        [SerializeField] private float fuerzaElasticidad = 24f;
        [SerializeField] private float amortiguacion = 3.5f;
        [SerializeField] private bool limitarDistanciaConJoint = true;

        [Header("Visual")]
        [SerializeField] private float grosor = 0.09f;
        [SerializeField] private Color colorCuerda = new Color(0.95f, 0.76f, 0.35f);
        [SerializeField] private int segmentosVisuales = 18;
        [SerializeField] private float caidaVisual = 0.25f;

        private LineRenderer linea;
        private DistanceJoint2D joint;

        public float LongitudMaxima => longitudMaxima;
        public float DistanciaActual { get; private set; }
        public float Tension01 => Mathf.Clamp01(DistanciaActual / Mathf.Max(0.01f, longitudMaxima));

        private void Awake()
        {
            linea = GetComponent<LineRenderer>();
            ConfigurarLinea();
        }

        private void Start()
        {
            if (limitarDistanciaConJoint && jugadorUno != null && jugadorDos != null)
            {
                joint = jugadorUno.gameObject.AddComponent<DistanceJoint2D>();
                joint.connectedBody = jugadorDos.Cuerpo;
                joint.autoConfigureDistance = false;
                joint.distance = longitudMaxima;
                joint.maxDistanceOnly = true;
                joint.enableCollision = true;
            }
        }

        private void FixedUpdate()
        {
            if (jugadorUno == null || jugadorDos == null) return;

            Vector2 desdeUno = jugadorUno.transform.position;
            Vector2 desdeDos = jugadorDos.transform.position;
            Vector2 diferencia = desdeDos - desdeUno;
            DistanciaActual = diferencia.magnitude;

            if (DistanciaActual <= longitudMaxima || DistanciaActual <= 0.01f)
            {
                return;
            }

            Vector2 direccion = diferencia.normalized;
            float exceso = DistanciaActual - longitudMaxima;
            Vector2 velocidadRelativa = jugadorDos.Cuerpo.linearVelocity - jugadorUno.Cuerpo.linearVelocity;
            float velocidadSeparacion = Vector2.Dot(velocidadRelativa, direccion);
            float fuerza = exceso * fuerzaElasticidad + velocidadSeparacion * amortiguacion;

            jugadorUno.Cuerpo.AddForce(direccion * fuerza, ForceMode2D.Force);
            jugadorDos.Cuerpo.AddForce(-direccion * fuerza, ForceMode2D.Force);
        }

        private void LateUpdate()
        {
            DibujarCuerda();
        }

        private void ConfigurarLinea()
        {
            linea.useWorldSpace = true;
            linea.positionCount = segmentosVisuales;
            linea.startWidth = grosor;
            linea.endWidth = grosor;
            linea.startColor = colorCuerda;
            linea.endColor = colorCuerda;
            linea.sortingOrder = 5;
        }

        private void DibujarCuerda()
        {
            if (jugadorUno == null || jugadorDos == null) return;

            Vector3 inicio = jugadorUno.transform.position + Vector3.up * 0.1f;
            Vector3 fin = jugadorDos.transform.position + Vector3.up * 0.1f;

            linea.positionCount = Mathf.Max(2, segmentosVisuales);
            for (int i = 0; i < linea.positionCount; i++)
            {
                float t = i / (float)(linea.positionCount - 1);
                Vector3 punto = Vector3.Lerp(inicio, fin, t);
                float curva = Mathf.Sin(t * Mathf.PI) * caidaVisual;
            linea.SetPosition(i, punto + Vector3.down * curva);
            }

            float tension = Tension01;
            Color colorActual = Color.Lerp(colorCuerda, new Color(1f, 0.24f, 0.12f), Mathf.InverseLerp(0.82f, 1f, tension));
            linea.startColor = colorActual;
            linea.endColor = colorActual;
            float grosorActual = Mathf.Lerp(grosor, grosor * 1.75f, Mathf.InverseLerp(0.88f, 1f, tension));
            linea.startWidth = grosorActual;
            linea.endWidth = grosorActual;
        }
    }
}
