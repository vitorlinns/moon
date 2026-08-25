namespace Moon.Api.Domain;

public class Category
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Slug { get; set; }

    // ordem de exibição no menu/rodapé/filtro — não é alfabética, então não dá pra
    // derivar isso de Name/Slug em runtime
    public int DisplayOrder { get; set; }
}
