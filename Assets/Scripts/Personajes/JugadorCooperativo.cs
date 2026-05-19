using UnityEngine;
using UnityEngine.InputSystem;

namespace JuegoCooperativo.Personajes
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class JugadorCooperativo : MonoBehaviour
    {
        [Header("Identidad")]
        [SerializeField] private string nombreJugador = "Jugador";
        [SerializeField] private Color colorJugador = Color.white;

        [Header("Movimiento")]
        [SerializeField] private float velocidad = 7f;
        [SerializeField] private float fuerzaSalto = 13f;
        [SerializeField] private float aceleracionAerea = 0.72f;
        [SerializeField] private float fuerzaArrastreCompanero = 16f;
        [SerializeField] private float tiempoCoyote = 0.12f;
        [SerializeField] private float tiempoBufferSalto = 0.12f;
        [SerializeField] private float amortiguacionSuelo = 0.18f;

        [Header("Teclas")]
        [SerializeField] private Key teclaIzquierda = Key.A;
        [SerializeField] private Key teclaDerecha = Key.D;
        [SerializeField] private Key teclaSalto = Key.W;
        [SerializeField] private Key teclaAgarrar = Key.LeftShift;

        [Header("Suelo")]
        [SerializeField] private Transform detectorSuelo;
        [SerializeField] private float radioDetectorSuelo = 0.16f;
        [SerializeField] private LayerMask capaSuelo;

        private Rigidbody2D cuerpo;
        private Collider2D colision;
        private SpriteRenderer sprite;
        private FixedJoint2D agarreActual;
        private float direccionHorizontal;
        private bool pidioSalto;
        private bool estaAgarrado;
        private bool estabaEnSuelo;
        private float relojCoyote;
        private float relojBufferSalto;

        public string NombreJugador => nombreJugador;
        public bool EstaEnSuelo { get; private set; }
        public bool EstaAgarrado => estaAgarrado;
        public Rigidbody2D Cuerpo => cuerpo;

        private void Awake()
        {
            cuerpo = GetComponent<Rigidbody2D>();
            colision = GetComponent<Collider2D>();
            sprite = GetComponent<SpriteRenderer>();

            if (sprite != null)
            {
                sprite.color = colorJugador;
            }

            if (detectorSuelo == null)
            {
                GameObject punto = new GameObject("DetectorSuelo");
                punto.transform.SetParent(transform);
                punto.transform.localPosition = new Vector3(0f, -0.56f, 0f);
                detectorSuelo = punto.transform;
            }
        }

        private void Update()
        {
            Keyboard teclado = Keyboard.current;
            if (teclado == null)
            {
                return;
            }

            direccionHorizontal = 0f;
            if (teclado[teclaIzquierda].isPressed) direccionHorizontal -= 1f;
            if (teclado[teclaDerecha].isPressed) direccionHorizontal += 1f;

            if (teclado[teclaSalto].wasPressedThisFrame)
            {
                relojBufferSalto = tiempoBufferSalto;
            }

            if (teclado[teclaAgarrar].wasPressedThisFrame)
            {
                CambiarAgarre();
            }

            if (sprite != null && Mathf.Abs(direccionHorizontal) > 0.01f)
            {
                sprite.flipX = direccionHorizontal < 0f;
            }
        }

        private void FixedUpdate()
        {
            RevisarSuelo();
            relojBufferSalto -= Time.fixedDeltaTime;
            Mover();

            if (relojBufferSalto > 0f && relojCoyote > 0f)
            {
                Saltar();
                relojBufferSalto = 0f;
                relojCoyote = 0f;
            }
        }

        private void RevisarSuelo()
        {
            estabaEnSuelo = EstaEnSuelo;
            EstaEnSuelo = Physics2D.OverlapCircle(detectorSuelo.position, radioDetectorSuelo, capaSuelo);
            relojCoyote = EstaEnSuelo ? tiempoCoyote : relojCoyote - Time.fixedDeltaTime;

            if (!estabaEnSuelo && EstaEnSuelo)
            {
                cuerpo.linearVelocity = new Vector2(cuerpo.linearVelocity.x, Mathf.Max(cuerpo.linearVelocity.y, -1f));
                JuegoCooperativo.Audio.SonidosJuego.Instancia?.ReproducirAterrizaje();
            }
        }

        private void Mover()
        {
            float factorControl = EstaEnSuelo ? 1f : aceleracionAerea;
            float velocidadObjetivo = direccionHorizontal * velocidad;
            float nuevaVelocidadX = Mathf.Lerp(cuerpo.linearVelocity.x, velocidadObjetivo, factorControl);
            if (EstaEnSuelo && Mathf.Abs(direccionHorizontal) < 0.01f)
            {
                nuevaVelocidadX = Mathf.Lerp(cuerpo.linearVelocity.x, 0f, amortiguacionSuelo);
            }
            cuerpo.linearVelocity = new Vector2(nuevaVelocidadX, cuerpo.linearVelocity.y);
        }

        private void Saltar()
        {
            cuerpo.linearVelocity = new Vector2(cuerpo.linearVelocity.x, 0f);
            cuerpo.AddForce(Vector2.up * fuerzaSalto, ForceMode2D.Impulse);
            JuegoCooperativo.Audio.SonidosJuego.Instancia?.ReproducirSalto();
        }

        public void EmpujarHacia(Vector2 direccion)
        {
            cuerpo.AddForce(direccion.normalized * fuerzaArrastreCompanero, ForceMode2D.Force);
        }

        public void Teletransportar(Vector3 posicion)
        {
            transform.position = posicion;
            cuerpo.linearVelocity = Vector2.zero;
            cuerpo.angularVelocity = 0f;
            SoltarAgarre();
        }

        private void CambiarAgarre()
        {
            if (estaAgarrado)
            {
                SoltarAgarre();
            }
            else
            {
                IntentarAgarrar();
            }
        }

        private void IntentarAgarrar()
        {
            RaycastHit2D golpe = Physics2D.BoxCast(colision.bounds.center, colision.bounds.size * 0.9f, 0f, Vector2.zero, 0f, capaSuelo);
            if (!golpe.collider)
            {
                return;
            }

            Rigidbody2D cuerpoObjetivo = golpe.collider.attachedRigidbody;
            agarreActual = gameObject.AddComponent<FixedJoint2D>();
            agarreActual.autoConfigureConnectedAnchor = true;
            agarreActual.enableCollision = false;
            agarreActual.connectedBody = cuerpoObjetivo;
            estaAgarrado = true;
            JuegoCooperativo.Audio.SonidosJuego.Instancia?.ReproducirAgarre();
        }

        private void SoltarAgarre()
        {
            if (agarreActual != null)
            {
                Destroy(agarreActual);
            }

            agarreActual = null;
            estaAgarrado = false;
        }

        private void OnDrawGizmosSelected()
        {
            if (detectorSuelo == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(detectorSuelo.position, radioDetectorSuelo);
        }
    }
}
