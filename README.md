# TinyMapper 
TinyMapper is, as its name implies, a tiny object to object mapping utility. It is very small (less than 300 PLOC), but does some useful things beyond just object conversion, one such example being the ability to translate Linq expression trees from one parameter type to a different type for an underlying query provider. 

It's experimental, and very basic. Use at your own risk.

Example configuration:
```C#
using static TinyMapper.TinyMapper;

ModelMapper.CreateMap<Foo, Fee>(map => 
{
    map.Property(x=>x.Id, x=>x.FeeId)
       .Property(x=>x.Transaction, x=>x.History, x=> ConversionLogic(x), x=> SomeOtherConversionLogic(x))
}

```

Example Object Conversion:
```C#
var foo = new Foo();
var fee = ModelMapper.Map<Fee>(foo);
Assert.ArEqual(foo.Id, fee.FeeId);

```

Example Expression Tree Translation:
```C#
// My data model only knows Foo as Fee
class MyRepo: IRepo
{
    public IQueryable<Foo> Find(Expression<Func<Foo, bool>> filter)
    {
        // breaks the "set" returns in memory. feel free to improve
        return db.Set<Fee>().Where(ModelMapper.Translate<Foo ,Fee, bool>(filter))
            .ToArray()
            .Select(x=>ModelMapper<Foo>(x))
            .AsQeryable();
    }
}

// elsewhere - will be executed on the db as a query against the Fee table
myRepo.Find(x=>x.Id == 1) 

```