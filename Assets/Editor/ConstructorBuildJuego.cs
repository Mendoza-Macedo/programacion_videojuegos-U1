using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace JuegoCooperativo.Editor
{
    public static class ConstructorBuildJuego
    {
        [MenuItem("Herramientas/BreadFred/Generar ejecutable Windows")]
        public static void GenerarEjecutableWindows()
        {
            ConstructorJuegoCompleto.Construir();

            string carpeta = "Builds/AnimalesDeLaCumbre";
            if (!Directory.Exists(carpeta)) Directory.CreateDirectory(carpeta);

            PlayerSettings.companyName = "EPIS";
            PlayerSettings.productName = "Animales de la Cumbre";
            PlayerSettings.bundleVersion = "1.0.0";

            BuildPlayerOptions opciones = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Escenas/EscenaCooperativa.unity" },
                locationPathName = Path.Combine(carpeta, "AnimalesDeLaCumbre.exe"),
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            BuildReport reporte = BuildPipeline.BuildPlayer(opciones);
            if (reporte.summary.result != BuildResult.Succeeded)
            {
                throw new System.Exception("No se pudo generar el ejecutable: " + reporte.summary.result);
            }

            Debug.Log("Ejecutable generado en " + opciones.locationPathName);
        }
    }
}
