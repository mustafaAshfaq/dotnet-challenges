namespace InputTrust.Api;

public sealed class ListingStore
{
    private readonly object _gate = new();
    private readonly List<Listing> _listings = [];
    private int _nextId = 1;

    public Listing? Get(int id)
    {
        lock (_gate)
        {
            return _listings.FirstOrDefault(listing => listing.Id == id);
        }
    }

    public Listing Create(Listing input)
    {
        lock (_gate)
        {
            var listing = new Listing
            {
                Id = _nextId++,
                Title = input.Title,
                Price = input.Price,
            };
            _listings.Add(listing);
            return listing;
        }
    }

    public bool Replace(int id, Listing input)
    {
        lock (_gate)
        {
            var index = _listings.FindIndex(listing => listing.Id == id);
            if (index < 0)
            {
                return false;
            }

            _listings[index] = new Listing
            {
                Id = id,
                Title = input.Title,
                Price = input.Price,
            };
            return true;
        }
    }
}
