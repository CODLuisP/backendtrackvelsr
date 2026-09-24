using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System;
using System.Data;
using VelsatBackendAPI.Data.Services;

namespace VelsatBackendAPI.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly string _defaultConnectionString;
        private readonly string _secondConnectionString;
        private readonly string _thridConnectionString;

        private MySqlConnection _defaultConnection;
        private MySqlTransaction _defaultTransaction;

        private MySqlConnection _secondConnection;
        private MySqlTransaction _secondTransaction;

        private MySqlConnection _thirdConnection;
        private MySqlTransaction _thirdTransaction;


        // ✅ Usar Lazy<T> para thread-safety sin locks manuales
        private readonly Lazy<IUserRepository> _userRepository;
        private readonly Lazy<IDatosCargainicialService> _datosCargaInicialService;
        private readonly Lazy<IHistoricosRepository> _historicosRepository;
        private readonly Lazy<IKilometrosRepository> _kilometrosRepository;
        private readonly Lazy<IServidorRepository> _servidorRepository;
        private readonly Lazy<IAlertaRepository> _alertaRepository;
        private readonly Lazy<IAdminRepository> _adminRepository;
        private readonly Lazy<IGeocercaRepository> _geocercaRepository;
        private readonly Lazy<IGeocercasRepository> _geocercasRepository;
        private readonly Lazy<IGeocercasVehiculosRepository> _geocercasVehiculosRepository;

        private bool _disposed = false;
        private bool _committed = false;
        private readonly object _lockObject = new object();

        public UnitOfWork(MySqlConfiguration configuration)
        {
            _defaultConnectionString = configuration.DefaultConnection
                ?? throw new ArgumentNullException(nameof(configuration.DefaultConnection));
            _secondConnectionString = configuration.SecondConnection
                ?? throw new ArgumentNullException(nameof(configuration.SecondConnection));

            _thridConnectionString = configuration.ThirdConnection
        ?? throw new ArgumentNullException(nameof(configuration.ThirdConnection));

            // ✅ Inicializar Lazy para cada repositorio
            _userRepository = new Lazy<IUserRepository>(() =>
                new UserRepository(DefaultConnection, _defaultTransaction));

            _datosCargaInicialService = new Lazy<IDatosCargainicialService>(() =>
                new DatosCargainicialService(DefaultConnection, _defaultTransaction));

            _servidorRepository = new Lazy<IServidorRepository>(() =>
                new ServidorRepository(DefaultConnection, _defaultTransaction));

            _alertaRepository = new Lazy<IAlertaRepository>(() =>
                new AlertaRepository(DefaultConnection, _defaultTransaction));

            // Repositorios con ambas conexiones
            _historicosRepository = new Lazy<IHistoricosRepository>(() =>
                new HistoricosRepository(DefaultConnection, SecondConnection, _defaultTransaction, _secondTransaction));

            _kilometrosRepository = new Lazy<IKilometrosRepository>(() =>
                new KilometrosRepository(DefaultConnection, SecondConnection, _defaultTransaction, _secondTransaction));

            //ADMIN
            _adminRepository = new Lazy<IAdminRepository>(() => new AdminRepository(DefaultConnection, _defaultTransaction, ThirdConnection, _thirdTransaction));

            _geocercaRepository = new Lazy<IGeocercaRepository>(() => new GeocercaRepository(DefaultConnection, _defaultTransaction));

            _geocercasRepository = new Lazy<IGeocercasRepository>(() =>
                new GeocercasRepository(DefaultConnection, _defaultTransaction));

            _geocercasVehiculosRepository = new Lazy<IGeocercasVehiculosRepository>(() =>
                new GeocercasVehiculosRepository(DefaultConnection, _defaultTransaction));
        }

        // ✅ Conexión principal con inicialización thread-safe y retry logic
        private MySqlConnection DefaultConnection
        {
            get
            {
                ValidateNotDisposedOrCommitted();

                if (_defaultConnection == null)
                {
                    lock (_lockObject)
                    {
                        if (_defaultConnection == null)
                        {
                            // ✅ CAMBIO: Usar método con retry
                            _defaultConnection = OpenConnectionWithRetry(
                                _defaultConnectionString,
                                "DEFAULT (con transacción)");

                            // Iniciar transacción DESPUÉS de abrir la conexión exitosamente
                            _defaultTransaction = _defaultConnection.BeginTransaction();

                            System.Diagnostics.Debug.WriteLine(
                                $"[UnitOfWork] Transacción DEFAULT iniciada");
                        }
                    }
                }
                return _defaultConnection;
            }
        }

        // ✅ Conexión secundaria con retry logic
        private MySqlConnection SecondConnection
        {
            get
            {
                ValidateNotDisposedOrCommitted();

                if (_secondConnection == null)
                {
                    lock (_lockObject)
                    {
                        if (_secondConnection == null)
                        {
                            // ✅ CAMBIO: Usar método con retry
                            _secondConnection = OpenConnectionWithRetry(
                                _secondConnectionString,
                                "SECOND (con transacción)");

                            // Iniciar transacción DESPUÉS de abrir la conexión exitosamente
                            _secondTransaction = _secondConnection.BeginTransaction();

                            System.Diagnostics.Debug.WriteLine(
                                $"[UnitOfWork] Transacción SECOND iniciada");
                        }
                    }
                }
                return _secondConnection;
            }
        }

        private MySqlConnection ThirdConnection
        {
            get
            {
                ValidateNotDisposedOrCommitted();

                if (_thirdConnection == null)
                {
                    lock (_lockObject)
                    {
                        if (_thirdConnection == null)
                        {
                            // ✅ CAMBIO: Usar método con retry
                            _thirdConnection = OpenConnectionWithRetry(
                                _thridConnectionString,
                                "THIRD (con transacción)");

                            // Iniciar transacción DESPUÉS de abrir la conexión exitosamente
                            _thirdTransaction = _thirdConnection.BeginTransaction();

                            System.Diagnostics.Debug.WriteLine(
                                $"[UnitOfWork] Transacción THIRD iniciada");
                        }
                    }
                }
                return _thirdConnection;
            }
        }

        // ✅ Validación mejorada
        private void ValidateNotDisposedOrCommitted()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(UnitOfWork),
                    "No se puede usar un UnitOfWork que ya ha sido liberado. Crea una nueva instancia.");
            }

            if (_committed)
            {
                throw new InvalidOperationException(
                    "Este UnitOfWork ya fue confirmado con SaveChanges(). Crea una nueva instancia para realizar más operaciones.");
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
                        $"[UnitOfWork] ✅ Conexión {connectionName} " +
                        $"{connection.ServerThread} abierta con transacción" +
                        (attempt > 0 ? $" (intento {attempt + 1})" : ""));

                    return connection;
                }
                catch (ArgumentException ex) when (
                    ex.Message.Contains("An item with the same key has already been added"))
                {
                    lastException = ex;

                    System.Diagnostics.Debug.WriteLine(
                        $"[UnitOfWork] ⚠️ Pool collision detectada en {connectionName} " +
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
                                $"[UnitOfWork] Pool {connectionName} limpiado");
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
                        $"[UnitOfWork] ❌ Error abriendo {connectionName}: {ex.Message}");
                    throw;
                }
            }

            // Si llegamos aquí, fallaron todos los intentos
            throw new InvalidOperationException(
                $"No se pudo abrir la conexión {connectionName} después de {maxRetries} intentos. " +
                $"Pool de conexiones MySQL posiblemente corrupto.",
                lastException);
        }


        // ✅ Propiedades usando Lazy
        public IUserRepository UserRepository
        {
            get
            {
                ValidateNotDisposedOrCommitted();
                return _userRepository.Value;
            }
        }

        public IDatosCargainicialService DatosCargainicialService
        {
            get
            {
                ValidateNotDisposedOrCommitted();
                return _datosCargaInicialService.Value;
            }
        }

        public IServidorRepository ServidorRepository
        {
            get
            {
                ValidateNotDisposedOrCommitted();
                return _servidorRepository.Value;
            }
        }

        public IAlertaRepository AlertaRepository
        {
            get
            {
                ValidateNotDisposedOrCommitted();
                return _alertaRepository.Value;
            }
        }

        public IHistoricosRepository HistoricosRepository
        {
            get
            {
                ValidateNotDisposedOrCommitted();
                return _historicosRepository.Value;
            }
        }

        public IKilometrosRepository KilometrosRepository
        {
            get
            {
                ValidateNotDisposedOrCommitted();
                return _kilometrosRepository.Value;
            }
        }

        public IAdminRepository AdminRepository
        {
            get
            {
                ValidateNotDisposedOrCommitted();
                return _adminRepository.Value;
            }
        }

        public IGeocercaRepository GeocercaRepository
        {
            get
            {
                ValidateNotDisposedOrCommitted();
                return _geocercaRepository.Value;
            }
        }

        public IGeocercasRepository GeocercasRepository
        {
            get
            {
                ValidateNotDisposedOrCommitted();
                return _geocercasRepository.Value;
            }
        }

        public IGeocercasVehiculosRepository GeocercasVehiculosRepository
        {
            get
            {
                ValidateNotDisposedOrCommitted();
                return _geocercasVehiculosRepository.Value;
            }
        }

        // ✅ SaveChanges optimizado
        public void SaveChanges()
        {
            ValidateNotDisposedOrCommitted();

            lock (_lockObject)
            {
                try
                {
                    // ✅ Commit de las 3 transacciones
                    _defaultTransaction?.Commit();
                    _secondTransaction?.Commit();
                    _thirdTransaction?.Commit();

                    _committed = true;

                    System.Diagnostics.Debug.WriteLine(
                        "[UnitOfWork] ✅ Todas las transacciones confirmadas");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[UnitOfWork] ❌ Error en SaveChanges: {ex.Message}");

                    // ✅ Rollback de las 3 transacciones en caso de error
                    try { _defaultTransaction?.Rollback(); } catch { }
                    try { _secondTransaction?.Rollback(); } catch { }
                    try { _thirdTransaction?.Rollback(); } catch { }

                    throw;
                }
                finally
                {
                    DisposeTransactionsAndConnections();
                }
            }
        }

        // ✅ NUEVO: Método que libera transacciones Y conexiones inmediatamente
        private void DisposeTransactionsAndConnections()
        {
            // Liberar transacciones
            if (_defaultTransaction != null)
            {
                _defaultTransaction.Dispose();
                _defaultTransaction = null;
            }

            if (_secondTransaction != null)
            {
                _secondTransaction.Dispose();
                _secondTransaction = null;
            }

            // ✅ AGREGAR ESTE BLOQUE
            if (_thirdTransaction != null)
            {
                _thirdTransaction.Dispose();
                _thirdTransaction = null;
            }

            // Cerrar y disponer conexiones
            if (_defaultConnection != null)
            {
                try
                {
                    if (_defaultConnection.State == ConnectionState.Open)
                        _defaultConnection.Close();
                    _defaultConnection.Dispose();
                }
                catch { }
                finally { _defaultConnection = null; }
            }

            if (_secondConnection != null)
            {
                try
                {
                    if (_secondConnection.State == ConnectionState.Open)
                        _secondConnection.Close();
                    _secondConnection.Dispose();
                }
                catch { }
                finally { _secondConnection = null; }
            }

            // ✅ AGREGAR ESTE BLOQUE
            if (_thirdConnection != null)
            {
                try
                {
                    if (_thirdConnection.State == ConnectionState.Open)
                        _thirdConnection.Close();
                    _thirdConnection.Dispose();
                }
                catch { }
                finally { _thirdConnection = null; }
            }
        }

        // ✅ Dispose optimizado
        public void Dispose()
        {
            Dispose(true);
            // ✅ REMOVIDO el finalizer, así que esto ya no es necesario
            // GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                lock (_lockObject)
                {
                    try
                    {
                        if (!_committed)
                        {
                            try
                            {
                                if (_defaultTransaction != null && _defaultTransaction.Connection != null)
                                    _defaultTransaction.Rollback();
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"[UnitOfWork] Rollback default error: {ex.Message}");
                            }

                            try
                            {
                                if (_secondTransaction != null && _secondTransaction.Connection != null)
                                    _secondTransaction.Rollback();
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"[UnitOfWork] Rollback second error: {ex.Message}");
                            }
                        }

                        DisposeTransactionsAndConnections();
                        DisposeRepositories();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[UnitOfWork] Error disposing: {ex.Message}");
                    }
                    finally
                    {
                        _disposed = true;
                    }
                }
            }
        }

        // ✅ NUEVO: Liberar repositorios si implementan IDisposable
        private void DisposeRepositories()
        {
            // Solo disponer si fueron inicializados
            TryDisposeRepository(_userRepository);
            TryDisposeRepository(_datosCargaInicialService);
            TryDisposeRepository(_historicosRepository);
            TryDisposeRepository(_kilometrosRepository);
            TryDisposeRepository(_servidorRepository);
            TryDisposeRepository(_alertaRepository);
            TryDisposeRepository(_adminRepository);
            TryDisposeRepository(_geocercaRepository);
            TryDisposeRepository(_geocercasRepository);
            TryDisposeRepository(_geocercasVehiculosRepository);
        }

        private void TryDisposeRepository<T>(Lazy<T> lazyRepo)
        {
            if (lazyRepo != null && lazyRepo.IsValueCreated && lazyRepo.Value is IDisposable disposable)
            {
                try
                {
                    disposable.Dispose();
                }
                catch
                {
                    // Ignorar errores al disponer repositorios
                }
            }
        }

    }
}