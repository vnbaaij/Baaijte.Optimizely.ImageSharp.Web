using EPiServer.Core;

namespace AlloyMVC.Models.Pages
{
    public interface IHasRelatedContent
    {
        ContentArea RelatedContentArea { get; }
    }
}
