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

        public int CantidadAnimales => animalesDisponibles == null ? 0 : animalesDisponibles.Length;
        public int IndiceAnimalActual => indiceAnimalActual;
        public string NombreAnimalActual => ObtenerAnimalActual()?.nombreAnimal ?? "Sin animal";
        public Sprite SpriteVistaPrevia => ObtenerAnimalActual()?.quieto;

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
            render.sprite = ObtenerAnimalActual().quieto;
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

            if (jugador.EstaAgarrado)
            {
                render.sprite = animal.agarrarse;
                return;
            }

            if (!jugador.EstaEnSuelo && velocidad.y > 0.15f)
            {
                render.sprite = animal.saltar;
                return;
            }

            if (!jugador.EstaEnSuelo && velocidad.y < -0.15f)
            {
                render.sprite = animal.caer;
                return;
            }

            if (Mathf.Abs(velocidad.x) > 0.18f)
            {
                relojAnimacion += Time.deltaTime * velocidadAnimacionCaminar;
                cuadroCaminar = Mathf.FloorToInt(relojAnimacion) % 2;
                render.sprite = cuadroCaminar == 0 ? animal.caminarUno : animal.caminarDos;
                return;
            }

            relojAnimacion = 0f;
            render.sprite = animal.quieto;
        }

        private AnimalSierra ObtenerAnimalActual()
        {
            if (animalesDisponibles == null || animalesDisponibles.Length == 0) return null;
            indiceAnimalActual = Mathf.Clamp(indiceAnimalActual, 0, animalesDisponibles.Length - 1);
            return animalesDisponibles[indiceAnimalActual];
        }
    }
}
