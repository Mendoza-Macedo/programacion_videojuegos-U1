using JuegoCooperativo.Audio;
using JuegoCooperativo.Cuerda;
using JuegoCooperativo.Juego;
using JuegoCooperativo.Mundo;
using JuegoCooperativo.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace JuegoCooperativo.Editor
{
    public static class ConstructorJuegoCompleto
    {
        private const string RutaEscenaPrincipal = "Assets/Escenas/EscenaCooperativa.unity";
        private const string RutaEscenaSample = "Assets/Scenes/SampleScene.unity";
        private const string NombreLayerSuelo = "Suelo";

        [MenuItem("Herramientas/BreadFred/Construir juego completo v1")]
        public static void Construir()
        {
            ConstructorAnimalesSierra.AgregarAnimalesALaEscena();
            ActualizarEscena(RutaEscenaPrincipal);
            ActualizarEscena(RutaEscenaSample);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Juego completo v1 construido.");
        }

        [MenuItem("Herramientas/BreadFred/Recrear escenario principal con assets")]
        public static void ConstruirEscenarioPrincipal()
        {
            ActualizarEscena(RutaEscenaPrincipal);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Escenario principal recreado con assets reales.");
        }

        private static void ActualizarEscena(string ruta)
        {
            var escena = EditorSceneManager.OpenScene(ruta);
            int layerSuelo = LayerMask.NameToLayer(NombreLayerSuelo);

            CrearAudio();
            CrearEscenarioPrincipalConAssets(layerSuelo);
            CrearUiJuego();
            ConectarIndicadorCuerda();
            ConectarFlujoUi();
            AjustarCamara();

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena, ruta);
        }

        private static void CrearEscenarioPrincipalConAssets(int layerSuelo)
        {
            LimpiarEscenarioAnterior();

            GameObject raiz = new GameObject("Escenario Principal Assets");
            CrearFondos(raiz.transform);
            CrearTilemapsEscenario(raiz.transform, layerSuelo);
            CrearDecoracionSunny(raiz.transform);
            CrearDecoracionHielo(raiz.transform);
            CrearPeligrosYAgua(raiz.transform, layerSuelo);
            ReubicarGameplay();
        }

        private static void LimpiarEscenarioAnterior()
        {
            string[] nombres =
            {
                "Escenario Principal Assets", "Escenario Pixel Art Principal",
                "Montana Fondo 1", "Montana Fondo 2", "Montana Nevada", "Montana Nevada (1)", "Cielo Claro",
                "Suelo inicial", "Escalon 1", "Escalon 2", "Puente angosto", "Repisa alta", "Cima",
                "Tutorial Movimiento", "Tutorial Salto", "Tutorial Cuerda", "Repisa Sierra 1", "Repisa Sierra 2",
                "Pared Cuerda Final", "Cima Final", "Pared agarre izquierda", "Pared agarre derecha",
                "Plataforma movil", "Aspa peligrosa", "Pilar cuerda bajo", "Torre condor",
                "Base bosque inicial", "Isla caja izquierda", "Meseta cuerda central", "Repisa manzanas superior",
                "Puente tierra pinchos", "Bosque maquinaria", "Cueva vizcacha", "Meta bosque",
                "Castillo hielo izquierda", "Piso hielo bajo", "Plataforma monedas hielo A", "Plataforma monedas hielo B",
                "Torre hielo alta", "Rampa hielo final", "Igloo final"
            };

            foreach (string nombre in nombres)
            {
                GameObject obj = Buscar(nombre);
                if (obj != null) Object.DestroyImmediate(obj);
            }

            for (int i = 0; i < 24; i++)
            {
                GameObject ichu = Buscar($"Ichu decorativo {i}");
                if (ichu != null) Object.DestroyImmediate(ichu);
            }
        }

        private static void CrearFondos(Transform raiz)
        {
            SpriteObj(raiz, "Assets/SunnyLand Artwork/Environment/back.png", "Cielo azul pixel", new Vector2(7f, 4.8f), new Vector2(0.30f, 0.22f), -60);
            SpriteObj(raiz, "Assets/2D Ice World/Assets/Sprites/Iceworld_Background_0002.png", "Montanas nevadas fondo A", new Vector2(0f, 4.2f), new Vector2(0.070f, 0.070f), -50);
            SpriteObj(raiz, "Assets/2D Ice World/Assets/Sprites/Iceworld_Background_0002.png", "Montanas nevadas fondo B", new Vector2(17f, 4.2f), new Vector2(0.070f, 0.070f), -51);
            SpriteObj(raiz, "Assets/2D Ice World/Assets/Sprites/Iceworld_Background_0001.png", "Lago y cordillera lejana", new Vector2(8f, -0.1f), new Vector2(0.085f, 0.070f), -49);
            SpriteObj(raiz, "Assets/SunnyLand Artwork/Environment/middle.png", "Bosque parallax izquierdo", new Vector2(-7.2f, -1.4f), new Vector2(0.070f, 0.085f), -42, new Color(0.78f, 0.95f, 0.84f));
            SpriteObj(raiz, "Assets/SunnyLand Artwork/Environment/middle.png", "Bosque parallax derecho", new Vector2(19.5f, -1.2f), new Vector2(-0.075f, 0.085f), -42, new Color(0.76f, 0.92f, 0.86f));
        }

        private static void CrearTilemapsEscenario(Transform raiz, int layerSuelo)
        {
            Grid grid = new GameObject("Grid terreno principal").AddComponent<Grid>();
            grid.transform.SetParent(raiz, false);
            grid.cellSize = Vector3.one;

            Tilemap sunny = CrearTilemap(grid.transform, "Tilemap bosque SunnyLand", layerSuelo, 0);
            Tilemap hielo = CrearTilemap(grid.transform, "Tilemap hielo IceWorld", layerSuelo, 1);

            TileBase pasto = Tile("Assets/SunnyLand Artwork/Environment/Tileset/tileset_1.asset");
            TileBase tierra = Tile("Assets/SunnyLand Artwork/Environment/Tileset/tileset_16.asset") ?? Tile("Assets/SunnyLand Artwork/Environment/Tileset/tileset_17.asset");
            TileBase borde = Tile("Assets/SunnyLand Artwork/Environment/Tileset/tileset_2.asset") ?? pasto;

            TileBase nieve = Tile("Assets/2D Ice World/Assets/Sprites/Tile_Prefabs/Ice_platform.asset") ?? Tile("Assets/2D Ice World/Assets/Sprites/Tile_Prefabs/Ice_Top_1.asset");
            TileBase hieloBase = Tile("Assets/2D Ice World/Assets/Sprites/Tile_Prefabs/Ice_G_M.asset");
            TileBase hieloBorde = Tile("Assets/2D Ice World/Assets/Sprites/Tile_Prefabs/Ice_G_26_big_R.asset") ?? hieloBase;

            RectSunny(sunny, "Inicio bosque", -10, -3, 8, 3, pasto, tierra, borde);
            RectSunny(sunny, "Isla caja", -2, 1, 3, 2, pasto, tierra, borde);
            RectSunny(sunny, "Meseta central cuerda", 3, -1, 6, 3, pasto, tierra, borde);
            RectSunny(sunny, "Repisa manzanas", 7, 4, 5, 2, pasto, tierra, borde);
            RectSunny(sunny, "Puente con pinchos", 9, -2, 5, 2, pasto, tierra, borde);
            RectSunny(sunny, "Bosque maquinaria", 17, -2, 7, 3, pasto, tierra, borde);
            RectSunny(sunny, "Cueva vizcacha", 18, -4, 4, 2, pasto, tierra, borde);
            RectSunny(sunny, "Meta", 25, -1, 4, 2, pasto, tierra, borde);

            RectIce(hielo, "Castillo hielo", -13, -1, 3, 8, nieve, hieloBase, hieloBorde);
            RectIce(hielo, "Piso hielo", -9, -5, 9, 2, nieve, hieloBase, hieloBorde);
            RectIce(hielo, "Plataforma hielo A", 1, 2, 3, 1, nieve, hieloBase, hieloBorde);
            RectIce(hielo, "Plataforma hielo B", 5, 2, 3, 1, nieve, hieloBase, hieloBorde);
            RectIce(hielo, "Torre hielo alta", 11, -2, 3, 6, nieve, hieloBase, hieloBorde);
            RectIce(hielo, "Rampa hielo final", 14, -4, 5, 2, nieve, hieloBase, hieloBorde);
            RectIce(hielo, "Igloo hielo", 22, -3, 6, 2, nieve, hieloBase, hieloBorde);
        }

        private static Tilemap CrearTilemap(Transform padre, string nombre, int layerSuelo, int orden)
        {
            GameObject obj = new GameObject(nombre);
            obj.transform.SetParent(padre, false);
            obj.layer = layerSuelo;

            Tilemap tilemap = obj.AddComponent<Tilemap>();
            TilemapRenderer renderer = obj.AddComponent<TilemapRenderer>();
            renderer.sortingOrder = orden;

            TilemapCollider2D collider = obj.AddComponent<TilemapCollider2D>();
            collider.usedByComposite = true;
            Rigidbody2D rb = obj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
            obj.AddComponent<CompositeCollider2D>();
            return tilemap;
        }

        private static void RectSunny(Tilemap mapa, string nombre, int x, int y, int ancho, int alto, TileBase top, TileBase fill, TileBase right)
        {
            RectTiles(mapa, x, y, ancho, alto, top, fill, right);
        }

        private static void RectIce(Tilemap mapa, string nombre, int x, int y, int ancho, int alto, TileBase top, TileBase fill, TileBase right)
        {
            RectTiles(mapa, x, y, ancho, alto, top, fill, right);
        }

        private static void RectTiles(Tilemap mapa, int x, int y, int ancho, int alto, TileBase top, TileBase fill, TileBase right)
        {
            for (int ix = x; ix < x + ancho; ix++)
            {
                for (int iy = y; iy < y + alto; iy++)
                {
                    TileBase tile = iy == y + alto - 1 ? top : fill;
                    if (ix == x + ancho - 1 && iy < y + alto - 1) tile = right;
                    mapa.SetTile(new Vector3Int(ix, iy, 0), tile);
                }
            }
        }

        private static TileBase Tile(string ruta)
        {
            return AssetDatabase.LoadAssetAtPath<TileBase>(ruta);
        }

        private static void CrearTerrenoSunny(Transform raiz, int layerSuelo)
        {
            CrearPlataformaSunny(raiz, "Base bosque inicial", new Vector2(-5.7f, -2.0f), new Vector2(8.4f, 1.55f), layerSuelo);
            CrearPlataformaSunny(raiz, "Isla caja izquierda", new Vector2(-0.7f, 0.55f), new Vector2(2.0f, 0.92f), layerSuelo);
            CrearPlataformaSunny(raiz, "Meseta cuerda central", new Vector2(5.0f, -0.55f), new Vector2(5.0f, 1.2f), layerSuelo);
            CrearPlataformaSunny(raiz, "Repisa manzanas superior", new Vector2(8.0f, 2.9f), new Vector2(4.6f, 0.9f), layerSuelo);
            CrearPlataformaSunny(raiz, "Puente tierra pinchos", new Vector2(10.2f, -1.0f), new Vector2(4.2f, 0.9f), layerSuelo);
            CrearPlataformaSunny(raiz, "Bosque maquinaria", new Vector2(18.4f, -1.15f), new Vector2(5.2f, 1.15f), layerSuelo);
            CrearPlataformaSunny(raiz, "Cueva vizcacha", new Vector2(18.1f, -2.95f), new Vector2(2.4f, 0.8f), layerSuelo);
            CrearPlataformaSunny(raiz, "Meta bosque", new Vector2(26.6f, -0.85f), new Vector2(2.7f, 0.95f), layerSuelo);

            CrearCollider("Pilar cuerda bajo", new Vector2(4.0f, -0.9f), new Vector2(0.35f, 2.8f), layerSuelo);
            SpriteObj(raiz, "Assets/SunnyLand Artwork/Environment/props/block-big.png", "Pilar cuerda visual", new Vector2(4.0f, -0.55f), new Vector2(0.7f, 2.4f), 2);

            CrearCollider("Torre condor", new Vector2(15.1f, 0.75f), new Vector2(1.1f, 4.3f), layerSuelo);
            for (int i = 0; i < 5; i++)
            {
                SpriteObj(raiz, "Assets/2D Ice World/Assets/Sprites/Single Sprites/Ice_cube_big_0001.png", $"Torre condor hielo {i}", new Vector2(15.1f, -0.95f + i * 0.75f), new Vector2(0.62f, 0.62f), 3);
            }
        }

        private static void CrearTerrenoHielo(Transform raiz, int layerSuelo)
        {
            CrearPlataformaHielo(raiz, "Castillo hielo izquierda", new Vector2(-9.8f, 1.6f), new Vector2(2.0f, 5.4f), layerSuelo);
            CrearPlataformaHielo(raiz, "Piso hielo bajo", new Vector2(-5.4f, -3.35f), new Vector2(8.8f, 0.9f), layerSuelo);
            CrearPlataformaHielo(raiz, "Plataforma monedas hielo A", new Vector2(3.0f, 1.15f), new Vector2(1.8f, 0.72f), layerSuelo);
            CrearPlataformaHielo(raiz, "Plataforma monedas hielo B", new Vector2(7.0f, 1.25f), new Vector2(1.9f, 0.72f), layerSuelo);
            CrearPlataformaHielo(raiz, "Torre hielo alta", new Vector2(11.7f, 2.15f), new Vector2(2.1f, 4.6f), layerSuelo);
            CrearPlataformaHielo(raiz, "Rampa hielo final", new Vector2(15.2f, -2.3f), new Vector2(4.6f, 0.82f), layerSuelo);
            CrearPlataformaHielo(raiz, "Igloo final", new Vector2(23.4f, -1.55f), new Vector2(6.4f, 1.0f), layerSuelo);
        }

        private static void CrearDecoracionSunny(Transform raiz)
        {
            SpriteObj(raiz, "Assets/SunnyLand Artwork/Environment/props/tree.png", "Arbol manzanas grande", new Vector2(-9.1f, -0.65f), new Vector2(1.85f, 1.85f), 9);
            SpriteObj(raiz, "Assets/SunnyLand Artwork/Environment/props/tree-2.png", "Arbol manzanas derecho", new Vector2(19.5f, 0.45f), new Vector2(1.65f, 1.65f), 9);
            SpriteObj(raiz, "Assets/SunnyLand Artwork/Environment/props/crate.png", "Caja bajo arbol", new Vector2(-9.2f, -0.92f), new Vector2(1.1f, 1.1f), 11);
            SpriteObj(raiz, "Assets/SunnyLand Artwork/Environment/props/crate.png", "Caja isla", new Vector2(-0.65f, 1.1f), new Vector2(1.1f, 1.1f), 11);
            SpriteObj(raiz, "Assets/SunnyLand Artwork/Environment/props/big-crate.png", "Caja superior", new Vector2(9.7f, 3.65f), new Vector2(1.05f, 1.05f), 11);
            SpriteObj(raiz, "Assets/SunnyLand Artwork/Environment/props/crank-up.png", "Engranaje bosque", new Vector2(-1.9f, -2.1f), new Vector2(1.4f, 1.4f), 12);
            SpriteObj(raiz, "Assets/SunnyLand Artwork/Environment/props/crank-down.png", "Palanca maquinaria", new Vector2(17.1f, -0.55f), new Vector2(1.0f, 1.0f), 12);
            SpriteObj(raiz, "Assets/SunnyLand Artwork/Environment/props/crank-up.png", "Engranaje peligro", new Vector2(20.8f, -2.4f), new Vector2(1.5f, 1.5f), 12);
            SpriteObj(raiz, "Assets/SunnyLand Artwork/Environment/props/bush.png", "Arbusto inicio", new Vector2(-7.8f, -0.9f), new Vector2(1.2f, 1.2f), 13);
            SpriteObj(raiz, "Assets/SunnyLand Artwork/Environment/props/rock-2.png", "Rocas fondo inicio", new Vector2(-7.0f, -1.9f), new Vector2(1.15f, 1.15f), 8);
            SpriteObj(raiz, "Assets/SunnyLand Artwork/Environment/props/sign.png", "Bandera meta decorativa", new Vector2(27.3f, -0.05f), new Vector2(1.1f, 1.1f), 13);
            SpriteObj(raiz, "Assets/SunnyLand Artwork/Environment/props/plant-house.png", "Casita bosque fondo", new Vector2(22.1f, -0.2f), new Vector2(1.1f, 1.1f), 7);
        }

        private static void CrearDecoracionHielo(Transform raiz)
        {
            SpriteObj(raiz, "Assets/2D Ice World/Assets/Sprites/Single Sprites/Igloo_Top_L_0001.png", "Igloo top L", new Vector2(23.0f, -0.35f), new Vector2(1.35f, 1.35f), 11);
            SpriteObj(raiz, "Assets/2D Ice World/Assets/Sprites/Single Sprites/Igloo_Top_R_0001.png", "Igloo top R", new Vector2(24.05f, -0.35f), new Vector2(1.35f, 1.35f), 11);
            SpriteObj(raiz, "Assets/2D Ice World/Assets/Sprites/Single Sprites/Igloo_Door_L_0001.png", "Igloo puerta L", new Vector2(23.0f, -1.15f), new Vector2(1.35f, 1.35f), 12);
            SpriteObj(raiz, "Assets/2D Ice World/Assets/Sprites/Single Sprites/Igloo_Door_R_0001.png", "Igloo puerta R", new Vector2(24.05f, -1.15f), new Vector2(1.35f, 1.35f), 12);
            SpriteObj(raiz, "Assets/2D Ice World/Assets/Sprites/Single Sprites/Snowtree_ground_0001.png", "Arbol nieve lateral", new Vector2(-10.8f, 0.3f), new Vector2(1.15f, 1.15f), 12);
            SpriteObj(raiz, "Assets/2D Ice World/Assets/Sprites/Single Sprites/Ice_decoration_0001.png", "Cristales entrada", new Vector2(-10.9f, 1.1f), new Vector2(1.1f, 1.1f), 13);
            SpriteObj(raiz, "Assets/2D Ice World/Assets/Sprites/Single Sprites/Snowball_big_0001.png", "Bola nieve izquierda", new Vector2(-2.5f, -2.15f), new Vector2(1.0f, 1.0f), 13);
            SpriteObj(raiz, "Assets/2D Ice World/Assets/Sprites/Single Sprites/Snowball_big_0001.png", "Bola nieve rampa", new Vector2(15.7f, -1.45f), new Vector2(1.0f, 1.0f), 13);
            SpriteObj(raiz, "Assets/2D Ice World/Assets/Sprites/Single Sprites/Ice_cube_small_0001.png", "Cubos hielo A", new Vector2(12.5f, -1.05f), new Vector2(0.9f, 0.9f), 13);
            SpriteObj(raiz, "Assets/2D Ice World/Assets/Sprites/Single Sprites/Ice_cube_small_0001.png", "Cubos hielo B", new Vector2(17.0f, -2.05f), new Vector2(0.9f, 0.9f), 13);
            SpriteObj(raiz, "Assets/2D Ice World/Assets/Sprites/Single Sprites/Snowflake_0004.png", "Copo decorativo A", new Vector2(4.2f, 5.8f), new Vector2(0.8f, 0.8f), 13);
            SpriteObj(raiz, "Assets/2D Ice World/Assets/Sprites/Single Sprites/Snowflake_0005.png", "Copo decorativo B", new Vector2(18.2f, 4.9f), new Vector2(0.7f, 0.7f), 13);
        }

        private static void CrearPeligrosYAgua(Transform raiz, int layerSuelo)
        {
            SpriteObj(raiz, "Assets/2D Ice World/Assets/Sprites/Single Sprites/Water_0001.png", "Rio helado A", new Vector2(12.4f, -3.5f), new Vector2(2.4f, 1.0f), 6);
            SpriteObj(raiz, "Assets/2D Ice World/Assets/Sprites/Single Sprites/Water_0002.png", "Rio helado B", new Vector2(15.8f, -3.5f), new Vector2(2.4f, 1.0f), 6);
            SpriteObj(raiz, "Assets/2D Ice World/Assets/Sprites/Single Sprites/Water_0003.png", "Rio helado C", new Vector2(19.2f, -3.5f), new Vector2(2.4f, 1.0f), 6);
            SpriteObj(raiz, "Assets/2D Ice World/Assets/Sprites/Single Sprites/Water_0004.png", "Cascada fondo derecha", new Vector2(24.8f, -0.8f), new Vector2(1.4f, 2.8f), 5);

            SpriteObj(raiz, "Assets/SunnyLand Artwork/Environment/props/spikes.png", "Pinchos tierra central", new Vector2(9.4f, -0.25f), new Vector2(1.25f, 1.0f), 14);
            SpriteObj(raiz, "Assets/2D Ice World/Assets/Sprites/Single Sprites/Ice_Bot_spikes_0001.png", "Pinchos hielo bajo", new Vector2(4.0f, -2.55f), new Vector2(1.25f, 1.0f), 14);
            SpriteObj(raiz, "Assets/2D Ice World/Assets/Sprites/Single Sprites/Ice_Top_spikes_0001.png", "Pinchos maquina final", new Vector2(21.5f, -1.55f), new Vector2(1.35f, 1.0f), 14);

            GameObject movil = CrearCollider("Plataforma movil", new Vector2(18.5f, 2.2f), new Vector2(2.5f, 0.28f), layerSuelo);
            Rigidbody2D rb = movil.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            PlataformaMovil plataforma = movil.AddComponent<PlataformaMovil>();
            SerializedObject so = new SerializedObject(plataforma);
            so.FindProperty("desplazamiento").vector2Value = new Vector2(2.0f, 0f);
            so.ApplyModifiedPropertiesWithoutUndo();
            SpriteObj(movil.transform, "Assets/SunnyLand Artwork/Environment/props/platform-long.png", "Visual plataforma movil", Vector2.zero, new Vector2(1.2f, 0.9f), 18);

            GameObject aspa = CrearCollider("Aspa peligrosa", new Vector2(1.2f, -2.2f), new Vector2(1.7f, 0.18f), layerSuelo);
            aspa.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            aspa.AddComponent<ObstaculoGiratorio>();
            SpriteObj(aspa.transform, "Assets/SunnyLand Artwork/Environment/props/crank-up.png", "Visual aspa engranaje", Vector2.zero, new Vector2(1.1f, 1.1f), 18);

            GameObject zona = Buscar("Zona de muerte");
            if (zona != null)
            {
                zona.transform.position = new Vector2(8f, -6f);
                BoxCollider2D trigger = zona.GetComponent<BoxCollider2D>();
                if (trigger != null) trigger.size = new Vector2(52f, 1f);
            }
        }

        private static void ReubicarGameplay()
        {
            Mover("Jugador Azul", new Vector2(-8.2f, 0.25f));
            Mover("Jugador Rojo", new Vector2(-6.6f, 0.25f));
            Mover("Punto Control Inicio", new Vector2(-7.4f, 1.15f));
            Mover("Punto Control 1", new Vector2(5.8f, 2.2f));
            Mover("Punto Control 2", new Vector2(19.2f, 1.2f));
            Mover("Meta", new Vector2(26.9f, 1.1f));

            GestorJuego gestor = Buscar("Gestor del Juego")?.GetComponent<GestorJuego>();
            if (gestor != null)
            {
                SerializedObject so = new SerializedObject(gestor);
                so.FindProperty("puntoReaparicionUno").vector3Value = new Vector3(-8.2f, 0.25f, 0f);
                so.FindProperty("puntoReaparicionDos").vector3Value = new Vector3(-6.6f, 0.25f, 0f);
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            Camera cam = Buscar("Camara Cooperativa")?.GetComponent<Camera>();
            if (cam != null)
            {
                cam.backgroundColor = new Color(0.22f, 0.70f, 1f);
                cam.orthographicSize = 6.4f;
            }
        }

        private static void CrearPlataformaSunny(Transform raiz, string nombre, Vector2 centro, Vector2 tamano, int layerSuelo)
        {
            CrearCollider(nombre, centro, tamano, layerSuelo);
            SpriteObj(raiz, "Assets/SunnyLand Artwork/Environment/props/platform-long.png", nombre + " pasto", centro + Vector2.up * (tamano.y * 0.48f), new Vector2(tamano.x * 0.42f, 1f), 5);
            SpriteObj(raiz, "Assets/SunnyLand Artwork/Environment/props/block-big.png", nombre + " tierra", centro + Vector2.down * 0.08f, new Vector2(tamano.x * 0.45f, Mathf.Max(0.55f, tamano.y * 0.95f)), 3);
            SpriteObj(raiz, "Assets/SunnyLand Artwork/Environment/props/block.png", nombre + " piedra borde", centro + new Vector2(tamano.x * 0.48f, 0f), new Vector2(0.95f, tamano.y * 0.95f), 4);
        }

        private static void CrearPlataformaHielo(Transform raiz, string nombre, Vector2 centro, Vector2 tamano, int layerSuelo)
        {
            CrearCollider(nombre, centro, tamano, layerSuelo);
            SpriteObj(raiz, "Assets/2D Ice World/Assets/Sprites/Single Sprites/Ice_platform_0001.png", nombre + " nieve superior", centro + Vector2.up * (tamano.y * 0.50f), new Vector2(tamano.x * 0.50f, 1.0f), 5);
            SpriteObj(raiz, "Assets/2D Ice World/Assets/Sprites/Single Sprites/Ice_G_M_0001.png", nombre + " bloque hielo", centro, new Vector2(tamano.x * 0.55f, Mathf.Max(0.65f, tamano.y * 0.90f)), 3);
            SpriteObj(raiz, "Assets/2D Ice World/Assets/Sprites/Single Sprites/Ice_Top_0001.png", nombre + " brillo nieve", centro + Vector2.up * (tamano.y * 0.53f), new Vector2(tamano.x * 0.45f, 0.8f), 6);
        }

        private static GameObject CrearCollider(string nombre, Vector2 centro, Vector2 tamano, int layerSuelo)
        {
            GameObject obj = new GameObject(nombre);
            obj.layer = layerSuelo;
            obj.transform.position = centro;
            BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
            col.size = tamano;
            return obj;
        }

        private static GameObject SpriteObj(Transform padre, string ruta, string nombre, Vector2 posicion, Vector2 escala, int orden)
        {
            return SpriteObj(padre, ruta, nombre, posicion, escala, orden, Color.white);
        }

        private static GameObject SpriteObj(Transform padre, string ruta, string nombre, Vector2 posicion, Vector2 escala, int orden, Color color)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
            if (sprite == null)
            {
                Debug.LogWarning("No se pudo cargar sprite real para escenario: " + ruta);
                return null;
            }

            GameObject obj = new GameObject(nombre);
            obj.transform.SetParent(padre, false);
            obj.transform.localPosition = posicion;
            obj.transform.localScale = new Vector3(escala.x, escala.y, 1f);
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = orden;
            return obj;
        }

        private static void Mover(string nombre, Vector2 posicion)
        {
            GameObject obj = Buscar(nombre);
            if (obj != null) obj.transform.position = posicion;
        }

        private static void CrearAudio()
        {
            GameObject audio = Buscar("Sonidos del Juego") ?? new GameObject("Sonidos del Juego");
            if (audio.GetComponent<AudioSource>() == null) audio.AddComponent<AudioSource>();
            if (audio.GetComponent<SonidosJuego>() == null) audio.AddComponent<SonidosJuego>();
        }

        private static void CrearDecoracionAndina(int layerSuelo)
        {
            CrearFondo("Montana Fondo 1", new Vector2(4f, 6f), new Vector2(18f, 7f), new Color(0.32f, 0.45f, 0.46f), -20);
            CrearFondo("Montana Fondo 2", new Vector2(20f, 8f), new Vector2(22f, 9f), new Color(0.24f, 0.36f, 0.40f), -21);
            CrearFondo("Montana Nevada", new Vector2(34f, 13f), new Vector2(24f, 12f), new Color(0.38f, 0.50f, 0.52f), -22);
            CrearFondo("Cielo Claro", new Vector2(18f, 12f), new Vector2(70f, 36f), new Color(0.45f, 0.75f, 0.95f), -30);

            for (int i = 0; i < 14; i++)
            {
                CrearBloque($"Ichu decorativo {i}", new Vector2(-2f + i * 3.2f, 0.1f + (i % 4) * 1.7f), new Vector2(0.15f, 0.85f), new Color(0.72f, 0.64f, 0.30f), layerSuelo, false, 2);
            }
        }

        private static void CrearNivelExtendido(int layerSuelo)
        {
            CrearBloque("Tutorial Movimiento", new Vector2(-4.5f, -0.6f), new Vector2(4f, 0.7f), new Color(0.20f, 0.28f, 0.30f), layerSuelo, true, 0);
            CrearBloque("Tutorial Salto", new Vector2(-7.5f, 1.0f), new Vector2(1.6f, 0.45f), new Color(0.25f, 0.35f, 0.33f), layerSuelo, true, 0);
            CrearBloque("Tutorial Cuerda", new Vector2(-10.5f, 2.6f), new Vector2(1.4f, 0.4f), new Color(0.25f, 0.35f, 0.33f), layerSuelo, true, 0);

            CrearBloque("Repisa Sierra 1", new Vector2(23.5f, 11.2f), new Vector2(2.8f, 0.45f), new Color(0.22f, 0.36f, 0.32f), layerSuelo, true, 0);
            CrearBloque("Repisa Sierra 2", new Vector2(27.2f, 13.4f), new Vector2(2.4f, 0.45f), new Color(0.22f, 0.36f, 0.32f), layerSuelo, true, 0);
            CrearBloque("Pared Cuerda Final", new Vector2(30.2f, 15.2f), new Vector2(0.35f, 4.3f), new Color(0.18f, 0.27f, 0.31f), layerSuelo, true, 0);
            CrearBloque("Cima Final", new Vector2(36f, 18f), new Vector2(8f, 0.6f), new Color(0.26f, 0.42f, 0.34f), layerSuelo, true, 0);

            GameObject meta = Buscar("Meta");
            if (meta != null) meta.transform.position = new Vector2(38.5f, 18.95f);
        }

        private static void CrearUiJuego()
        {
            GameObject canvas = Buscar("Interfaz Juego Completo") ?? new GameObject("Interfaz Juego Completo");
            Canvas c = canvas.GetComponent<Canvas>(); if (c == null) c = canvas.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>(); if (scaler == null) scaler = canvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            if (canvas.GetComponent<GraphicRaycaster>() == null) canvas.AddComponent<GraphicRaycaster>();

            CrearPanelTexto(canvas.transform, "Panel Menu Principal", "ANIMALES DE LA CUMBRE\nCooperativo Andino\n\nEnter/Espacio: empezar\nEsc: pausa durante el juego", new Vector2(0f, 0f), new Vector2(760f, 390f), 34);
            CrearPanelTexto(canvas.transform, "Panel Pausa", "PAUSA\n\nEsc: continuar\nR: reiniciar", new Vector2(0f, 0f), new Vector2(520f, 260f), 32).SetActive(false);
            CrearPanelTexto(canvas.transform, "Panel Victoria", "META ALCANZADA", new Vector2(0f, 0f), new Vector2(600f, 300f), 32).SetActive(false);

            CrearBarraTension(canvas.transform);
        }

        private static void CrearBarraTension(Transform canvas)
        {
            GameObject raiz = CrearUi(canvas, "Indicador Tension");
            ConfigRect(raiz.GetComponent<RectTransform>(), new Vector2(0f, -316f), new Vector2(430f, 44f));
            Image fondo = raiz.GetComponent<Image>(); if (fondo == null) fondo = raiz.AddComponent<Image>();
            fondo.color = new Color(0.04f, 0.06f, 0.08f, 0.82f);

            GameObject fill = CrearUi(raiz.transform, "Relleno Tension");
            Image img = fill.GetComponent<Image>(); if (img == null) img = fill.AddComponent<Image>();
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Horizontal;
            img.fillAmount = 0f;
            img.color = new Color(0.3f, 0.9f, 0.55f);
            ConfigRect(fill.GetComponent<RectTransform>(), Vector2.zero, new Vector2(400f, 18f));

            Text texto = CrearTexto(raiz.transform, "Texto Tension", "Tension de cuerda", 18, TextAnchor.UpperCenter);
            ConfigRect(texto.GetComponent<RectTransform>(), new Vector2(0f, 14f), new Vector2(400f, 24f));

            IndicadorTensionCuerda indicador = raiz.GetComponent<IndicadorTensionCuerda>(); if (indicador == null) indicador = raiz.AddComponent<IndicadorTensionCuerda>();
            SerializedObject so = new SerializedObject(indicador);
            so.FindProperty("relleno").objectReferenceValue = img;
            so.FindProperty("texto").objectReferenceValue = texto;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConectarIndicadorCuerda()
        {
            IndicadorTensionCuerda indicador = Buscar("Indicador Tension")?.GetComponent<IndicadorTensionCuerda>();
            CuerdaCooperativa cuerda = Buscar("Cuerda Cooperativa")?.GetComponent<CuerdaCooperativa>();
            if (indicador == null || cuerda == null) return;
            SerializedObject so = new SerializedObject(indicador);
            so.FindProperty("cuerda").objectReferenceValue = cuerda;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConectarFlujoUi()
        {
            GameObject flujoObj = Buscar("Flujo Juego UI") ?? new GameObject("Flujo Juego UI");
            FlujoJuegoUI flujo = flujoObj.GetComponent<FlujoJuegoUI>(); if (flujo == null) flujo = flujoObj.AddComponent<FlujoJuegoUI>();
            SerializedObject so = new SerializedObject(flujo);
            so.FindProperty("panelMenu").objectReferenceValue = Buscar("Panel Menu Principal");
            so.FindProperty("panelPausa").objectReferenceValue = Buscar("Panel Pausa");
            so.FindProperty("panelVictoria").objectReferenceValue = Buscar("Panel Victoria");
            so.FindProperty("textoVictoria").objectReferenceValue = Buscar("Texto Panel Victoria")?.GetComponent<Text>();
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AjustarCamara()
        {
            var camara = Buscar("Camara Cooperativa")?.GetComponent<JuegoCooperativo.Camara.CamaraCooperativa>();
            if (camara == null) return;
            SerializedObject so = new SerializedObject(camara);
            so.FindProperty("limiteMinimo").vector2Value = new Vector2(-11.5f, -4.8f);
            so.FindProperty("limiteMaximo").vector2Value = new Vector2(28f, 6.5f);
            so.FindProperty("tamanoMinimo").floatValue = 5.6f;
            so.FindProperty("tamanoMaximo").floatValue = 6.8f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject CrearPanelTexto(Transform canvas, string nombre, string texto, Vector2 pos, Vector2 size, int fontSize)
        {
            GameObject panel = CrearUi(canvas, nombre);
            Image img = panel.GetComponent<Image>(); if (img == null) img = panel.AddComponent<Image>();
            img.color = new Color(0.04f, 0.07f, 0.10f, 0.90f);
            ConfigRect(panel.GetComponent<RectTransform>(), pos, size);

            Text t = CrearTexto(panel.transform, "Texto " + nombre.Replace("Panel ", "Panel "), texto, fontSize, TextAnchor.MiddleCenter);
            ConfigRect(t.GetComponent<RectTransform>(), Vector2.zero, size - new Vector2(50f, 40f));
            return panel;
        }

        private static Text CrearTexto(Transform padre, string nombre, string contenido, int fontSize, TextAnchor anchor)
        {
            GameObject obj = CrearUi(padre, nombre);
            Text t = obj.GetComponent<Text>(); if (t == null) t = obj.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = fontSize;
            t.alignment = anchor;
            t.color = Color.white;
            t.text = contenido;
            t.raycastTarget = false;
            return t;
        }

        private static GameObject CrearUi(Transform padre, string nombre)
        {
            GameObject obj = Buscar(nombre) ?? new GameObject(nombre);
            obj.transform.SetParent(padre, false);
            if (obj.GetComponent<RectTransform>() == null) obj.AddComponent<RectTransform>();
            return obj;
        }

        private static void CrearFondo(string nombre, Vector2 pos, Vector2 escala, Color color, int order)
        {
            GameObject obj = Buscar(nombre) ?? new GameObject(nombre);
            obj.transform.position = pos;
            obj.transform.localScale = escala;
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>(); if (sr == null) sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Bloque.png");
            sr.color = color;
            sr.sortingOrder = order;
        }

        private static GameObject CrearBloque(string nombre, Vector2 pos, Vector2 escala, Color color, int layer, bool colision, int order)
        {
            GameObject obj = Buscar(nombre) ?? new GameObject(nombre);
            obj.layer = layer;
            obj.transform.position = pos;
            obj.transform.localScale = escala;
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>(); if (sr == null) sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Bloque.png");
            sr.color = color;
            sr.sortingOrder = order;
            if (colision && obj.GetComponent<BoxCollider2D>() == null) obj.AddComponent<BoxCollider2D>();
            return obj;
        }

        private static void ConfigRect(RectTransform rect, Vector2 pos, Vector2 size)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
        }

        private static GameObject Buscar(string nombre)
        {
            foreach (GameObject raiz in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                GameObject encontrado = BuscarHijo(raiz.transform, nombre);
                if (encontrado != null) return encontrado;
            }
            return null;
        }

        private static GameObject BuscarHijo(Transform actual, string nombre)
        {
            if (actual.name == nombre) return actual.gameObject;
            for (int i = 0; i < actual.childCount; i++)
            {
                GameObject r = BuscarHijo(actual.GetChild(i), nombre);
                if (r != null) return r;
            }
            return null;
        }
    }
}


