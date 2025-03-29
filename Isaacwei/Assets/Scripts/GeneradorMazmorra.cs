using System.Collections.Generic;
using System.Linq;
using NavMeshPlus.Components;
using UnityEngine;

public class DungeonRoom
{
    public int x, y;
    public List<ROOM_DIRECTIONS> neighbours = new List<ROOM_DIRECTIONS>();
    public DungeonType Type { get; private set; } = DungeonType.Normal; // Por defecto es normal

    public DungeonRoom(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public void SetType(DungeonType type)
    {
        this.Type = type;
    }



    public void AddNeighbourInDirection(DungeonRoom neighbour, ROOM_DIRECTIONS direction)
    {
        if (!neighbours.Contains(direction))
        {
            neighbours.Add(direction);
        }

        // Asegurar que la otra sala también nos registre como vecino en la dirección opuesta
        ROOM_DIRECTIONS oppositeDirection = GetOppositeDirection(direction);
        if (!neighbour.neighbours.Contains(oppositeDirection))
        {
            neighbour.neighbours.Add(oppositeDirection);
        }
    }

    private ROOM_DIRECTIONS GetOppositeDirection(ROOM_DIRECTIONS direction)
    {
        switch (direction)
        {
            case ROOM_DIRECTIONS.Up: return ROOM_DIRECTIONS.Down;
            case ROOM_DIRECTIONS.Down: return ROOM_DIRECTIONS.Up;
            case ROOM_DIRECTIONS.Left: return ROOM_DIRECTIONS.Right;
            case ROOM_DIRECTIONS.Right: return ROOM_DIRECTIONS.Left;
            default: return direction;
        }
    }


    public int NeighboursCount
    {
        get { return neighbours.Count; }
    }
}

public enum ROOM_DIRECTIONS
{
    Up,
    Down,
    Left,
    Right
}

public class GeneradorMazmorra : MonoBehaviour
{
    public int maxRooms = 20; // Máximo de salas a generar
    public int minRooms = 6;
    private List<DungeonRoom> _dungeonRooms;
    private Queue<DungeonRoom> _pendingRooms;
    private int nCurrentRooms;

    private NavMeshSurface navMeshSurface;
    // Diccionario para almacenar las instancias de las salas generadas por sus coordenadas
    private Dictionary<Vector2Int, GameObject> instantiatedRooms = new Dictionary<Vector2Int, GameObject>();

    // Prefabs de las salas cargados desde Resources/Salas
    private GameObject[] salasPrefabs;

    void Start()
    {
        // Cargar los prefabs de las salas desde la carpeta Resources/Salas
        salasPrefabs = Resources.LoadAll<GameObject>("Salas");

        if (salasPrefabs.Length == 0)
        {
            Debug.LogError("❌ No se encontraron salas en la carpeta Resources/Salas.");
            return;
        }

        // Encontrar el objeto 'navmesh' en la escena y obtener el componente NavMeshSurface
        GameObject navMeshObject = GameObject.Find("Navmesh"); // Busca el objeto llamado "navmesh"
        if (navMeshObject != null)
        {
            navMeshSurface = navMeshObject.GetComponent<NavMeshSurface>(); // Obtén el componente NavMeshSurface
            if (navMeshSurface == null)
            {
                Debug.LogError("❌ El objeto 'navmesh' no tiene el componente NavMeshSurface.");
                return;
            }
        }
        else
        {
            Debug.LogError("❌ No se encontró el objeto 'navmesh' en la escena.");
            return;
        }

        // Generar la mazmora
        GenerateDungeonLayout();
        ConnectAdjacentRooms();
        EnsureSingleEntranceRooms(2);
        GenerateDungeonRooms();
        AddSpecialRooms();
        navMeshSurface.BuildNavMesh();
    }

