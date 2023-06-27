namespace Blog.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    //Lista sempre e inicializada - nunca sera nula e sim vazio
    public IList<Post> Posts { get; set; }
}