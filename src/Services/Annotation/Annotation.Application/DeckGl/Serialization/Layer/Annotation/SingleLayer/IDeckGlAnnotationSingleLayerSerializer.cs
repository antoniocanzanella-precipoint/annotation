﻿using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using System;

namespace PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Layer.Annotation.SingleLayer;

public interface IDeckGlAnnotationSingleLayerSerializer
{
    int SerializeLayer(LayerHeaderDto layerHeader, DeckGlLayer<AnnotationShape> layer, Span<byte> memory);
}