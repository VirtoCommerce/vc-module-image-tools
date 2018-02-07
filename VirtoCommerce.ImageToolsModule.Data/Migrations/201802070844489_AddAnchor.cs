namespace VirtoCommerce.ImageToolsModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAnchor : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ThumbnailOption", "AnchorPosition", c => c.String(maxLength: 64));
            AlterColumn("dbo.ThumbnailOption", "Width", c => c.Int());
            AlterColumn("dbo.ThumbnailOption", "Height", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ThumbnailOption", "Height", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.ThumbnailOption", "Width", c => c.Decimal(precision: 18, scale: 2));
            DropColumn("dbo.ThumbnailOption", "AnchorPosition");
        }
    }
}
