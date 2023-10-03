using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using PreciPoint.Ims.Services.Annotation.Enums.DeckGl;
using System.Linq;

namespace PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Custom;

public class DeckGlCounterLayer : DeckGlLayer<AnnotationShape>
{
    public DeckGlCounterLayer(string id) : base(id, DeckGlLayerType.Counter)
    {
        AddAttribute(DeckGlAttributeCatalog.Position());
        AddAttribute(DeckGlAttributeCatalog.Color());
    }

    public override int Length => Data.Sum(x => x.CounterGroups.Sum(y => y.Counters.Count));
}