    private void GenerateDungeonLayout()
    {
        _dungeonRooms = new List<DungeonRoom>();
        nCurrentRooms = 0;
        _pendingRooms = new Queue<DungeonRoom>();

        // Crear la sala inicial
        DungeonRoom startRoom = new DungeonRoom(0, 0);
        _pendingRooms.Enqueue(startRoom);
        _dungeonRooms.Add(startRoom);

        // Generar la mazmorra normalmente
        while (_pendingRooms.Count > 0)
        {
            nCurrentRooms++;
            DungeonRoom currentRoom = _pendingRooms.Dequeue();

            int nNeighbours = (nCurrentRooms + _pendingRooms.Count < maxRooms) ? Random.Range(1, 4) : 0;

            for (int i = 0; i < nNeighbours; ++i)
            {
                if (currentRoom.NeighboursCount < 4)
                {
                    ROOM_DIRECTIONS newNeighbourDirection = GetRandomNeighbourDirection(currentRoom);
                    (DungeonRoom, bool) newNeighbour = GenerateNeighbour(currentRoom, newNeighbourDirection);
                    DungeonRoom newNeighbourRoom = newNeighbour.Item1;
                    bool neighbourJustCreated = newNeighbour.Item2;

                    currentRoom.AddNeighbourInDirection(newNeighbourRoom, newNeighbourDirection);

                    if (neighbourJustCreated)
                    {
                        _pendingRooms.Enqueue(newNeighbourRoom);
                        _dungeonRooms.Add(newNeighbourRoom);
                    }
                }
            }
        }

        // Si el número de salas es menor al mínimo, seguir generando hasta alcanzarlo
        while (_dungeonRooms.Count < minRooms)
        {
            Debug.LogWarning($"🔴 Añadiendo más salas... (Actual: {_dungeonRooms.Count}, Mínimo: {minRooms})");

            DungeonRoom extraRoom = GetRandomExistingRoom();
            ROOM_DIRECTIONS newNeighbourDirection = GetAvailableNeighbourDirection(extraRoom);

            if (newNeighbourDirection != ROOM_DIRECTIONS.Up) // Asegura que haya una dirección válida
            {
                (DungeonRoom, bool) newNeighbour = GenerateNeighbour(extraRoom, newNeighbourDirection);
                DungeonRoom newNeighbourRoom = newNeighbour.Item1;
                bool neighbourJustCreated = newNeighbour.Item2;

                if (neighbourJustCreated)
                {
                    extraRoom.AddNeighbourInDirection(newNeighbourRoom, newNeighbourDirection);
                    _dungeonRooms.Add(newNeighbourRoom);
                }
            }
        }

        Debug.Log($"✅ DUNGEON GENERATED WITH {_dungeonRooms.Count} ROOMS (Min: {minRooms}, Max: {maxRooms})");
    }

    // Método para encontrar una sala existente aleatoria
    private DungeonRoom GetRandomExistingRoom()
    {
        return _dungeonRooms[Random.Range(0, _dungeonRooms.Count)];
    }

    // Método para encontrar una dirección disponible para una nueva habitación
    private ROOM_DIRECTIONS GetAvailableNeighbourDirection(DungeonRoom room)
    {
        List<ROOM_DIRECTIONS> availableDirections = new List<ROOM_DIRECTIONS>();

        if (!_dungeonRooms.Exists(r => r.x == room.x && r.y == room.y + 1)) availableDirections.Add(ROOM_DIRECTIONS.Up);
        if (!_dungeonRooms.Exists(r => r.x == room.x && r.y == room.y - 1)) availableDirections.Add(ROOM_DIRECTIONS.Down);
        if (!_dungeonRooms.Exists(r => r.x == room.x - 1 && r.y == room.y)) availableDirections.Add(ROOM_DIRECTIONS.Left);
        if (!_dungeonRooms.Exists(r => r.x == room.x + 1 && r.y == room.y)) availableDirections.Add(ROOM_DIRECTIONS.Right);

        if (availableDirections.Count > 0)
        {
            return availableDirections[Random.Range(0, availableDirections.Count)];
        }

        return ROOM_DIRECTIONS.Up; // Default para evitar errores, aunque no debería ocurrir
    }


