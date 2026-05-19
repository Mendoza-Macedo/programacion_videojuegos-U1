# Proyecto cooperativo estilo Bread & Fred

Este proyecto fue armado para entender una base jugable de plataformas cooperativas 2D en Unity.
Todo lo principal esta escrito en español: nombres de carpetas, scripts, variables visibles y objetos de escena.

## Escena principal
Abre:
Assets/Escenas/EscenaCooperativa.unity

Tambien esta registrada en Build Settings como escena inicial.

## Controles
Jugador Azul:
- A / D: moverse
- W: saltar
- Shift izquierdo: agarrarse o soltarse

Jugador Rojo:
- Flecha izquierda / derecha: moverse
- Flecha arriba: saltar
- Shift derecho: agarrarse o soltarse

General:
- R: reiniciar la escena

## Mecanicas incluidas
- Dos jugadores con Rigidbody2D.
- Cuerda visual y fisica entre ambos jugadores.
- Camara cooperativa que sigue el punto medio.
- Menu principal.
- Pausa con Esc.
- Pantalla de victoria.
- Barra de tension de cuerda.
- Plataformas fijas.
- Plataforma movil.
- Obstaculo giratorio peligroso.
- Zona de muerte con reaparicion.
- Puntos de control.
- Meta que requiere que ambos jugadores lleguen.
- UI simple con tiempo e instrucciones.
- Mejor tiempo guardado con PlayerPrefs.
- Sonidos generados por codigo para salto, agarre, caida, checkpoint, dano y victoria.
- Escenario extendido con tutorial, montana principal y decoracion andina.

## Carpetas creadas
- Assets/Scripts/Personajes
- Assets/Scripts/Cuerda
- Assets/Scripts/Camara
- Assets/Scripts/Mundo
- Assets/Scripts/Juego
- Assets/Editor
- Assets/Escenas
- Assets/Prefabs
- Assets/Sprites
- Assets/Materiales

## Regenerar la escena
Si quieres reconstruir todo desde Unity:
Herramientas > BreadFred > Re construir escena completa

Ese menu vuelve a crear la escena, sprites, material y prefabs basicos.
## Seleccion de animales de la sierra
Al iniciar la escena aparece una pantalla para escoger animal.
La pantalla tiene dos tarjetas, una por jugador, con vista previa grande, nombre del animal y estado de confirmacion.

Animales disponibles:
- Vicuna
- Alpaca
- Llama
- Vizcacha
- Condor Andino

Controles de seleccion:
- Jugador Azul: A / D cambia animal, W confirma.
- Jugador Rojo: Flecha izquierda / derecha cambia animal, Flecha arriba confirma.
- Enter: empieza con la seleccion actual de ambos.

Cada animal tiene sprites para:
- Quieto
- Caminar 1
- Caminar 2
- Saltar
- Caer
- Agarrarse

Los sprites estan en:
Assets/Sprites/AnimalesSierra

La version visual actual usa sprites raster generados especialmente para el proyecto y recortados desde:
Assets/Sprites/AnimalesSierraIA/HojaAnimalesSierraIA.png

Previsualizacion de recortes:
Assets/Sprites/AnimalesSierraIA/PreviewRecortes.png

Los sprites procedurales anteriores quedaron respaldados en:
Assets/Sprites/AnimalesSierraProceduralBackup

Los sprites ahora tienen bordes, rasgos de especie y detalles visuales:
- Vicuna: pecho claro y cuerpo fino.
- Alpaca: lana mas redondeada.
- Llama: manta decorativa.
- Vizcacha: orejas largas y cola marcada.
- Condor Andino: alas, collar claro y pico.

Scripts nuevos:
- Assets/Scripts/Animales/AnimadorAnimalSierra.cs
- Assets/Scripts/Animales/SelectorAnimalesSierra.cs
- Assets/Editor/ConstructorAnimalesSierra.cs

Para regenerar o reconectar los animales desde Unity:
Herramientas > BreadFred > Agregar animales de la sierra

## Version completa v1
Para reconstruir todo el juego desde Unity:
Herramientas > BreadFred > Construir juego completo v1

Incluye:
- Menu principal: Enter o Espacio para empezar.
- Seleccion de animales.
- Pausa: Esc.
- Reinicio: R.
- Barra de tension de cuerda.
- Nivel tutorial y montana extendida.
- Fondo de montanas andinas, ichu decorativo y cima final.
- Pantalla de victoria con mejor tiempo.

## Ejecutable generado
El ejecutable Windows esta en:
Builds/AnimalesDeLaCumbre/AnimalesDeLaCumbre.exe

Si quieres regenerarlo desde Unity:
Herramientas > BreadFred > Generar ejecutable Windows

