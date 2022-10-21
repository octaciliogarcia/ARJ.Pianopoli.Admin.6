using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ARJ.Pianopoli.Admin._6.Data
{
    public partial class DBContext
    {
        private DBContextProcedures _procedures;

        public virtual DBContextProcedures Procedures
        {
            get
            {
                if (_procedures is null) _procedures = new DBContextProcedures(this);
                return _procedures;
            }
            set
            {
                _procedures = value;
            }
        }

        public DBContextProcedures GetProcedures()
        {
            return Procedures;
        }

        protected void OnModelCreatingGeneratedProcedures(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SP_LISTAR_LOTESResult>().HasNoKey().ToView(null);
        }
    }
    public interface IDatabaseContextProceduresContract
    {
        Task<List<SP_LISTAR_LOTESResult>> SP_LISTAR_LOTESAsync(int? Loteamento, CancellationToken cancellationToken = default);
    }
    public partial class DBContextProcedures
    {
        private readonly DBContext _context;

        public DBContextProcedures(DBContext context)
        {
            _context = context;
        }

        public virtual async Task<List<SP_LISTAR_LOTESResult>> SP_LISTAR_LOTESAsync(int? Loteamento, OutputParameter<int> returnValue = null, CancellationToken cancellationToken = default)
        {
            var parameterreturnValue = new SqlParameter
            {
                ParameterName = "returnValue",
                Direction = System.Data.ParameterDirection.Output,
                SqlDbType = System.Data.SqlDbType.Int,
            };

            var sqlParameters = new[]
            {
                new SqlParameter
                {
                    ParameterName = "Loteamento",
                    Value = Loteamento ?? Convert.DBNull,
                    SqlDbType = System.Data.SqlDbType.Int,
                },
                parameterreturnValue,
            };
            var _ = await _context.SqlQueryAsync<SP_LISTAR_LOTESResult>("EXEC @returnValue = [dbo].[SP_LISTAR_LOTES] @Loteamento", sqlParameters, cancellationToken);

            returnValue?.SetValue(parameterreturnValue.Value);

            return _;
        }
    }
}