    private void ConnectAdjacentRooms()
    {
        foreach (var room in _dungeonRooms)
        {
            foreach (ROOM_DIRECTIONS direction in System.Enum.GetValues(typeof(ROOM_DIRECTIONS)))
            {
                // Obtener la sala vecina en la dirección dada
                DungeonRoom neighbour = _dungeonRooms.Find(r =>
                    r.x == room.x + (direction == ROOM_DIRECTIONS.Right ? 1 : direction == ROOM_DIRECTIONS.Left ? -1 : 0) &&
                    r.y == room.y + (direction == ROOM_DIRECTIONS.Up ? 1 : direction == ROOM_DIRECTIONS.Down ? -1 : 0)
                );

                // Si hay una sala en esa posición y aún no es vecina, la conectamos
                if (neighbour != null && !room.neighbours.Contains(direction))
                {
                    room.AddNeighbourInDirection(neighbour, direction);
                    neighbour.AddNeighbourInDirection(room, GetOppositeDirection(direction));
                }
            }
        }
    }

    // Método auxiliar para obtener la dirección opuesta
    private ROOM_DIRECTIONS GetOppositeDirection(ROOM_DIRECTIONS direction)
    {
        switch (direction)
        {
            case ROOM_DIRECTIONS.Up: return ROOM_DIRECTIONS.Down;
            case ROOM_DIRECTIONS.Down: return ROOM_DIRECTIONS.Up;
            case ROOM_DIRECTIONS.Left: return ROOM_DIRECTIONS.Right;
            case ROOM_DIRECTIONS.Right: return ROOM_DIRECTIONS.Left;
            default: return direction;
        }
    }


    // Obtener una dirección aleatoria para un vecino
    private ROOM_DIRECTIONS GetRandomNeighbourDirection(DungeonRoom room)
    {
        return (ROOM_DIRECTIONS)Random.Range(0, 4); // Aleatorio entre Up, Down, Left, Right
    }

    private (DungeonRoom, bool) GenerateNeighbour(DungeonRoom currentRoom, ROOM_DIRECTIONS direction)
    {
        int newX = currentRoom.x;
        int newY = currentRoom.y;

        switch (direction)
        {
            case ROOM_DIRECTIONS.Up: newY += 1; break;
            case ROOM_DIRECTIONS.Down: newY -= 1; break;
            case ROOM_DIRECTIONS.Left: newX -= 1; break;
            case ROOM_DIRECTIONS.Right: newX += 1; break;
        }

        // Verificar si la sala ya existe en la lista
        DungeonRoom existingRoom = _dungeonRooms.Find(r => r.x == newX && r.y == newY);

        if (existingRoom != null)
        {
            return (existingRoom, false); // La sala ya existe, no creamos una nueva
        }

        // Si no existe, creamos una nueva y la añadimos a la lista
        DungeonRoom newNeighbourRoom = new DungeonRoom(newX, newY);
        _dungeonRooms.Add(newNeighbourRoom);

        return (newNeighbourRoom, true);
    }


