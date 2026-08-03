using JuegoCooperativo.Juego;
using JuegoCooperativo.Personajes;
using UnityEngine;

namespace JuegoCooperativo.Mundo
{
    public class EnemigoPatrulla : MonoBehaviour
    {
        [SerializeField] private float distanciaPatrulla = 3f;
        [SerializeField] private float velocidad = 1.8f;
        [SerializeField] private Vector2 tamanoCollider = new Vector2(0.75f, 1.35f);
        [SerializeField] private Vector2 offsetCollider = new Vector2(0f, 0.55f);
        [SerializeField] private float tiempoEntreGolpes = 0.8f;
        [SerializeField] private bool empujablePorEspalda = true;
        [SerializeField] private float velocidadEmpuje = 2.7f;
        [SerializeField] private float velocidadDisparoEmpuje = 9f;
        [SerializeField] private float duracionDisparoEmpuje = 0.45f;
        [SerializeField] private float tiempoEntreEmpujes = 0.35f;
        [SerializeField] private bool bloquearPasoFisico = true;
        [SerializeField] private bool caerSinSuelo = true;
        [SerializeField] private LayerMask capaSuelo = ~0;
        [SerializeField] private float distanciaDetectorSuelo = 0.35f;
        [SerializeField] private float gravedadAlCaer = 3.2f;

        private float limiteIzquierdo;
        private float limiteDerecho;
        private int direccion = 1;
        private float proximoGolpe;
        private float proximoEmpuje;
        private float tiempoDisparoRestante;
        private float velocidadDisparoActual;
        private Vector3 escalaInicial;
        private Collider2D colisionPrincipal;
        private Rigidbody2D cuerpo;
        private bool cayendo;

        private void Awake()
        {
            escalaInicial = transform.localScale;
            ConfigurarFisica();

            float mitadDistancia = Mathf.Abs(distanciaPatrulla) * 0.5f;
            limiteIzquierdo = transform.position.x - mitadDistancia;
            limiteDerecho = transform.position.x + mitadDistancia;
        }

        private void Update()
        {
            if (cayendo) return;

            if (DebeCaer())
            {
                Caer();
                return;
            }

            if (tiempoDisparoRestante > 0f)
            {
                MoverDisparado();
                return;
            }

            Patrullar();
        }

        private void MoverDisparado()
        {
            Vector3 posicion = transform.position;
            posicion.x += velocidadDisparoActual * Time.deltaTime;
            transform.position = posicion;

            tiempoDisparoRestante -= Time.deltaTime;
            if (tiempoDisparoRestante <= 0f)
            {
                RecentrarPatrulla();
            }
        }

        private void Patrullar()
        {
            Vector3 posicion = transform.position;
            posicion.x += direccion * velocidad * Time.deltaTime;

            if (posicion.x >= limiteDerecho)
            {
                posicion.x = limiteDerecho;
                direccion = -1;
            }
            else if (posicion.x <= limiteIzquierdo)
            {
                posicion.x = limiteIzquierdo;
                direccion = 1;
            }

            transform.position = posicion;
            transform.localScale = new Vector3(Mathf.Abs(escalaInicial.x) * direccion, escalaInicial.y, escalaInicial.z);
        }

        private void ConfigurarFisica()
        {
            cuerpo = GetComponent<Rigidbody2D>();
            if (cuerpo == null) cuerpo = gameObject.AddComponent<Rigidbody2D>();
            cuerpo.bodyType = RigidbodyType2D.Kinematic;
            cuerpo.gravityScale = 0f;
            cuerpo.freezeRotation = true;
            cuerpo.simulated = true;

            colisionPrincipal = GetComponent<Collider2D>();
            if (colisionPrincipal == null)
            {
                CapsuleCollider2D capsula = gameObject.AddComponent<CapsuleCollider2D>();
                capsula.size = tamanoCollider;
                capsula.offset = offsetCollider;
                capsula.direction = CapsuleDirection2D.Vertical;
                capsula.isTrigger = !bloquearPasoFisico;
                colisionPrincipal = capsula;
            }
            else
            {
                colisionPrincipal.isTrigger = !bloquearPasoFisico;
            }
        }

