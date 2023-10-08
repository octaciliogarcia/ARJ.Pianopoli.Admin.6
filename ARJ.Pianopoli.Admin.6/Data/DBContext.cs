#nullable disable
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ARJ.Pianopoli.Admin._6.Data
{
    public partial class DBContext : DbContext
    {
        public DBContext()
        {
        }

        public DBContext(DbContextOptions<DBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Imobiliaria> Imobiliarias { get; set; }
        public virtual DbSet<Corretor> Corretores { get; set; }
        public virtual DbSet<Loteamento> Loteamentos { get; set; }
        public virtual DbSet<Lotes> Lotes { get; set; }
        public virtual DbSet<LotesDescritivos> LotesDescritivos { get; set; }
        public virtual DbSet<LoteCategoria> LoteCategorias { get; set; }
        public virtual DbSet<TabelaPreco> TabelaPrecos { get; set; }
        public virtual DbSet<TabelaPrecoLotes> TabelaPrecoLotes { get; set; }
        public virtual DbSet<Comprador> Comprador { get; set; }
        public virtual DbSet<Proposta> Propostas { get; set; }
        public virtual DbSet<PropostaCondicaoComercial> PropostasCondicoesComerciais { get; set; }
        public virtual DbSet<PropostaAnexo> PropostaAnexo { get; set; }
        public virtual DbSet<TabelaPrecosPlanos> TabelaPrecosPlanos { get; set; }
        public virtual DbSet<TabelaM2> TabelaM2 { get; set; }
        public virtual DbSet<EstadoCivil> EstadoCivil { get; set; }
        public virtual DbSet<PropostasCompradores> PropostasCompradores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("dbo")
                .HasAnnotation("Relational:Collation", "Latin1_General_CI_AI");
            modelBuilder.Entity<Imobiliaria>(entity =>
            {
                entity.ToTable("Imobiliaria");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DataHora).HasColumnType("smalldatetime");
                entity.Property(e => e.DataExclusao).HasColumnType("smalldatetime");
                entity.Property(e => e.Nome).HasColumnType("varchar")
                 .HasMaxLength(80)
                 .IsUnicode(false);
                entity.Property(e => e.Usuario).HasColumnType("nvarchar")
                .HasMaxLength(450);
                entity.Property(e => e.UsuarioExclusao).HasColumnType("nvarchar")
                .HasMaxLength(450)
                .IsRequired(false)
                .IsUnicode(false);

            });
            modelBuilder.Entity<Corretor>(entity =>
            {
                entity.ToTable("Corretor");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DataHora).HasColumnType("smalldatetime");
                entity.Property(e => e.DataExclusao).HasColumnType("smalldatetime");
                entity.Property(e => e.Nome).HasColumnType("varchar")
                 .HasMaxLength(80)
                 .IsUnicode(false);
                entity.Property(e => e.Usuario).HasColumnType("nvarchar")
                .HasMaxLength(450);
                entity.Property(e => e.UsuarioExclusao).HasColumnType("nvarchar")
                .HasMaxLength(450)
                .IsRequired(false)
                .IsUnicode(false);

            });
            modelBuilder.Entity<Loteamento>(entity =>
            {
                entity.ToTable("Loteamento");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.M2Total).HasPrecision(2);
                entity.Property(e => e.DataHora).HasColumnType("smalldatetime");
                entity.Property(e => e.DataExclusao).HasColumnType("smalldatetime");
                entity.Property(e => e.Nome).HasColumnType("varchar")
                 .HasMaxLength(80)
                 .IsUnicode(false);
                entity.Property(e => e.Usuario).HasColumnType("nvarchar")
                .HasMaxLength(450);
                entity.Property(e => e.UsuarioExclusao).HasColumnType("nvarchar")
                .HasMaxLength(450)
                .IsRequired(false)
                .IsUnicode(false);
            });

            modelBuilder.Entity<Lotes>(entity =>
            {
                entity.ToTable("Lote");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Quadra).HasColumnType("varchar(3)");
                entity.Property(e => e.Area).HasColumnType("decimal(10,2)");
                entity.Property(e => e.DataHora).HasColumnType("smalldatetime");
                entity.Property(e => e.DataExclusao).HasColumnType("smalldatetime");
                entity.Property(e => e.Usuario).HasColumnType("nvarchar")
                .HasMaxLength(450);
                entity.Property(e => e.UsuarioExclusao).HasColumnType("nvarchar")
                .HasMaxLength(450)
                .IsRequired(false)
                .IsUnicode(false);
            });

            modelBuilder.Entity<LotesDescritivos>(entity =>
            {
                entity.ToTable("LotesDescritivos");
                entity.HasKey(e => new { e.LoteamentoId, e.Quadra, e.Lote });
                entity.Property(e => e.Area).HasColumnType("decimal(10,2)"); ;
                entity.Property(e => e.DataHora).HasColumnType("smalldatetime");
                entity.Property(e => e.DataExclusao).HasColumnType("smalldatetime");
                entity.Property(e => e.Usuario).HasColumnType("nvarchar")
                .HasMaxLength(450);
                entity.Property(e => e.UsuarioExclusao).HasColumnType("nvarchar")
                .HasMaxLength(450)
                .IsRequired(false)
                .IsUnicode(false);
            });

            modelBuilder.Entity<LoteCategoria>(entity =>
            {
                entity.ToTable("LoteCategoria");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nome).HasColumnType("varchar(60)").HasMaxLength(60).IsRequired(false).IsUnicode(false);
                entity.Property(e => e.DataHora).HasColumnType("smalldatetime");
                entity.Property(e => e.DataExclusao).HasColumnType("smalldatetime");
                entity.Property(e => e.Usuario).HasColumnType("nvarchar").HasMaxLength(450);
                entity.Property(e => e.UsuarioExclusao).HasColumnType("nvarchar").HasMaxLength(450).IsRequired(false).IsUnicode(false);
            });

            modelBuilder.Entity<TabelaPreco>(entity =>
            {
                entity.ToTable("TabelaPreco");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DataHora).HasColumnType("smalldatetime");
                entity.Property(e => e.DataExclusao).HasColumnType("smalldatetime");
                entity.Property(e => e.Usuario).HasColumnType("nvarchar")
                .HasMaxLength(450);
                entity.Property(e => e.UsuarioExclusao).HasColumnType("nvarchar")
                .HasMaxLength(450)
                .IsRequired(false)
                .IsUnicode(false);
            });
            modelBuilder.Entity<TabelaPrecoLotes>(entity =>
            {
                entity.ToTable("TabelaPrecoLotes");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Quadra).HasColumnType("varchar(3)");
                entity.Property(e => e.PrecoVenda).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Entrada).HasColumnType("decimal(18,2)");
                entity.Property(e => e.VrParcelasMensais).HasColumnType("decimal(18,2)");
                entity.Property(e => e.VrParcelasSemestrais).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DataHora).HasColumnType("smalldatetime");
                entity.Property(e => e.DataExclusao).HasColumnType("smalldatetime");
                entity.Property(e => e.Usuario).HasColumnType("nvarchar")
                .HasMaxLength(450);
                entity.Property(e => e.UsuarioExclusao).HasColumnType("nvarchar")
                .HasMaxLength(450)
                .IsRequired(false)
                .IsUnicode(false);
            });

            modelBuilder.Entity<EstadoCivil>(entity =>
            {
                entity.ToTable("EstadoCivil");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Descricao).HasColumnType("varchar(50)");
                entity.Property(e => e.DataHora).HasColumnType("smalldatetime");
                entity.Property(e => e.DataExclusao).HasColumnType("smalldatetime");
                entity.Property(e => e.Usuario).HasColumnType("nvarchar")
                .HasMaxLength(450);
                entity.Property(e => e.UsuarioExclusao).HasColumnType("nvarchar")
                .HasMaxLength(450)
                .IsRequired(false)
                .IsUnicode(false);
            });


            modelBuilder.Entity<TabelaPrecosPlanos>(entity =>
            {
                entity.ToTable("TabelaPrecoPlano");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DataHora).HasColumnType("smalldatetime");
                entity.Property(e => e.DataExclusao).HasColumnType("smalldatetime");
                entity.Property(e => e.Usuario).HasColumnType("nvarchar")
                .HasMaxLength(450);
                entity.Property(e => e.UsuarioExclusao).HasColumnType("nvarchar")
                .HasMaxLength(450)
                .IsRequired(false)
                .IsUnicode(false);
            });

            modelBuilder.Entity<Proposta>(entity =>
            {
                entity.ToTable("Proposta");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DataHora).HasColumnType("smalldatetime");
                entity.Property(e => e.DataExclusao).HasColumnType("smalldatetime");
                entity.Property(e => e.Usuario).HasColumnType("nvarchar")
                .HasMaxLength(450);
                entity.Property(e => e.UsuarioExclusao).HasColumnType("nvarchar")
                .HasMaxLength(450)
                .IsRequired(false)
                .IsUnicode(false);
            });


            modelBuilder.Entity<PropostaAnexo>(entity =>
             {
                 entity.Property(e => e.DataHora).HasColumnType("smalldatetime");
                 entity.Property(e => e.DataExclusao).HasColumnType("smalldatetime");
                 entity.Property(e => e.Usuario).HasColumnType("nvarchar").HasMaxLength(450);
                 entity.Property(e => e.UsuarioExclusao).HasColumnType("nvarchar")
                 .HasMaxLength(450)
                 .IsRequired(false)
                 .IsUnicode(false);
                 entity.ToTable("PropostaAnexo").HasKey(e => e.Id);
             }
            );


            modelBuilder.Entity<PropostaCondicaoComercial>(entity =>
            {
                entity.ToTable("PropostaCondicaoComercial");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DataHora).HasColumnType("smalldatetime");
                entity.Property(e => e.DataExclusao).HasColumnType("smalldatetime");
                entity.Property(e => e.Usuario).HasColumnType("nvarchar")
                .HasMaxLength(450);
                entity.Property(e => e.UsuarioExclusao).HasColumnType("nvarchar")
                .HasMaxLength(450)
                .IsRequired(false)
                .IsUnicode(false);
            });

            modelBuilder.Entity<Comprador>(entity =>
            {
                entity.ToTable("Comprador");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DataHora).HasColumnType("smalldatetime");
                entity.Property(e => e.DataExclusao).HasColumnType("smalldatetime");
                entity.Property(e => e.Usuario).HasColumnType("nvarchar")
                .HasMaxLength(450);
                entity.Property(e => e.UsuarioExclusao).HasColumnType("nvarchar")
                .HasMaxLength(450)
                .IsRequired(false)
                .IsUnicode(false);
            });

            modelBuilder.Entity<TabelaM2>(entity =>
            {
                entity.ToTable("TabelaM2");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DataHora).HasColumnType("smalldatetime");
                entity.Property(e => e.DataExclusao).HasColumnType("smalldatetime");
                entity.Property(e => e.Usuario).HasColumnType("nvarchar")
                .HasMaxLength(450);
                entity.Property(e => e.UsuarioExclusao).HasColumnType("nvarchar")
                .HasMaxLength(450)
                .IsRequired(false)
                .IsUnicode(false);
            });
            modelBuilder.Entity<PropostasCompradores>(entity =>
            {
                entity.ToTable("PropostasCompradores", "dbo");

                entity.Property(e => e.DataExclusao).HasColumnType("smalldatetime");

                entity.Property(e => e.DataHora).HasColumnType("smalldatetime");

                entity.Property(e => e.Usuario).HasMaxLength(450);

                entity.Property(e => e.UsuarioExclusao).HasMaxLength(450);

                entity.HasOne(d => d.Comprador)
                    .WithMany(p => p.PropostasCompradores)
                    .HasForeignKey(d => d.CompradorId)
                    .HasConstraintName("FK_PropostasCompradores_Compradores");

                entity.HasOne(d => d.Proposta)
                    .WithMany(p => p.PropostasCompradores)
                    .HasForeignKey(d => d.PropostaId)
                    .HasConstraintName("FK_PropostasCompradores_Propostas");
            });

            OnModelCreatingGeneratedProcedures(modelBuilder);
            OnModelCreatingGeneratedFunctions(modelBuilder);
            OnModelCreatingPartial(modelBuilder);


        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
