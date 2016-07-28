using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Infrastructure;

namespace LocalDatabase
{	
	
	public class LocalDbContext : DbContext
	{
		public DbSet<ItemChannel> Channels { get; set; }
		public DbSet<ItemItem> Items { get; set; }
		public DbSet<ItemTax> Taxes { get; set; }
		public DbSet<ItemPayment> Payments { get; set; }
		//public DbSet<ItemEmps> Emps { get; set; }
		public LocalDbContext( string connection):base( connection)
		{
			//Database.Initialize(false);
		}
		protected override bool ShouldValidateEntity(System.Data.Entity.Infrastructure.DbEntityEntry entityEntry)
		{
			return base.ShouldValidateEntity(entityEntry);
		}
		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
		}
	}


	public class ProductInitializer : DropCreateDatabaseIfModelChanges<LocalDbContext>
	
	{
		protected override void Seed(LocalDbContext context)
		{
		}
	}

	public class ItemEmps: ItemBase
	{

	}
	public class ItemPayment: ItemBase
	{
	}

	public class ItemChannel: ItemBase
	{
		[MaxLength(250)]
		public string ChannelName{ get; set;}
	}
	public class ItemItem : ItemBase
	{

	}

	public class ItemTax : ItemBase
	{
		[MaxLength(255)]
		public override string Name
		{
			get
			{
				return base.Name;
			}
			set
			{
				base.Name = value;
			}
		}
	}
	public class ItemBaseConfiguration : EntityTypeConfiguration<ItemBase>
	{
		public ItemBaseConfiguration( string tablename):base()
		{
			// Properties
			this.Property(t => t.ID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

			this.Property(t => t.Name).IsRequired().HasMaxLength(255);

			// Table & Column Mappings
			this.ToTable(tablename);
			this.Property(t => t.ID).HasColumnName("ID");
			this.Property(t => t.Name).HasColumnName("Name");
		}
	}
	public class ItemBase
	{
		 [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public virtual int ID { get; set; }
		[MaxLength(150)]
		public virtual string Name { get; set; }
	}
}