        private bool DebeCaer()
        {
            if (!caerSinSuelo || colisionPrincipal == null) return false;

            Vector2 origen = new Vector2(colisionPrincipal.bounds.center.x, colisionPrincipal.bounds.min.y + 0.03f);
            RaycastHit2D[] golpes = Physics2D.RaycastAll(origen, Vector2.down, distanciaDetectorSuelo, capaSuelo);
            foreach (RaycastHit2D golpe in golpes)
            {
                if (golpe.collider == null) continue;
                if (golpe.collider == colisionPrincipal) continue;
                if (golpe.collider.GetComponentInParent<EnemigoPatrulla>() == this) continue;
                if (golpe.collider.isTrigger) continue;
                return false;
            }

            return true;
        }

        private void Caer()
        {
            cayendo = true;
            tiempoDisparoRestante = 0f;

            if (cuerpo == null) return;
            cuerpo.bodyType = RigidbodyType2D.Dynamic;
            cuerpo.gravityScale = gravedadAlCaer;
            cuerpo.freezeRotation = true;
            cuerpo.linearVelocity = new Vector2(velocidadDisparoActual, cuerpo.linearVelocity.y);
        }

        private void OnTriggerEnter2D(Collider2D otro)
        {
            IntentarDaniarJugador(otro);
        }

        private void OnTriggerStay2D(Collider2D otro)
        {
            IntentarEmpujarPorEspalda(otro);
        }

        private void OnCollisionEnter2D(Collision2D colision)
        {
            IntentarDaniarJugador(colision.collider);
        }

        private void OnCollisionStay2D(Collision2D colision)
        {
            IntentarEmpujarPorEspalda(colision.collider);
        }

        private void IntentarDaniarJugador(Collider2D otro)
        {
            if (Time.time < proximoGolpe) return;

            JugadorCooperativo jugador = otro.GetComponentInParent<JugadorCooperativo>();
            if (jugador == null && !otro.CompareTag("Jugador")) return;
            if (jugador != null && EstaTocandoEspalda(jugador.transform.position)) return;

            proximoGolpe = Time.time + tiempoEntreGolpes;
            GestorJuego.Instancia?.ReaparecerJugadores();
        }

        private void IntentarEmpujarPorEspalda(Collider2D otro)
        {
            if (!empujablePorEspalda) return;
            if (Time.time < proximoEmpuje) return;

            JugadorCooperativo jugador = otro.GetComponentInParent<JugadorCooperativo>();
            if (jugador == null || !EstaTocandoEspalda(jugador.transform.position)) return;

            proximoEmpuje = Time.time + tiempoEntreEmpujes;
            velocidadDisparoActual = direccion * Mathf.Max(velocidadEmpuje, velocidadDisparoEmpuje);
            tiempoDisparoRestante = duracionDisparoEmpuje;
        }

        private void RecentrarPatrulla()
        {
            float mitadDistancia = Mathf.Abs(distanciaPatrulla) * 0.5f;
            limiteIzquierdo = transform.position.x - mitadDistancia;
            limiteDerecho = transform.position.x + mitadDistancia;
        }

        private bool EstaTocandoEspalda(Vector3 posicionJugador)
        {
            float ladoJugador = Mathf.Sign(posicionJugador.x - transform.position.x);
            return direccion > 0 ? ladoJugador < 0f : ladoJugador > 0f;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Vector3 centro = Application.isPlaying
                ? new Vector3((limiteIzquierdo + limiteDerecho) * 0.5f, transform.position.y, transform.position.z)
                : transform.position;
            Vector3 izquierda = centro + Vector3.left * Mathf.Abs(distanciaPatrulla) * 0.5f;
            Vector3 derecha = centro + Vector3.right * Mathf.Abs(distanciaPatrulla) * 0.5f;
            Gizmos.DrawLine(izquierda, derecha);
            Gizmos.DrawWireSphere(izquierda, 0.12f);
            Gizmos.DrawWireSphere(derecha, 0.12f);

            Gizmos.color = Color.yellow;
            Vector3 origen = colisionPrincipal != null
                ? new Vector3(colisionPrincipal.bounds.center.x, colisionPrincipal.bounds.min.y + 0.03f, transform.position.z)
                : transform.position + Vector3.down * 0.55f;
            Gizmos.DrawLine(origen, origen + Vector3.down * distanciaDetectorSuelo);
        }
    }
}
