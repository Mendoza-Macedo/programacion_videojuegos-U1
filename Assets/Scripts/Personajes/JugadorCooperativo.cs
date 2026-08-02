using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using JuegoCooperativo.Animales;
using JuegoCooperativo.Campania;

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
        private AnimadorAnimalSierra animadorAnimal;
        private FixedJoint2D agarreActual;
        private float direccionHorizontal;
        private bool pidioSalto;
        private bool estaAgarrado;
        private bool estabaEnSuelo;
        private float relojCoyote;
        private float relojBufferSalto;
        private float velocidadBase;
        private float fuerzaSaltoBase;
        private float aceleracionAereaBase;
        private float fuerzaArrastreBase;
        private float amortiguacionSueloBase;
        private bool habilidadPotenciada;
        private Color colorSpriteBase;
        private readonly Color colorPotenciado = new Color(1f, 0.78f, 0.12f, 1f);

        public string NombreJugador => nombreJugador;
        public bool EstaEnSuelo { get; private set; }
        public bool EstaAgarrado => estaAgarrado;
        public Rigidbody2D Cuerpo => cuerpo;



        public TMP_Text TextCoins;
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Coin"))
            {
                collision.gameObject.SetActive(false);
                int monedas = EstadoCampania.RegistrarMoneda();
                if (TextCoins != null) TextCoins.text = monedas.ToString();
                Destroy(collision.gameObject);
            }
        }
        private void Awake()
        {
            cuerpo = GetComponent<Rigidbody2D>();
            colision = GetComponent<Collider2D>();
            sprite = GetComponent<SpriteRenderer>();
            animadorAnimal = GetComponent<AnimadorAnimalSierra>();
            velocidadBase = velocidad;
            fuerzaSaltoBase = fuerzaSalto;
            aceleracionAereaBase = aceleracionAerea;
            fuerzaArrastreBase = fuerzaArrastreCompanero;
            amortiguacionSueloBase = amortiguacionSuelo;
            if (TextCoins != null) TextCoins.text = EstadoCampania.Monedas.ToString();

            if (sprite != null)
            {
                sprite.color = colorJugador;
                colorSpriteBase = sprite.color;
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
            int indiceMando = nombreJugador.ToLowerInvariant().Contains("azul") ? 0 : 1;
            Gamepad mando = Gamepad.all.Count > indiceMando ? Gamepad.all[indiceMando] : null;

            direccionHorizontal = 0f;
            if (teclado != null && teclado[teclaIzquierda].isPressed) direccionHorizontal -= 1f;
            if (teclado != null && teclado[teclaDerecha].isPressed) direccionHorizontal += 1f;
            if (mando != null)
            {
                float entradaMando = mando.leftStick.x.ReadValue();
                if (Mathf.Abs(entradaMando) < 0.15f) entradaMando = mando.dpad.x.ReadValue();
                if (Mathf.Abs(entradaMando) > Mathf.Abs(direccionHorizontal)) direccionHorizontal = entradaMando;
            }

            if ((teclado != null && teclado[teclaSalto].wasPressedThisFrame) ||
                (mando != null && mando.buttonSouth.wasPressedThisFrame))
            {
                relojBufferSalto = tiempoBufferSalto;
            }

            if ((teclado != null && teclado[teclaAgarrar].wasPressedThisFrame) ||
                (mando != null && mando.rightShoulder.wasPressedThisFrame))
            {
                CambiarAgarre();
            }

            if (sprite != null && Mathf.Abs(direccionHorizontal) > 0.01f)
            {
                sprite.flipX = direccionHorizontal < 0f;
            }

            ActualizarColorPotenciado();
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
            float factorControl = EstaEnSuelo ? 1f : ObtenerAceleracionAereaActual();
            float velocidadObjetivo = direccionHorizontal * ObtenerVelocidadActual();
            float nuevaVelocidadX = Mathf.Lerp(cuerpo.linearVelocity.x, velocidadObjetivo, factorControl);
            if (EstaEnSuelo && Mathf.Abs(direccionHorizontal) < 0.01f)
            {
                nuevaVelocidadX = Mathf.Lerp(cuerpo.linearVelocity.x, 0f, ObtenerAmortiguacionSueloActual());
            }
            cuerpo.linearVelocity = new Vector2(nuevaVelocidadX, cuerpo.linearVelocity.y);
        }

        private void Saltar()
        {
            cuerpo.linearVelocity = new Vector2(cuerpo.linearVelocity.x, 0f);
            cuerpo.AddForce(Vector2.up * ObtenerFuerzaSaltoActual(), ForceMode2D.Impulse);
            JuegoCooperativo.Audio.SonidosJuego.Instancia?.ReproducirSalto(animadorAnimal?.NombreAnimalActual);
        }

        public void EmpujarHacia(Vector2 direccion)
        {
            cuerpo.AddForce(direccion.normalized * ObtenerFuerzaArrastreActual(), ForceMode2D.Force);
        }

        public void ActivarPotenciadorHabilidad()
        {
            habilidadPotenciada = true;
            if (sprite != null)
            {
                colorSpriteBase = colorJugador.a > 0f ? colorJugador : sprite.color;
            }
        }

        public void DesactivarPotenciadorHabilidad()
        {
            habilidadPotenciada = false;
            if (sprite != null)
            {
                sprite.color = colorSpriteBase;
            }
        }

        private float ObtenerVelocidadActual()
        {
            float multiplicador = habilidadPotenciada ? 1.22f : 1f;
            if (AnimalActualContiene("alpaca")) return velocidadBase * 1.22f * multiplicador;
            if (AnimalActualContiene("vicuna") || AnimalActualContiene("vicu")) return velocidadBase * 1.12f * multiplicador;
            return velocidadBase * multiplicador;
        }

        private float ObtenerFuerzaSaltoActual()
        {
            float multiplicador = habilidadPotenciada ? 1.18f : 1f;
            if (AnimalActualContiene("condor")) return fuerzaSaltoBase * 1.14f * multiplicador;
            return fuerzaSaltoBase * multiplicador;
        }

        private float ObtenerAceleracionAereaActual()
        {
            float multiplicador = habilidadPotenciada ? 1.12f : 1f;
            if (AnimalActualContiene("vicuna") || AnimalActualContiene("vicu")) return aceleracionAereaBase * 1.22f * multiplicador;
            return aceleracionAereaBase * multiplicador;
        }

        private float ObtenerFuerzaArrastreActual()
        {
            float multiplicador = habilidadPotenciada ? 1.35f : 1f;
            if (AnimalActualContiene("llama")) return fuerzaArrastreBase * 1.45f * multiplicador;
            return fuerzaArrastreBase * multiplicador;
        }

        private float ObtenerAmortiguacionSueloActual()
        {
            float multiplicador = habilidadPotenciada ? 1.35f : 1f;
            if (AnimalActualContiene("vizcacha")) return amortiguacionSueloBase * 1.9f * multiplicador;
            return amortiguacionSueloBase * multiplicador;
        }

        private bool AnimalActualContiene(string texto)
        {
            string nombre = animadorAnimal != null ? animadorAnimal.NombreAnimalActual : string.Empty;
            return nombre.ToLowerInvariant().Contains(texto);
        }

        public void Teletransportar(Vector3 posicion)
        {
            transform.position = posicion;
            cuerpo.linearVelocity = Vector2.zero;
            cuerpo.angularVelocity = 0f;
            SoltarAgarre();
        }

        private void ActualizarColorPotenciado()
        {
            if (sprite == null || !habilidadPotenciada) return;

            float t = Mathf.PingPong(Time.time * 9f, 1f);
            sprite.color = Color.Lerp(colorSpriteBase, colorPotenciado, t);
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
