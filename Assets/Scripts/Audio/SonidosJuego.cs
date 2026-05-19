using UnityEngine;

namespace JuegoCooperativo.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class SonidosJuego : MonoBehaviour
    {
        private AudioSource fuente;
        private AudioClip salto;
        private AudioClip agarre;
        private AudioClip aterrizaje;
        private AudioClip checkpoint;
        private AudioClip dano;
        private AudioClip victoria;

        public static SonidosJuego Instancia { get; private set; }

        private void Awake()
        {
            Instancia = this;
            fuente = GetComponent<AudioSource>();
            fuente.playOnAwake = false;
            salto = CrearTono("Salto", 520f, 0.08f, 0.18f);
            agarre = CrearTono("Agarre", 320f, 0.06f, 0.15f);
            aterrizaje = CrearRuido("Aterrizaje", 0.07f, 0.10f);
            checkpoint = CrearTono("Checkpoint", 720f, 0.16f, 0.20f);
            dano = CrearTono("Dano", 140f, 0.18f, 0.24f);
            victoria = CrearTono("Victoria", 880f, 0.28f, 0.22f);
        }

        public void ReproducirSalto() => Reproducir(salto, 0.8f);
        public void ReproducirAgarre() => Reproducir(agarre, 0.75f);
        public void ReproducirAterrizaje() => Reproducir(aterrizaje, 0.55f);
        public void ReproducirCheckpoint() => Reproducir(checkpoint, 0.9f);
        public void ReproducirDano() => Reproducir(dano, 0.9f);
        public void ReproducirVictoria() => Reproducir(victoria, 1f);

        private void Reproducir(AudioClip clip, float volumen)
        {
            if (clip != null) fuente.PlayOneShot(clip, volumen);
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
