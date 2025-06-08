using Microsoft.EntityFrameworkCore;
using WeatherAlertAPI.Models;

namespace WeatherAlertAPI.Data
{
    public class WeatherAlertContext : DbContext
    {
        public WeatherAlertContext(DbContextOptions<WeatherAlertContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<DadosChuva> DadosChuva { get; set; }
        public DbSet<PreferenciasNotificacao> PreferenciasNotificacao { get; set; }
        public DbSet<AlertaTemperatura> AlertasTemperatura { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuração do Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("USUARIO");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID").HasMaxLength(36);
                entity.Property(e => e.Nome).HasColumnName("NOME").HasMaxLength(100).IsRequired();
                entity.Property(e => e.Cidade).HasColumnName("CIDADE").HasMaxLength(100).IsRequired();
                entity.Property(e => e.Idade).HasColumnName("IDADE").IsRequired();
                entity.Property(e => e.Username).HasColumnName("USERNAME").HasMaxLength(50).IsRequired();
                entity.Property(e => e.Password).HasColumnName("PASSWORD").HasMaxLength(100).IsRequired();
                entity.Property(e => e.TipoUsuario).HasColumnName("TIPO_USUARIO").HasMaxLength(20);
                entity.Property(e => e.IdDadosChuva).HasColumnName("ID_DADOS_CHUVA");

                entity.HasIndex(e => e.Nome).IsUnique();
                entity.HasIndex(e => e.Username).IsUnique();

                entity.HasOne<DadosChuva>()
                    .WithMany()
                    .HasForeignKey(e => e.IdDadosChuva)
                    .HasConstraintName("FK_USUARIO_DADOS_CHUVA");
            });

            // Configuração do DadosChuva
            modelBuilder.Entity<DadosChuva>(entity =>
            {
                entity.ToTable("DADOS_CHUVA");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID").UseIdentityColumn();
                entity.Property(e => e.Cidade).HasColumnName("CIDADE").HasMaxLength(100).IsRequired();
                entity.Property(e => e.Estado).HasColumnName("ESTADO").HasMaxLength(100).IsRequired();
                entity.Property(e => e.Data).HasColumnName("DATA").HasMaxLength(15).IsRequired();
                entity.Property(e => e.TemperaturaMedia).HasColumnName("TEMPERATURA_MEDIA").HasPrecision(5, 2);
                entity.Property(e => e.TotalPrecipitacao).HasColumnName("TOTAL_PRECIPITACAO").HasPrecision(5, 2);
                entity.Property(e => e.ProbabilidadeDeChuva).HasColumnName("PROBABILIDADE_DE_CHUVA");
                entity.Property(e => e.Conclusao).HasColumnName("CONCLUSAO").HasMaxLength(255);
                entity.Property(e => e.IdUsuario).HasColumnName("ID_USUARIO").HasMaxLength(36).IsRequired();

                entity.HasOne<Usuario>()
                    .WithMany()
                    .HasForeignKey(e => e.IdUsuario)
                    .HasConstraintName("FK_DADOS_CHUVA_USUARIO");
            });

            // Configuração do PreferenciasNotificacao
            modelBuilder.Entity<PreferenciasNotificacao>(entity =>
            {
                entity.ToTable("PREFERENCIAS_NOTIFICACAO");
                entity.HasKey(e => e.IdPreferencia);
                entity.Property(e => e.IdPreferencia).HasColumnName("ID_PREFERENCIA").UseIdentityColumn();
                entity.Property(e => e.Cidade).HasColumnName("CIDADE").HasMaxLength(100).IsRequired();
                entity.Property(e => e.Estado).HasColumnName("ESTADO").HasMaxLength(2).IsRequired();
                entity.Property(e => e.TemperaturaMin).HasColumnName("TEMPERATURA_MIN").HasPrecision(5, 2);
                entity.Property(e => e.TemperaturaMax).HasColumnName("TEMPERATURA_MAX").HasPrecision(5, 2);
                entity.Property(e => e.Ativo).HasColumnName("ATIVO").HasDefaultValue(1);
                entity.Property(e => e.DataCriacao).HasColumnName("DATA_CRIACAO").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.DataAtualizacao).HasColumnName("DATA_ATUALIZACAO");
            });

            // Configuração do AlertaTemperatura
            modelBuilder.Entity<AlertaTemperatura>(entity =>
            {
                entity.ToTable("ALERTAS_TEMPERATURA");
                entity.HasKey(e => e.IdAlerta);
                entity.Property(e => e.IdAlerta).HasColumnName("ID_ALERTA").UseIdentityColumn();
                entity.Property(e => e.Cidade).HasColumnName("CIDADE").HasMaxLength(100).IsRequired();
                entity.Property(e => e.Estado).HasColumnName("ESTADO").HasMaxLength(2).IsRequired();
                entity.Property(e => e.Temperatura).HasColumnName("TEMPERATURA").HasPrecision(5, 2).IsRequired();
                entity.Property(e => e.TipoAlerta).HasColumnName("TIPO_ALERTA").HasMaxLength(20).IsRequired();
                entity.Property(e => e.Mensagem).HasColumnName("MENSAGEM").HasMaxLength(500).IsRequired();
                entity.Property(e => e.DataHora).HasColumnName("DATA_HORA").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.Status).HasColumnName("STATUS").HasMaxLength(20).HasDefaultValue("ATIVO");
            });
        }
    }
} 