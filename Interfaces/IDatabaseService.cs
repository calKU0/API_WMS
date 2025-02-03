using APIWMS.Models;

namespace APIWMS.Interfaces
{
    public interface IDatabaseService
    {
        public Task<List<string>> UpdateAttributes(int obiNumber, int obiType, int obiLp, List<Models.Attribute> attributes);
        public Task<int> UpdateProduct(Product product);
        public Task<List<string>> UpdateProductUnits(int productId, List<ProductUnit> productUnits);
    }
}
