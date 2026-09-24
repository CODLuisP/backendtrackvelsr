using MySqlConnector;
using System;
using System.Data;
using System.Data.Common;
using VelsatBackendAPI.Data.Services;

namespace VelsatBackendAPI.Data.Repositories
{
    /// <summary>
    /// UnitOfWork optimizado SOLO para operaciones de lectura.
    /// NO inicia transacciones - ideal para SignalR y consultas de solo lectura.
    /// </summary>
    public class ReadOnlyUnitOfWork : IReadOnlyUnitOfWork
    {
        private readonly string _defaultConnectionString;
        private readonly string _secondConnectionString;

        private MySqlConnection _defaultConnection;
        private MySqlConnection _secondConnection;

        private readonly Lazy<IDatosCargainicialService> _datosCargaInicialService;
        private readonly Lazy<IServidorRepository> _servidorRepository;
        private readonly Lazy<IHistoricosRepository> _historicosRepository;
        private readonly Lazy<IKilometrosRepository> _kilometrosRepository;
        private readonly Lazy<IUserRepository> _userRepository;
        private readonly Lazy<IAdminRepository> _adminRepository;
        private readonly Lazy<IGeocercaRepository> _geocercaRepository;
        private readonly Lazy<IGeocercasRepository> _geocercasRepository;
        private readonly Lazy<IGeocercasVehiculosRepository> _geocercasVehiculosRepository;

        private bool _disposed = false;
        private readonly object _lockObject = new object();

        public ReadOnlyUnitOfWork(MySqlConfiguration configuration)
        {
            _defaultConnectionString = configuration.DefaultConnection
                ?? throw new ArgumentNullException(nameof(configuration.DefaultConnection));

            _secondConnectionString = configuration.SecondConnection // ✅ NUEVO
                ?? throw new ArgumentNullException(nameof(configuration.SecondConnection));


            // ✅ Inicializar servicio SIN transacción (segundo parámetro = null)
            _datosCargaInicialService = new Lazy<IDatosCargainicialService>(() => new DatosCargainicialService(DefaultConnection, null));
            _servidorRepository = new Lazy<IServidorRepository>(() => new ServidorRepository(DefaultConnection, null));
            _historicosRepository = new Lazy<IHistoricosRepository>(() => new HistoricosRepository(DefaultConnection, SecondConnection, null, null));
            _kilometrosRepository = new Lazy<IKilometrosRepository>(() => new KilometrosRepository(DefaultConnection, SecondConnection, null, null));
            _userRepository = new Lazy<IUserRepository>(() => new UserRepository(DefaultConnection, null));

            //ADMIN
            _adminRepository = new Lazy<IAdminRepository>(() => new AdminRepository(DefaultConnection, null, null, null));

            _geocercaRepository = new Lazy<IGeocercaRepository>(() => new GeocercaRepository(DefaultConnection, null));

            _geocercasRepository = new Lazy<IGeocercasRepository>(() => new GeocercasRepository(DefaultConnection, null));

            _geocercasVehiculosRepository = new Lazy<IGeocercasVehiculosRepository>(() => new GeocercasVehiculosRepository(DefaultConnection, null));

        }

        private MySqlConnection DefaultConnection
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(ReadOnlyUnitOfWork));