    private void GenerateDungeonRooms()
    {
        // Verificamos si los prefabs están cargados correctamente
        if (salasPrefabs.Length == 0)
        {
            Debug.LogError("❌ No se encontraron salas en la carpeta Resources/Salas.");
            return;
        }

        // Colocamos las salas basadas en el layout generado
        foreach (var room in _dungeonRooms)
        {
            Vector2Int position = new Vector2Int(room.x, room.y);

            // Verificar que no haya ninguna sala instanciada en esa posición
            if (!instantiatedRooms.ContainsKey(position))
            {
                // Revisamos las direcciones de los vecinos para saber qué puertas necesitamos
                bool tienePuertaArriba = false;
                bool tienePuertaAbajo = false;
                bool tienePuertaIzquierda = false;
                bool tienePuertaDerecha = false;

                // Recorremos los vecinos de la sala y marcamos las puertas necesarias
                foreach (var neighbour in room.neighbours)
                {
                    switch (neighbour)
                    {
                        case ROOM_DIRECTIONS.Up:
                            tienePuertaArriba = true;
                            break;
                        case ROOM_DIRECTIONS.Down:
                            tienePuertaAbajo = true;
                            break;
                        case ROOM_DIRECTIONS.Left:
                            tienePuertaIzquierda = true;
                            break;
                        case ROOM_DIRECTIONS.Right:
                            tienePuertaDerecha = true;
                            break;
                    }
                }

                // Agregamos un log para ver qué puertas estamos buscando
                Debug.Log($"Buscando prefab para sala en ({room.x}, {room.y}) con puertas: Arriba={tienePuertaArriba}, Abajo={tienePuertaAbajo}, Izquierda={tienePuertaIzquierda}, Derecha={tienePuertaDerecha}");

                // Buscar el prefab adecuado que tenga las puertas correspondientes
                GameObject roomPrefab = null;

                // Recorremos todos los prefabs para encontrar el adecuado
                foreach (var prefab in salasPrefabs)
                {
                    Sala salaScript = prefab.GetComponent<Sala>();

                    if (salaScript != null)
                    {
                        // Agregamos un log para mostrar las puertas del prefab que estamos comprobando
                        Debug.Log($"Comprobando prefab {prefab.name}: Arriba={salaScript.TienePuertaArriba}, Abajo={salaScript.TienePuertaAbajo}, Izquierda={salaScript.TienePuertaIzquierda}, Derecha={salaScript.TienePuertaDerecha}");

                        // Comprobamos si el prefab tiene las puertas necesarias
                        if (salaScript.TienePuertaArriba == tienePuertaArriba &&
                            salaScript.TienePuertaAbajo == tienePuertaAbajo &&
                            salaScript.TienePuertaIzquierda == tienePuertaIzquierda &&
                            salaScript.TienePuertaDerecha == tienePuertaDerecha)
                        {
                            roomPrefab = prefab;
                            break; // Si encontramos el prefab adecuado, salimos del bucle
                        }
                    }
                }

                // Si encontramos un prefab adecuado, lo instanciamos
                if (roomPrefab != null)
                {
                    GameObject roomObject = Instantiate(roomPrefab, new Vector3(room.x * 25, room.y * 11, 0), Quaternion.identity);
                    instantiatedRooms[position] = roomObject;
                }
                else
                {
                    Debug.LogError($"❌ No se encontró un prefab adecuado para la sala en la posición ({room.x}, {room.y})");
                }
            }
        }
    }

    private void AddSpecialRooms()
    {
        List<DungeonRoom> singleEntranceRooms = _dungeonRooms.FindAll(r => r.NeighboursCount == 1);

        if (singleEntranceRooms.Count < 2)
        {
            Debug.LogError("❌ No hay suficientes salas con una sola entrada para asignar salas especiales.");
            return;
        }

        // 🔥 Sala del Jefe: La más alejada del inicio (0,0)
        DungeonRoom bossRoom = singleEntranceRooms
            .OrderByDescending(r => Mathf.Abs(r.x) + Mathf.Abs(r.y))
            .First();

        // 🔥 Sala del Tesoro: La más cercana al inicio, pero NO en (0,0) y diferente al jefe
        DungeonRoom treasureRoom = singleEntranceRooms
            .Where(r => r != bossRoom && !(r.x == 0 && r.y == 0)) // Excluir el inicio y la sala del jefe
            .OrderBy(r => Mathf.Abs(r.x) + Mathf.Abs(r.y))
            .FirstOrDefault();

        // Verificación final
        if (bossRoom == null || treasureRoom == null)
        {
            Debug.LogError("❌ ERROR: No se pudieron asignar correctamente las salas especiales.");
            return;
        }

        Debug.Log($"💀 Sala del jefe en ({bossRoom.x}, {bossRoom.y})");
        Debug.Log($"💎 Sala del tesoro en ({treasureRoom.x}, {treasureRoom.y})");

        // Marcar las salas con DungeonType
        bossRoom.SetType(DungeonType.Boss);
        treasureRoom.SetType(DungeonType.Treasure);
    }

    

