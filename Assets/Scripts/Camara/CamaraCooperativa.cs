using UnityEngine;

namespace JuegoCooperativo.Camara
{
    public class CamaraCooperativa : MonoBehaviour
    {
        [SerializeField] private Transform jugadorUno;
        [SerializeField] private Transform jugadorDos;
        [SerializeField] private Vector3 desplazamiento = new Vector3(0f, 1.2f, -10f);
        [SerializeField] private float suavizado = 5f;
        [SerializeField] private float tamanoMinimo = 6f;
        [SerializeField] private float tamanoMaximo = 9f;
        [SerializeField] private float multiplicadorDistancia = 1.1f;
        [SerializeField] private bool usarLimites = true;
        [SerializeField] private Vector2 limiteMinimo = new Vector2(-6f, -4f);
        [SerializeField] private Vector2 limiteMaximo = new Vector2(45f, 28f);

        private Camera camara;
        private float fuerzaSacudida;
        private float tiempoSacudida;

        private void Awake()
        {
            camara = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            if (jugadorUno == null || jugadorDos == null) return;

            Vector3 puntoMedio = (jugadorUno.position + jugadorDos.position) * 0.5f;
            Vector3 objetivo = puntoMedio + desplazamiento;
            if (usarLimites)
            {
                objetivo.x = Mathf.Clamp(objetivo.x, limiteMinimo.x, limiteMaximo.x);
                objetivo.y = Mathf.Clamp(objetivo.y, limiteMinimo.y, limiteMaximo.y);
            }

            if (tiempoSacudida > 0f)
            {
                tiempoSacudida -= Time.deltaTime;
                objetivo += (Vector3)Random.insideUnitCircle * fuerzaSacudida;
                fuerzaSacudida = Mathf.Lerp(fuerzaSacudida, 0f, 8f * Time.deltaTime);
            }

            transform.position = Vector3.Lerp(transform.position, objetivo, 1f - Mathf.Exp(-suavizado * Time.deltaTime));

            float distancia = Vector2.Distance(jugadorUno.position, jugadorDos.position);
            float tamanoObjetivo = Mathf.Clamp(tamanoMinimo + distancia * multiplicadorDistancia, tamanoMinimo, tamanoMaximo);
            camara.orthographicSize = Mathf.Lerp(camara.orthographicSize, tamanoObjetivo, 1f - Mathf.Exp(-suavizado * Time.deltaTime));
        }

        public void Sacudir(float fuerza, float duracion)
        {
            fuerzaSacudida = Mathf.Max(fuerzaSacudida, fuerza);
            tiempoSacudida = Mathf.Max(tiempoSacudida, duracion);
        }
    }
}
