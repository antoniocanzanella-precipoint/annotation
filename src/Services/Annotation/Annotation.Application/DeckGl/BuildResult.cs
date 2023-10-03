using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using System.Collections.Generic;

namespace PreciPoint.Ims.Services.Annotation.Application.DeckGl;

public class BuildResult
{
    public BuildResult(List<BaseLayerHeaderDto> layerHeaders, int binaryDataSize,
        Dictionary<string, DeckGlLayer<AnnotationShape>> deckGlLayers)
    {
        LayerHeaders = layerHeaders;
        BinaryDataSize = binaryDataSize;
        DeckGlLayers = deckGlLayers;
    }

    public List<BaseLayerHeaderDto> LayerHeaders { get; }
    public int BinaryDataSize { get; }
    public Dictionary<string, DeckGlLayer<AnnotationShape>> DeckGlLayers { get; }
}