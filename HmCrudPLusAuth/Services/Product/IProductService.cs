using HmCrudPLusAuth.Common;
using HmCrudPLusAuth.Models;

namespace HmCrudPLusAuth.Services.Product
{
    public interface IProductService
    {
        Result<List<Models.Product>> GetAll(int userId);
        Result<Models.Product> GetById(int productId, int userId);
        Result<Models.Product> Create(string name, string description, double price, int userId);
        Result<Models.Product> Update(int productId, string name, string description, double price, int userId);
        Result<string> Delete(int productId, int userId);
    }
}
