using Microsoft.EntityFrameworkCore;

namespace ARJ.Pianopoli.Admin._6.Data
{
    public partial class DBContext
    {
        [DbFunction("LPAD", "dbo")]
        public static string LPAD(string cstring, int? tam, string caracter)
        {
            throw new NotSupportedException("This method can only be called from Entity Framework Core queries");
        }

        [DbFunction("ttod", "dbo")]
        public static DateTime? ttod(DateTime? p_datetime)
        {
            throw new NotSupportedException("This method can only be called from Entity Framework Core queries");
        }
        protected void OnModelCreatingGeneratedFunctions(ModelBuilder modelBuilder)
        {
        }
    }

}
