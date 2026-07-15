using System;
using JuegoCooperativo.Personajes;
using UnityEngine;

namespace JuegoCooperativo.Animales
{
    [Serializable]
    public class AnimalSierra
    {
        public string nombreAnimal;
        public Sprite quieto;
        public Sprite caminarUno;
        public Sprite caminarDos;
        public Sprite saltar;
        public Sprite caer;
        public Sprite agarrarse;
    }

    [RequireComponent(typeof(SpriteRenderer))]
    public class AnimadorAnimalSierra : MonoBehaviour
    {
        [SerializeField] private JugadorCooperativo jugador;
        [SerializeField] private AnimalSierra[] animalesDisponibles;
        [SerializeField] private int indiceAnimalActual;
        [SerializeField] private float velocidadAnimacionCaminar = 8f;

        private SpriteRenderer render;
        private float relojAnimacion;
        private int cuadroCaminar;
        private Sprite ultimoSpriteValido;

        public int CantidadAnimales => animalesDisponibles == null ? 0 : animalesDisponibles.Length;
        public int IndiceAnimalActual => indiceAnimalActual;
        public string NombreAnimalActual => ObtenerAnimalActual()?.nombreAnimal ?? "Sin animal";
        public string HabilidadAnimalActual => ObtenerHabilidadAnimal(NombreAnimalActual);
        public string EtiquetaAnimalActual => $"{NombreAnimalActual} - {HabilidadAnimalActual}";
        public Sprite SpriteVistaPrevia => ObtenerAnimalActual()?.quieto;

        public static string ObtenerHabilidadAnimal(string nombreAnimal)
        {
            string nombre = (nombreAnimal ?? string.Empty).ToLowerInvariant();

            if (nombre.Contains("alpaca")) return "Rapidez";
            if (nombre.Contains("condor")) return "Salto alto";
            if (nombre.Contains("llama")) return "Empuje fuerte";
            if (nombre.Contains("vizcacha")) return "No resbala";
            if (nombre.Contains("vicuna") || nombre.Contains("vicu")) return "Agilidad";

            return "Equilibrado";
        }

        private void Awake()
        {
            render = GetComponent<SpriteRenderer>();
            if (jugador == null) jugador = GetComponent<JugadorCooperativo>();
            AplicarAnimal(indiceAnimalActual);
        }

        private void Update()
        {
            ActualizarSpritePorEstado();
        }

        public void AplicarAnimal(int nuevoIndice)
        {
            if (animalesDisponibles == null || animalesDisponibles.Length == 0) return;

            indiceAnimalActual = Mathf.Clamp(nuevoIndice, 0, animalesDisponibles.Length - 1);
            if (render == null) render = GetComponent<SpriteRenderer>();
            render.color = Color.white;
            render.sprite = ObtenerSpriteSeguro(ObtenerAnimalActual(), ObtenerAnimalActual().quieto);
            ultimoSpriteValido = render.sprite;
        }

        public void CambiarAnimal(int direccion)
        {
            if (CantidadAnimales == 0) return;
            int nuevoIndice = (indiceAnimalActual + direccion + CantidadAnimales) % CantidadAnimales;
            AplicarAnimal(nuevoIndice);
        }

        private void ActualizarSpritePorEstado()
        {
            AnimalSierra animal = ObtenerAnimalActual();
            if (animal == null || jugador == null || render == null) return;

            Vector2 velocidad = jugador.Cuerpo != null ? jugador.Cuerpo.linearVelocity : Vector2.zero;
            render.enabled = true;

            if (jugador.EstaAgarrado)
            {
                AsignarSpriteSeguro(animal, animal.agarrarse);
                return;
            }

            if (!jugador.EstaEnSuelo && EsCondor(animal))
            {
                AsignarSpriteSeguro(animal, animal.quieto);
                return;
            }

            if (!jugador.EstaEnSuelo && velocidad.y > 0.15f)
            {
                AsignarSpriteSeguro(animal, animal.saltar);
                return;
            }

            if (!jugador.EstaEnSuelo && velocidad.y < -0.15f)
            {
                AsignarSpriteSeguro(animal, animal.caer);
                return;
            }

            if (Mathf.Abs(velocidad.x) > 0.18f)
            {
                relojAnimacion += Time.deltaTime * velocidadAnimacionCaminar;
                cuadroCaminar = Mathf.FloorToInt(relojAnimacion) % 2;
                AsignarSpriteSeguro(animal, cuadroCaminar == 0 ? animal.caminarUno : animal.caminarDos);
                return;
            }

            relojAnimacion = 0f;
            AsignarSpriteSeguro(animal, animal.quieto);
        }

        private void AsignarSpriteSeguro(AnimalSierra animal, Sprite spritePreferido)
        {
            render.sprite = ObtenerSpriteSeguro(animal, spritePreferido);
            if (render.sprite != null)
            {
                ultimoSpriteValido = render.sprite;
            }
        }

        private Sprite ObtenerSpriteSeguro(AnimalSierra animal, Sprite spritePreferido)
        {
            if (spritePreferido != null && !DebeEvitarSpriteSaltoCondor(animal, spritePreferido)) return spritePreferido;
            if (animal.quieto != null) return animal.quieto;
            if (animal.caminarUno != null) return animal.caminarUno;
            if (animal.caminarDos != null) return animal.caminarDos;
            return ultimoSpriteValido;
        }

        private bool DebeEvitarSpriteSaltoCondor(AnimalSierra animal, Sprite spritePreferido)
        {
            if (animal == null || spritePreferido != animal.saltar) return false;

            return EsCondor(animal);
        }

        private bool EsCondor(AnimalSierra animal)
        {
            string nombre = (animal?.nombreAnimal ?? string.Empty).ToLowerInvariant();
            return nombre.Contains("condor");
        }

        private AnimalSierra ObtenerAnimalActual()
        {
            if (animalesDisponibles == null || animalesDisponibles.Length == 0) return null;
            indiceAnimalActual = Mathf.Clamp(indiceAnimalActual, 0, animalesDisponibles.Length - 1);
            return animalesDisponibles[indiceAnimalActual];
        }
    }
}
