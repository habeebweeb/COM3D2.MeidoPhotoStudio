using System;

using MeidoPhotoStudio.Plugin.Core.Schema;
using MeidoPhotoStudio.Plugin.Core.Schema.Background;
using MeidoPhotoStudio.Plugin.Core.Schema.Camera;
using MeidoPhotoStudio.Plugin.Core.Schema.Effects;
using MeidoPhotoStudio.Plugin.Core.Schema.Light;
using MeidoPhotoStudio.Plugin.Core.Schema.Message;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class SceneSchemaBuilder
{
    private readonly ISceneSchemaAspectBuilder<MessageWindowSchema> messageWindowSchemaBuilder;
    private readonly ISceneSchemaAspectBuilder<CameraSchema> cameraSchemaBuilder;
    private readonly ISceneSchemaAspectBuilder<LightRepositorySchema> lightRepositorySchemaBuilder;
    private readonly ISceneSchemaAspectBuilder<EffectsSchema> effectsSchemaBuilder;
    private readonly ISceneSchemaAspectBuilder<BackgroundSchema> backgroundSchemaBuilder;

    public SceneSchemaBuilder(
        ISceneSchemaAspectBuilder<MessageWindowSchema> messageWindowSchemaBuilder,
        ISceneSchemaAspectBuilder<CameraSchema> cameraSchemaBuilder,
        ISceneSchemaAspectBuilder<LightRepositorySchema> lightRepositorySchemaBuilder,
        ISceneSchemaAspectBuilder<EffectsSchema> effectsSchemaBuilder,
        ISceneSchemaAspectBuilder<BackgroundSchema> backgroundSchemaBuilder)
    {
        this.messageWindowSchemaBuilder = messageWindowSchemaBuilder ?? throw new ArgumentNullException(nameof(messageWindowSchemaBuilder));
        this.cameraSchemaBuilder = cameraSchemaBuilder ?? throw new ArgumentNullException(nameof(cameraSchemaBuilder));
        this.lightRepositorySchemaBuilder = lightRepositorySchemaBuilder ?? throw new ArgumentNullException(nameof(lightRepositorySchemaBuilder));
        this.effectsSchemaBuilder = effectsSchemaBuilder ?? throw new ArgumentNullException(nameof(effectsSchemaBuilder));
        this.backgroundSchemaBuilder = backgroundSchemaBuilder ?? throw new ArgumentNullException(nameof(backgroundSchemaBuilder));
    }

    public SceneSchema Build() =>
        new()
        {
            MessageWindow = messageWindowSchemaBuilder.Build(),
            Camera = cameraSchemaBuilder.Build(),
            Lights = lightRepositorySchemaBuilder.Build(),
            Effects = effectsSchemaBuilder.Build(),
            Background = backgroundSchemaBuilder.Build(),
        };
}
