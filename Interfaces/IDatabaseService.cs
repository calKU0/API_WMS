using APIWMS.Models;

namespace APIWMS.Interfaces
{
    public interface IDatabaseService
    {
        public int UpdateAttribute(Object obj, List<Models.Attribute> attribute);
        public int UpdateProduct(Product product);
    }
}
