using PreciPoint.Ims.Services.Annotation.Enums;
using PreciPoint.Ims.Services.Annotation.Enums.DeckGl;

namespace PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;

public class DeckGlTextLayer<T> : DeckGlLayer<T>
{
    public DeckGlTextLayer(string id = "text-layer-id", bool useHighPrecision = false) : base(id, DeckGlLayerType.Text)
    {
        AddAttribute(DeckGlAttributeCatalog.Text(PrimitiveDataType.UInt32));
        AddAttribute(DeckGlAttributeCatalog.Position(useHighPrecision));
        AddAttribute(DeckGlAttributeCatalog.Color());
    }
}