    private void EnsureSingleEntranceRooms(int minRequired)
    {
        int attempts = 0;
        while (CountSingleEntranceRooms() < minRequired && attempts < 10) // Evita bucles infinitos
        {
            AddExtraSingleEntranceRoom();
            attempts++;
        }
    }

    // 🔢 Cuenta cuántas salas tienen solo una entrada
    private int CountSingleEntranceRooms()
    {
        int count = 0;
        foreach (var room in _dungeonRooms)
        {
            if (room.NeighboursCount == 1)
            {
                count++;
            }
        }
        return count;
    }

    // ➕ Añade una sala con solo una entrada a la mazmorra
    private void AddExtraSingleEntranceRoom()
    {
        List<DungeonRoom> candidates = new List<DungeonRoom>();

        // Buscar salas que puedan generar un nuevo vecino con solo una conexión
        foreach (var room in _dungeonRooms)
        {
            foreach (ROOM_DIRECTIONS direction in System.Enum.GetValues(typeof(ROOM_DIRECTIONS)))
            {
                Vector2Int newRoomPos = GetNewRoomPosition(room, direction);
                if (!_dungeonRooms.Exists(r => r.x == newRoomPos.x && r.y == newRoomPos.y)) // No existe ya
                {
                    candidates.Add(new DungeonRoom(newRoomPos.x, newRoomPos.y));
                }
            }
        }

        if (candidates.Count > 0)
        {
            DungeonRoom newRoom = candidates[Random.Range(0, candidates.Count)];
            _dungeonRooms.Add(newRoom);

            // Conectamos la nueva sala con su vecino más cercano
            DungeonRoom neighbour = FindNeighbour(newRoom);
            if (neighbour != null)
            {
                ROOM_DIRECTIONS direction = GetDirectionToNeighbour(newRoom, neighbour);
                newRoom.AddNeighbourInDirection(neighbour, direction);
                neighbour.AddNeighbourInDirection(newRoom, GetOppositeDirection(direction));
            }

            Debug.Log($"🟩 Se agregó una sala extra en ({newRoom.x}, {newRoom.y}) con solo una entrada.");
        }
    }

    // 📍 Encuentra la primera sala vecina de una nueva sala
    private DungeonRoom FindNeighbour(DungeonRoom room)
    {
        foreach (var existingRoom in _dungeonRooms)
        {
            if (Mathf.Abs(existingRoom.x - room.x) + Mathf.Abs(existingRoom.y - room.y) == 1)
            {
                return existingRoom;
            }
        }
        return null;
    }

    // 📌 Determina la dirección del vecino
    private ROOM_DIRECTIONS GetDirectionToNeighbour(DungeonRoom from, DungeonRoom to)
    {
        if (to.x == from.x && to.y == from.y + 1) return ROOM_DIRECTIONS.Up;
        if (to.x == from.x && to.y == from.y - 1) return ROOM_DIRECTIONS.Down;
        if (to.x == from.x + 1 && to.y == from.y) return ROOM_DIRECTIONS.Right;
        if (to.x == from.x - 1 && to.y == from.y) return ROOM_DIRECTIONS.Left;
        return ROOM_DIRECTIONS.Up; // Default
    }


    // 📍 Devuelve la posición de la nueva sala en la dirección dada
    private Vector2Int GetNewRoomPosition(DungeonRoom room, ROOM_DIRECTIONS direction)
    {
        switch (direction)
        {
            case ROOM_DIRECTIONS.Up: return new Vector2Int(room.x, room.y + 1);
            case ROOM_DIRECTIONS.Down: return new Vector2Int(room.x, room.y - 1);
            case ROOM_DIRECTIONS.Left: return new Vector2Int(room.x - 1, room.y);
            case ROOM_DIRECTIONS.Right: return new Vector2Int(room.x + 1, room.y);
        }
        return new Vector2Int(room.x, room.y);
    }


}
