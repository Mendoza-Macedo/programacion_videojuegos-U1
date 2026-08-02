using UnityEngine;

namespace JuegoCooperativo.Campania
{
    [RequireComponent(typeof(LineRenderer))]
    public class VisualPortal : MonoBehaviour
    {
        [SerializeField] private Color color = Color.cyan;
        [SerializeField] private Vector2 radio = new Vector2(0.75f, 1.15f);
        [SerializeField] private float velocidad = 70f;

        private LineRenderer linea;
        private float fase;

        public void Configurar(Color nuevoColor)
        {
            color = nuevoColor;
            if (linea != null) AplicarVisual();
        }

        private void Awake()
        {
            linea = GetComponent<LineRenderer>();
            AplicarVisual();
            CrearParticulas();
        }

        private void Update()
        {
            fase += velocidad * Time.unscaledDeltaTime;
            DibujarElipse(fase);
            float pulso = 1f + Mathf.Sin(Time.unscaledTime * 4f) * 0.06f;
            transform.localScale = new Vector3(pulso, pulso, 1f);
        }

        private void AplicarVisual()
        {
            linea.useWorldSpace = false;
            linea.loop = true;
            linea.positionCount = 48;
            linea.startWidth = 0.15f;
            linea.endWidth = 0.15f;
            linea.startColor = color;
            linea.endColor = new Color(color.r, color.g, color.b, 0.55f);
            linea.sortingOrder = 80;
            DibujarElipse(0f);
        }

        private void DibujarElipse(float rotacion)
        {
            if (linea == null) return;
            float giro = rotacion * Mathf.Deg2Rad;
            for (int i = 0; i < linea.positionCount; i++)
            {
                float angulo = i / (float)linea.positionCount * Mathf.PI * 2f + giro;
                linea.SetPosition(i, new Vector3(Mathf.Cos(angulo) * radio.x, Mathf.Sin(angulo) * radio.y, 0f));
            }
        }

        private void CrearParticulas()
        {
            ParticleSystem existente = GetComponentInChildren<ParticleSystem>();
            if (existente != null) return;

            GameObject objeto = new GameObject("Particulas Portal");
            objeto.transform.SetParent(transform, false);
            ParticleSystem particulas = objeto.AddComponent<ParticleSystem>();
            var main = particulas.main;
            main.loop = true;
            main.startLifetime = 0.7f;
            main.startSpeed = 0.35f;
            main.startSize = 0.09f;
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            var emission = particulas.emission;
            emission.rateOverTime = 18f;
            var shape = particulas.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.75f;
            var renderer = particulas.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 79;
        }
    }
}
