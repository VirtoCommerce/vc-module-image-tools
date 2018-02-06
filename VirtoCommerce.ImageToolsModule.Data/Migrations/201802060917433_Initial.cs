namespace VirtoCommerce.ImageToolsModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ThumbnailTask",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 1024),
                        WorkPath = c.String(nullable: false, maxLength: 2048),
                        LastRun = c.DateTime(),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(),
                        CreatedBy = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ThumbnailTaskOption",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ThumbnailTaskEntityId = c.String(nullable: false, maxLength: 128),
                        ThumbnailOptionEntityId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ThumbnailOption", t => t.ThumbnailOptionEntityId, cascadeDelete: true)
                .ForeignKey("dbo.ThumbnailTask", t => t.ThumbnailTaskEntityId, cascadeDelete: true)
                .Index(t => t.ThumbnailTaskEntityId)
                .Index(t => t.ThumbnailOptionEntityId);
            
            CreateTable(
                "dbo.ThumbnailOption",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 1024),
                        FileSuffix = c.String(nullable: false, maxLength: 128),
                        ResizeMethod = c.String(nullable: false, maxLength: 64),
                        ThumbnailTaskId = c.String(nullable: false, maxLength: 128),
                        Width = c.Decimal(precision: 18, scale: 2),
                        Height = c.Decimal(precision: 18, scale: 2),
                        BackgroundColor = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(),
                        CreatedBy = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ThumbnailTaskOption", "ThumbnailTaskEntityId", "dbo.ThumbnailTask");
            DropForeignKey("dbo.ThumbnailTaskOption", "ThumbnailOptionEntityId", "dbo.ThumbnailOption");
            DropIndex("dbo.ThumbnailTaskOption", new[] { "ThumbnailOptionEntityId" });
            DropIndex("dbo.ThumbnailTaskOption", new[] { "ThumbnailTaskEntityId" });
            DropTable("dbo.ThumbnailOption");
            DropTable("dbo.ThumbnailTaskOption");
            DropTable("dbo.ThumbnailTask");
        }
    }
}