                if (_defaultConnection == null || _defaultConnection.State != ConnectionState.Open)
                {
                    lock (_lockObject)
                    {
                        if (_defaultConnection == null || _defaultConnection.State != ConnectionState.Open)
                        {
                            // ✅ CAMBIO: Usar método con retry
                            _defaultConnection = OpenConnectionWithRetry(
                                _defaultConnectionString,
                                "DEFAULT");
                        }
                    }
                }
                return _defaultConnection;
            }
        }

        // ✅ NUEVA: Conexión secundaria
        private MySqlConnection SecondConnection
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(ReadOnlyUnitOfWork));

                if (_secondConnection == null || _secondConnection.State != ConnectionState.Open)
                {
                    lock (_lockObject)
                    {
                        if (_secondConnection == null || _secondConnection.State != ConnectionState.Open)
                        {
                            // ✅ CAMBIO: Usar método con retry
                            _secondConnection = OpenConnectionWithRetry(
                                _secondConnectionString,
                                "SECOND");
                        }
                    }
                }
                return _secondConnection;
            }
        }

        public IDatosCargainicialService DatosCargainicialService
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(ReadOnlyUnitOfWork));
                return _datosCargaInicialService.Value;
            }
        }

        public IServidorRepository ServidorRepository
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(ReadOnlyUnitOfWork));
                return _servidorRepository.Value;
            }
        }

        public IHistoricosRepository HistoricosRepository
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(ReadOnlyUnitOfWork));
                return _historicosRepository.Value;
            }
        }

        public IKilometrosRepository KilometrosRepository
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(ReadOnlyUnitOfWork));
                return _kilometrosRepository.Value;
            }
        }

        public IUserRepository UserRepository
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(ReadOnlyUnitOfWork));
                return _userRepository.Value;
            }
        }

        public IAdminRepository AdminRepository
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(ReadOnlyUnitOfWork));
                return _adminRepository.Value;
            }
        }

        public IGeocercaRepository GeocercaRepository
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(ReadOnlyUnitOfWork));
                return _geocercaRepository.Value;
            }
        }

        public IGeocercasRepository GeocercasRepository
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(ReadOnlyUnitOfWork));
                return _geocercasRepository.Value;
            }
        }

        public IGeocercasVehiculosRepository GeocercasVehiculosRepository
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(ReadOnlyUnitOfWork));
                return _geocercasVehiculosRepository.Value;
            }
        }

        /// <summary>
        /// Abre una conexión MySQL con reintentos automáticos en caso de colisión de pool.
        /// </summary>
        private MySqlConnection OpenConnectionWithRetry(
            string connectionString,
            string connectionName,
            int maxRetries = 5)
        {
            Exception lastException = null;

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    var connection = new MySqlConnection(connectionString);
                    connection.Open();

                    // ✅ CRÍTICO: Configurar charset UTF-8 inmediatamente después de abrir
                    using (var cmd = new MySqlCommand("SET NAMES utf8mb4 COLLATE utf8mb4_unicode_ci", connection))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    System.Diagnostics.Debug.WriteLine(
                        $"[ReadOnlyUnitOfWork] ✅ Conexión {connectionName} " +
                        $"{connection.ServerThread} abierta" +
                        (attempt > 0 ? $" (intento {attempt + 1})" : ""));

                    return connection;
                }
                catch (ArgumentException ex) when (
                    ex.Message.Contains("An item with the same key has already been added"))
                {
                    lastException = ex;

                    System.Diagnostics.Debug.WriteLine(
                        $"[ReadOnlyUnitOfWork] ⚠️ Pool collision detectada en {connectionName} " +
                        $"(intento {attempt + 1}/{maxRetries})");

                    if (attempt < maxRetries - 1)
                    {
                        // Backoff exponencial: 10ms, 20ms, 40ms, 80ms, 160ms
                        int delayMs = 10 * (int)Math.Pow(2, attempt);
                        System.Threading.Thread.Sleep(delayMs);

                        // ✅ CRÍTICO: Intentar limpiar el pool antes de reintentar
                        try
                        {
                            MySqlConnection.ClearPool(new MySqlConnection(connectionString));
                            System.Diagnostics.Debug.WriteLine(
                                $"[ReadOnlyUnitOfWork] Pool {connectionName} limpiado");
                        }
                        catch
                        {
                            // Ignorar errores al limpiar
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Otros errores no relacionados con el pool - fallar inmediatamente
                    System.Diagnostics.Debug.WriteLine(
                        $"[ReadOnlyUnitOfWork] ❌ Error abriendo {connectionName}: {ex.Message}");
                    throw;
                }
            }

            // Si llegamos aquí, fallaron todos los intentos
            throw new InvalidOperationException(
                $"No se pudo abrir la conexión {connectionName} después de {maxRetries} intentos. " +
                $"Pool de conexiones MySQL posiblemente corrupto.",
                lastException);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            lock (_lockObject)
            {
                try
                {
                    // Cerrar conexión DEFAULT
                    if (_defaultConnection != null)
                    {
                        var connectionId = _defaultConnection.ServerThread;

                        if (_defaultConnection.State == ConnectionState.Open)
                            _defaultConnection.Close();

                        _defaultConnection.Dispose();

                        System.Diagnostics.Debug.WriteLine(
                            $"[ReadOnlyUnitOfWork] Conexión DEFAULT {connectionId} cerrada");
                    }

                    // ✅ Cerrar conexión SECOND
                    if (_secondConnection != null)
                    {
                        var connectionId = _secondConnection.ServerThread;

                        if (_secondConnection.State == ConnectionState.Open)
                            _secondConnection.Close();

                        _secondConnection.Dispose();

                        System.Diagnostics.Debug.WriteLine(
                            $"[ReadOnlyUnitOfWork] Conexión SECOND {connectionId} cerrada");
                    }

                    // Disponer servicios si fueron creados
                    if (_datosCargaInicialService.IsValueCreated &&
                        _datosCargaInicialService.Value is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[ReadOnlyUnitOfWork] Error disposing: {ex.Message}");
                }
                finally
                {
                    _defaultConnection = null;
                    _secondConnection = null;
                    _disposed = true;
                }
            }
        }
    }
}