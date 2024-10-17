namespace Sisk.Mvc;

public sealed record class ModelBag<TModel>(TModel? Model)
{
    public static readonly ModelBag<TModel> Empty = new ModelBag<TModel>(default(TModel));

    public string PageTitle { get; set; } = "Document";
    public string DebugId { get; set; } = Guid.NewGuid().ToString()[0..8];
}
