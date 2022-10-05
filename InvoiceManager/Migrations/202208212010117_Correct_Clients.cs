namespace InvoiceManager.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Correct_Clients : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Clients", "Address_Id", "dbo.Addresses");
            DropIndex("dbo.Clients", new[] { "Address_Id" });
            RenameColumn(table: "dbo.Clients", name: "Address_Id", newName: "AddressId");
            AlterColumn("dbo.Clients", "AddressId", c => c.Int(nullable: false));
            CreateIndex("dbo.Clients", "AddressId");
            AddForeignKey("dbo.Clients", "AddressId", "dbo.Addresses", "Id", cascadeDelete: true);
            DropColumn("dbo.Clients", "AssressId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Clients", "AssressId", c => c.Int(nullable: false));
            DropForeignKey("dbo.Clients", "AddressId", "dbo.Addresses");
            DropIndex("dbo.Clients", new[] { "AddressId" });
            AlterColumn("dbo.Clients", "AddressId", c => c.Int());
            RenameColumn(table: "dbo.Clients", name: "AddressId", newName: "Address_Id");
            CreateIndex("dbo.Clients", "Address_Id");
            AddForeignKey("dbo.Clients", "Address_Id", "dbo.Addresses", "Id");
        }
    }
}
