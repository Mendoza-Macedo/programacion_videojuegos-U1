using UnityEngine;

namespace JuegoCooperativo.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class SonidosJuego : MonoBehaviour
    {
        [Header("Musica")]
        [SerializeField] private AudioClip musicaPrincipal;
        [SerializeField] private AudioClip musicaBosque;
        [SerializeField] private AudioClip musicaHielo;
        [SerializeField, Range(0f, 1f)] private float volumenMusica = 0.35f;
        [SerializeField, Range(0f, 1f)] private float volumenEfectos = 1f;

        [Header("Saltos")]
        [SerializeField] private AudioClip saltoGeneral;
        [SerializeField] private AudioClip saltoAlpaca;
        [SerializeField] private AudioClip saltoCondor;
        [SerializeField] private AudioClip saltoVicuna;
        [SerializeField] private AudioClip saltoVizcacha;

        [Header("Objetos y estado")]
        [SerializeField] private AudioClip agarre;
        [SerializeField] private AudioClip aterrizaje;
        [SerializeField] private AudioClip checkpoint;
        [SerializeField] private AudioClip dano;
        [SerializeField] private AudioClip victoria;
        [SerializeField] private AudioClip reinicio;

        private AudioSource fuente;
        private AudioSource fuenteMusica;

        public static SonidosJuego Instancia { get; private set; }

        private void Awake()
        {
            Instancia = this;
            AudioSource[] fuentes = GetComponents<AudioSource>();
            fuente = fuentes.Length > 0 ? fuentes[0] : gameObject.AddComponent<AudioSource>();
            fuente.playOnAwake = false;
            fuente.loop = false;
            fuente.volume = volumenEfectos;
            fuente.mute = false;
            fuente.spatialBlend = 0f;
            fuente.ignoreListenerPause = true;
            fuente.priority = 64;

            fuenteMusica = fuentes.Length > 1 ? fuentes[1] : gameObject.AddComponent<AudioSource>();
            fuenteMusica.clip = musicaBosque != null ? musicaBosque : musicaPrincipal;
            fuenteMusica.playOnAwake = true;
            fuenteMusica.loop = true;
            fuenteMusica.volume = volumenMusica;
            fuenteMusica.mute = false;
            fuenteMusica.spatialBlend = 0f;
            fuenteMusica.ignoreListenerPause = true;
            fuenteMusica.priority = 128;

            if (saltoGeneral == null) saltoGeneral = CrearTono("Salto", 520f, 0.08f, 0.18f);
            if (agarre == null) agarre = CrearTono("Agarre", 320f, 0.06f, 0.15f);
            if (aterrizaje == null) aterrizaje = CrearRuido("Aterrizaje", 0.07f, 0.10f);
            if (checkpoint == null) checkpoint = CrearTono("Checkpoint", 720f, 0.16f, 0.20f);
            if (dano == null) dano = CrearTono("Dano", 140f, 0.18f, 0.24f);
            if (victoria == null) victoria = CrearTono("Victoria", 880f, 0.28f, 0.22f);
        }

        private void Start()
        {
            ReproducirMusicaBosque();
        }

        public void ReproducirSalto() => Reproducir(saltoGeneral, 0.8f);
        public void ReproducirSalto(string nombreAnimal) => Reproducir(ClipSalto(nombreAnimal), 0.85f * VolumenSalto(nombreAnimal));
        public void ReproducirAgarre() => Reproducir(agarre, 0.75f);
        public void ReproducirAterrizaje() => Reproducir(aterrizaje, 0.55f);
        public void ReproducirCheckpoint() => Reproducir(checkpoint, 0.9f);
        public void ReproducirDano() => Reproducir(dano, 0.9f);
        public void ReproducirVictoria() => Reproducir(victoria, 1f);
        public void ReproducirReinicio() => Reproducir(reinicio, 0.9f);

        public void ReproducirMusicaPrincipal() => ReproducirMusica(musicaPrincipal);
        public void ReproducirMusicaBosque() => ReproducirMusica(musicaBosque != null ? musicaBosque : musicaPrincipal);
        public void ReproducirMusicaHielo() => ReproducirMusica(musicaHielo != null ? musicaHielo : musicaPrincipal);

        private void Reproducir(AudioClip clip, float volumen)
        {
            if (clip == null || fuente == null) return;
            fuente.PlayOneShot(clip, volumen * volumenEfectos);
        }

        private AudioClip ClipSalto(string nombreAnimal)
        {
            string nombre = (nombreAnimal ?? string.Empty).ToLowerInvariant();
            if (nombre.Contains("alpaca") && saltoAlpaca != null) return saltoAlpaca;
            if (nombre.Contains("condor") && saltoCondor != null) return saltoCondor;
            if ((nombre.Contains("vicuna") || nombre.Contains("vicu")) && saltoVicuna != null) return saltoVicuna;
            if (nombre.Contains("vizcacha") && saltoVizcacha != null) return saltoVizcacha;
            return saltoGeneral;
        }

        private float VolumenSalto(string nombreAnimal)
        {
            string nombre = (nombreAnimal ?? string.Empty).ToLowerInvariant();
            if (nombre.Contains("condor") || nombre.Contains("vizcacha") || nombre.Contains("llama")) return 0.5f;
            return 1f;
        }

        private void ReproducirMusica(AudioClip clip)
        {
            if (clip == null || fuenteMusica == null) return;
            if (fuenteMusica.clip == clip && fuenteMusica.isPlaying) return;
            fuenteMusica.clip = clip;
            fuenteMusica.volume = volumenMusica;
            fuenteMusica.Play();
        }

        private AudioClip CrearTono(string nombre, float frecuencia, float duracion, float volumen)
        {
            int frecuenciaMuestreo = 44100;
            int muestras = Mathf.CeilToInt(frecuenciaMuestreo * duracion);
            float[] datos = new float[muestras];
            for (int i = 0; i < muestras; i++)
            {
                float t = i / (float)frecuenciaMuestreo;
                float envolvente = 1f - i / (float)muestras;
                datos[i] = Mathf.Sin(2f * Mathf.PI * frecuencia * t) * volumen * envolvente;
            }

            AudioClip clip = AudioClip.Create(nombre, muestras, 1, frecuenciaMuestreo, false);
            clip.SetData(datos, 0);
            return clip;
        }

        private AudioClip CrearRuido(string nombre, float duracion, float volumen)
        {
            int frecuenciaMuestreo = 44100;
            int muestras = Mathf.CeilToInt(frecuenciaMuestreo * duracion);
            float[] datos = new float[muestras];
            for (int i = 0; i < muestras; i++)
            {
                float envolvente = 1f - i / (float)muestras;
                datos[i] = Random.Range(-1f, 1f) * volumen * envolvente;
            }

            AudioClip clip = AudioClip.Create(nombre, muestras, 1, frecuenciaMuestreo, false);
            clip.SetData(datos, 0);
            return clip;
        }
    }
}
