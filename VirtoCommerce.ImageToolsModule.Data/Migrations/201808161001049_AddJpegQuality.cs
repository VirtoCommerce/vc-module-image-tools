namespace VirtoCommerce.ImageToolsModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddJpegQuality : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ThumbnailOption", "JpegQuality", c => c.String(maxLength: 64));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ThumbnailOption", "JpegQuality");
        }
    }
}
