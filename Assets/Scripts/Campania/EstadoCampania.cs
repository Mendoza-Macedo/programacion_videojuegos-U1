using UnityEngine;

namespace JuegoCooperativo.Campania
{
    public static class EstadoCampania
    {
        private const string ClaveNivel = "Campania_NivelDesbloqueado";
        private const string ClaveAnimalUno = "Campania_AnimalUno";
        private const string ClaveAnimalDos = "Campania_AnimalDos";
        private const string ClaveSeleccion = "Campania_SeleccionRealizada";
        private const string ClaveMonedas = "Campania_Monedas";

        public static int NivelDesbloqueado => Mathf.Clamp(PlayerPrefs.GetInt(ClaveNivel, 1), 1, 8);
        public static int AnimalUno => Mathf.Max(0, PlayerPrefs.GetInt(ClaveAnimalUno, 0));
        public static int AnimalDos => Mathf.Max(0, PlayerPrefs.GetInt(ClaveAnimalDos, 1));
        public static bool HaySeleccion => PlayerPrefs.GetInt(ClaveSeleccion, 0) == 1;
        public static int Monedas => Mathf.Max(0, PlayerPrefs.GetInt(ClaveMonedas, 0));

        public static void IniciarNuevaCampania()
        {
            PlayerPrefs.SetInt(ClaveNivel, 1);
            PlayerPrefs.SetInt(ClaveMonedas, 0);
            PlayerPrefs.SetInt(ClaveSeleccion, 0);
            PlayerPrefs.Save();
        }

        public static void GuardarSeleccion(int animalUno, int animalDos)
        {
            PlayerPrefs.SetInt(ClaveAnimalUno, Mathf.Max(0, animalUno));
            PlayerPrefs.SetInt(ClaveAnimalDos, Mathf.Max(0, animalDos));
            PlayerPrefs.SetInt(ClaveSeleccion, 1);
            PlayerPrefs.Save();
        }

        public static void DesbloquearNivel(int nivel)
        {
            if (nivel <= NivelDesbloqueado) return;
            PlayerPrefs.SetInt(ClaveNivel, Mathf.Clamp(nivel, 1, 8));
            PlayerPrefs.Save();
        }

        public static int RegistrarMoneda()
        {
            int total = Monedas + 1;
            PlayerPrefs.SetInt(ClaveMonedas, total);
            PlayerPrefs.Save();
            return total;
        }
    }
}
