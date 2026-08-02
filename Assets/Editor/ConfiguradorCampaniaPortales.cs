using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JuegoCooperativo.Campania;
using JuegoCooperativo.Mundo;
using JuegoCooperativo.Personajes;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace JuegoCooperativo.Editor
{
    public static class ConfiguradorCampaniaPortales
    {
        private const string CarpetaPrefabs = "Assets/Prefabs/Campania";
        private const string CarpetaMateriales = "Assets/Materiales/Campania";
        private const string PrefabBase = CarpetaPrefabs + "/BaseNivelCooperativo.prefab";
        private const string PrefabRojo = CarpetaPrefabs + "/PortalRojo.prefab";
        private const string PrefabAzul = CarpetaPrefabs + "/PortalAzul.prefab";

        private static readonly string[] ObjetosBase =
        {
            "Jugador Azul", "Jugador Rojo", "Camara Cooperativa", "Cuerda Cooperativa",
            "Gestor del Juego", "Selector de Animales", "Flujo Juego UI", "Sonidos del Juego",
            "Interfaz", "Interfaz Seleccion Animales", "Interfaz Juego Completo", "Zona de muerte"
        };

        public static string[] EscenasCampania => Enumerable.Range(1, 8)
            .Select(i => $"Assets/Scenes/Scene{i}.unity").ToArray();

        [MenuItem("Herramientas/Animales de la Cumbre/Configurar campaña y portales")]
        public static void ConfigurarCampania()
        {
            CrearCarpetas();
            CrearPrefabBaseDesdeScene1();
            CrearPrefabsPortales();

            for (int nivel = 1; nivel <= 8; nivel++)
            {
                ConfigurarEscena(nivel);
            }

            ConfigurarBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Campaña Scene1-Scene8 configurada: base cooperativa, portales y Build Settings.");
        }

        public static void ConfigurarDesdeLineaComandos()
        {
            ConfigurarCampania();
        }

        [MenuItem("Herramientas/Animales de la Cumbre/Validar campaña")]
        public static void ValidarCampania()
        {
            List<string> errores = new List<string>();
            string[] escenasBuild = EditorBuildSettings.scenes
                .Where(escena => escena.enabled)
                .Select(escena => escena.path)
                .ToArray();
            if (!escenasBuild.SequenceEqual(EscenasCampania))
            {
                errores.Add($"Build Settings: {string.Join(", ", escenasBuild)}");
            }

            for (int nivel = 1; nivel <= 8; nivel++)
            {
                Scene escena = EditorSceneManager.OpenScene(EscenasCampania[nivel - 1], OpenSceneMode.Single);
                JugadorCooperativo[] jugadores = UnityEngine.Object.FindObjectsByType<JugadorCooperativo>(FindObjectsSortMode.None);
                PortalEntrada[] entradas = UnityEngine.Object.FindObjectsByType<PortalEntrada>(FindObjectsSortMode.None);
                PortalSalida[] salidas = UnityEngine.Object.FindObjectsByType<PortalSalida>(FindObjectsSortMode.None);
                ControlCampania[] controles = UnityEngine.Object.FindObjectsByType<ControlCampania>(FindObjectsSortMode.None);
                MetaNivel[] metasActivas = UnityEngine.Object.FindObjectsByType<MetaNivel>(FindObjectsSortMode.None);

                if (jugadores.Length != 2 || entradas.Length != 1 || salidas.Length != 1 || controles.Length != 1)
                {
                    errores.Add($"{escena.name}: jugadores={jugadores.Length}, rojos={entradas.Length}, azules={salidas.Length}, controles={controles.Length}");
                    continue;
                }

                Collider2D activadorSalida = salidas[0].GetComponent<Collider2D>();
                if (activadorSalida == null || !activadorSalida.isTrigger)
                {
                    errores.Add($"{escena.name}: el portal azul no tiene Collider2D trigger");
                }

                if (metasActivas.Any(meta => meta.enabled))
                {
                    errores.Add($"{escena.name}: queda una MetaNivel antigua activa");
                }

                float distancia = Vector2.Distance(entradas[0].transform.position, salidas[0].transform.position);
                if (distancia < 2f)
                {
                    errores.Add($"{escena.name}: portales demasiado cerca ({distancia:F2})");
                }

                Debug.Log($"[VALIDACIÓN] {escena.name}: rojo={entradas[0].transform.position}, azul={salidas[0].transform.position}, distancia={distancia:F2}");
            }

            if (errores.Count > 0) throw new Exception("Campaña inválida:\n" + string.Join("\n", errores));
            Debug.Log("Validación correcta: las 8 escenas tienen dos jugadores, ambos portales y el Build Settings ordenado.");
        }

        private static void ConfigurarEscena(int nivel)
        {
            Scene escena = EditorSceneManager.OpenScene(EscenasCampania[nivel - 1], OpenSceneMode.Single);

            if (nivel == 2 || UnityEngine.Object.FindObjectsByType<JugadorCooperativo>(FindObjectsSortMode.None).Length < 2)
            {
                ReemplazarBaseCooperativa(escena);
            }

            int layerSuelo = LayerMask.NameToLayer("Suelo");
            foreach (TilemapCollider2D collider in UnityEngine.Object.FindObjectsByType<TilemapCollider2D>(FindObjectsSortMode.None))
            {
                if (layerSuelo >= 0) collider.gameObject.layer = layerSuelo;
            }

            JugadorCooperativo[] jugadores = UnityEngine.Object.FindObjectsByType<JugadorCooperativo>(FindObjectsSortMode.None);
            if (jugadores.Length < 2) throw new Exception($"{escena.name} no pudo recibir los dos jugadores.");

            Vector3 entrada = CentroJugadores(jugadores);
            Vector3 salida = BuscarPosicionMeta();
            if (nivel == 2)
            {
                CalcularExtremosTilemap(out entrada, out salida);
                OrdenarJugadores(jugadores);
                jugadores[0].transform.position = entrada + Vector3.left * 1.15f;
                jugadores[1].transform.position = entrada + Vector3.right * 1.15f;
            }

            foreach (MetaNivel meta in UnityEngine.Object.FindObjectsByType<MetaNivel>(FindObjectsSortMode.None))
            {
                meta.enabled = false;
            }

            DestruirRaizSiExiste("Control de Campania");
            GameObject control = new GameObject("Control de Campania");
            control.AddComponent<ControlCampania>();

            CrearInstanciaPortal(PrefabRojo, "Portal Rojo - Llegada", entrada);
            CrearInstanciaPortal(PrefabAzul, "Portal Azul - Salida", salida);

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
        }

        private static void CrearPrefabBaseDesdeScene1()
        {
            Scene escena = EditorSceneManager.OpenScene(EscenasCampania[0], OpenSceneMode.Single);
            GameObject temporal = new GameObject("Base Nivel Cooperativo");
            List<Transform> movidos = new List<Transform>();

            foreach (string nombre in ObjetosBase)
            {
                GameObject objeto = BuscarRaiz(escena, nombre);
                if (objeto == null) throw new Exception($"No se encontró '{nombre}' en Scene1.");
                movidos.Add(objeto.transform);
                objeto.transform.SetParent(temporal.transform, true);
            }

            PrefabUtility.SaveAsPrefabAsset(temporal, PrefabBase);
            foreach (Transform movido in movidos) movido.SetParent(null, true);
            UnityEngine.Object.DestroyImmediate(temporal);
            EditorSceneManager.CloseScene(escena, true);
        }

        private static void ReemplazarBaseCooperativa(Scene escena)
        {
            foreach (string nombre in ObjetosBase) DestruirRaizSiExiste(nombre);

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBase);
            GameObject instancia = (GameObject)PrefabUtility.InstantiatePrefab(prefab, escena);
            PrefabUtility.UnpackPrefabInstance(instancia, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            List<Transform> hijos = new List<Transform>();
            foreach (Transform hijo in instancia.transform) hijos.Add(hijo);
            foreach (Transform hijo in hijos) hijo.SetParent(null, true);
            UnityEngine.Object.DestroyImmediate(instancia);
        }

        private static void CrearPrefabsPortales()
        {
            Material rojo = CrearMaterial(CarpetaMateriales + "/PortalRojo.mat", new Color(1f, 0.12f, 0.08f, 1f));
            Material azul = CrearMaterial(CarpetaMateriales + "/PortalAzul.mat", new Color(0.08f, 0.55f, 1f, 1f));
            CrearPrefabPortal(PrefabRojo, "PORTAL ROJO\nLLEGADA", new Color(1f, 0.12f, 0.08f), rojo, true);
            CrearPrefabPortal(PrefabAzul, "PORTAL AZUL\nSALIDA", new Color(0.08f, 0.55f, 1f), azul, false);
        }

        private static void CrearPrefabPortal(string ruta, string etiqueta, Color color, Material material, bool entrada)
        {
            GameObject portal = new GameObject(entrada ? "Portal Rojo - Llegada" : "Portal Azul - Salida");
            LineRenderer linea = portal.AddComponent<LineRenderer>();
            linea.sharedMaterial = material;
            VisualPortal visual = portal.AddComponent<VisualPortal>();
            visual.Configurar(color);
            CircleCollider2D trigger = portal.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = 1.05f;
            if (entrada) portal.AddComponent<PortalEntrada>();
            else portal.AddComponent<PortalSalida>();

            GameObject textoObjeto = new GameObject("Etiqueta");
            textoObjeto.transform.SetParent(portal.transform, false);
            textoObjeto.transform.localPosition = new Vector3(0f, 1.65f, 0f);
            TextMesh texto = textoObjeto.AddComponent<TextMesh>();
            texto.text = etiqueta;
            texto.anchor = TextAnchor.MiddleCenter;
            texto.alignment = TextAlignment.Center;
            texto.fontSize = 36;
            texto.characterSize = 0.055f;
            texto.color = color;
            texto.GetComponent<MeshRenderer>().sortingOrder = 81;

            PrefabUtility.SaveAsPrefabAsset(portal, ruta);
            UnityEngine.Object.DestroyImmediate(portal);
        }

        private static void CrearInstanciaPortal(string ruta, string nombre, Vector3 posicion)
        {
            DestruirRaizSiExiste(nombre);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ruta);
            GameObject portal = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            portal.name = nombre;
            portal.transform.position = new Vector3(posicion.x, posicion.y, 0f);
        }

        private static Material CrearMaterial(string ruta, Color color)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(ruta);
            if (material == null)
            {
                Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, ruta);
            }
            material.color = color;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void CalcularExtremosTilemap(out Vector3 entrada, out Vector3 salida)
        {
            List<Vector3> celdas = new List<Vector3>();
            foreach (Tilemap mapa in UnityEngine.Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
            {
                foreach (Vector3Int celda in mapa.cellBounds.allPositionsWithin)
                {
                    if (mapa.HasTile(celda)) celdas.Add(mapa.GetCellCenterWorld(celda));
                }
            }

            if (celdas.Count == 0)
            {
                entrada = new Vector3(-7f, 2f, 0f);
                salida = new Vector3(18f, 2f, 0f);
                return;
            }

            float minimoX = celdas.Min(p => p.x);
            float maximoX = celdas.Max(p => p.x);
            entrada = celdas.Where(p => p.x <= minimoX + 2.1f).OrderByDescending(p => p.y).First() + Vector3.up * 1.45f;
            salida = celdas.Where(p => p.x >= maximoX - 2.1f).OrderByDescending(p => p.y).First() + Vector3.up * 1.45f;
        }

        private static Vector3 CentroJugadores(JugadorCooperativo[] jugadores)
        {
            return (jugadores[0].transform.position + jugadores[1].transform.position) * 0.5f;
        }

        private static Vector3 BuscarPosicionMeta()
        {
            MetaNivel meta = UnityEngine.Object.FindFirstObjectByType<MetaNivel>();
            if (meta != null) return meta.transform.position;
            CalcularExtremosTilemap(out _, out Vector3 salida);
            return salida;
        }

        private static void OrdenarJugadores(JugadorCooperativo[] jugadores)
        {
            Array.Sort(jugadores, (a, b) => string.CompareOrdinal(a.NombreJugador, b.NombreJugador));
        }

        private static GameObject BuscarRaiz(Scene escena, string nombre)
        {
            return escena.GetRootGameObjects().FirstOrDefault(objeto => objeto.name == nombre);
        }

        private static void DestruirRaizSiExiste(string nombre)
        {
            Scene escena = SceneManager.GetActiveScene();
            GameObject objeto = BuscarRaiz(escena, nombre);
            if (objeto != null) UnityEngine.Object.DestroyImmediate(objeto);
        }

        private static void CrearCarpetas()
        {
            CrearCarpetaSiFalta("Assets/Prefabs", "Campania");
            CrearCarpetaSiFalta("Assets/Materiales", "Campania");
        }

        private static void CrearCarpetaSiFalta(string padre, string nombre)
        {
            string ruta = padre + "/" + nombre;
            if (!AssetDatabase.IsValidFolder(ruta)) AssetDatabase.CreateFolder(padre, nombre);
        }

        public static void ConfigurarBuildSettings()
        {
            EditorBuildSettings.scenes = EscenasCampania
                .Select(ruta => new EditorBuildSettingsScene(ruta, true)).ToArray();
        }
    }
}
