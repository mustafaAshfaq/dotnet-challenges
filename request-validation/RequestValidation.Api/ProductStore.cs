namespace RequestValidation.Api;

public sealed record Product(int Id, string Name, int Quantity);

public sealed class ProductStore
{
    private readonly object _gate = new();
    private readonly List<Product> _products = [];
    private int _nextId = 1;

    public IReadOnlyCollection<Product> GetAll()
    {
        lock (_gate)
        {
            return _products.ToArray();
        }
    }

    public Product? Get(int id)
    {
        lock (_gate)
        {
            return _products.FirstOrDefault(product => product.Id == id);
        }
    }

    public Product Create(ProductInput input)
    {
        lock (_gate)
        {
            var product = new Product(_nextId++, input.Name, input.Quantity);
            _products.Add(product);
            return product;
        }
    }

    public bool Replace(int id, ProductInput input)
    {
        lock (_gate)
        {
            var index = _products.FindIndex(product => product.Id == id);
            if (index < 0)
            {
                return false;
            }

            _products[index] = new Product(id, input.Name, input.Quantity);
            return true;
        }
    }
}
