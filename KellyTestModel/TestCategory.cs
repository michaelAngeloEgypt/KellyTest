namespace KellyTestModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TestCategory
    {
        public int id { get; set; }

        [StringLength(255)]
        public string Name { get; set; }
    }
}
