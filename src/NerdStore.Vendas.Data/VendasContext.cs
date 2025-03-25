using Microsoft.EntityFrameworkCore;
using NerdStore.Core.Bus;
using NerdStore.Core.Data;
using NerdStore.Core.Messages;
using NerdStore.Vendas.Domain;

namespace NerdStore.Vendas.Data
{
    public class VendasContext : DbContext, IUnitOfWork
    {
        private readonly IMediatrHandler _mediatorHandler;


        public VendasContext(DbContextOptions<VendasContext> options, IMediatrHandler mediatorHandler)
            : base(options)
        {
            _mediatorHandler = mediatorHandler;
        }

        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoItem> PedidoItems { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }


        public async Task<bool> Commit()
        {
            foreach (var entry in ChangeTracker.Entries().Where(entry => entry.Entity.GetType().GetProperty("DataCadastro") != null))
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Property("DataCadastro").CurrentValue = DateTime.Now;
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Property("DataCadastro").IsModified = false;
                }
            }

            var sucesso = await base.SaveChangesAsync() > 0;
            if (sucesso) await _mediatorHandler.PublicarEventos(this);
            return sucesso;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(VendasContext).Assembly);
            modelBuilder.Ignore<Event>();

            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
                relationship.DeleteBehavior = DeleteBehavior.ClientSetNull;

            base.OnModelCreating(modelBuilder);
        }

    }
}
