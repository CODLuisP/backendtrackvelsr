using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelsatBackendAPI.Model;
using VelsatBackendAPI.Model.Geocerca;

namespace VelsatBackendAPI.Data.Repositories
{
    public class GeocercaRepository : IGeocercaRepository
    {
        private readonly IDbConnection _defaultConnection; IDbTransaction _defaultTransaction;

        public GeocercaRepository(IDbConnection defaultconnection, IDbTransaction defaulttransaction)
        {
            _defaultConnection = defaultconnection;
            _defaultTransaction = defaulttransaction;
        }

        public async Task<IEnumerable<Geocerca>> GetGeocercas(string usuario)
        {
            var sql = @"SELECT * FROM geocerca WHERE usuario = @Usuario";

            var resultado = await _defaultConnection.QueryAsync<Geocerca>(sql, new { Usuario = usuario }, transaction: _defaultTransaction);
            return resultado;
        }

        public async Task<int> InsertGeocerca(Geocerca geocerca)
        {
            var sql = @"INSERT INTO geocerca (usuario, nombre, tipo, radio, puntoorigen, segundopunto, tercerpunto, puntofinal) VALUES (@Usuario, @Nombre, @Tipo, @Radio, @Puntoorigen, @Segundopunto, @Tercerpunto, @Puntofinal)";

            var resultado = await _defaultConnection.ExecuteAsync(sql, geocerca, transaction: _defaultTransaction);
            return resultado;
        }

        public async Task<int> DeleteGeocerca(int id)
        {
            var sql = @"DELETE FROM geocerca WHERE idgeocerca = @Id";

            var parametros = new { Id = id };

            var resultado = await _defaultConnection.ExecuteAsync(sql, parametros, transaction: _defaultTransaction);
            return resultado;
        }
    }
}
