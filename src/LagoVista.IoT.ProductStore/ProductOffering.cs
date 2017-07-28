using System;

namespace LagoVista.IoT.ProductStore
{
    public class ProductOffering
    {
        public string Id { get; set; }

        public string ProductCategoryKey { get; set; }
        public string Name { get; set; }

        public string SKU { get; set; }

        public string DetailsHTML { get; set; }

        public string Description { get; set; }

        public string RemoteResourceId { get; set; }

        public decimal UnitCost { get; set; }

        public decimal UnitType { get; set; }
    }
}
