using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using MonoGame.Framework.Content.Pipeline.Builder;

namespace TGC.MonoGame.Samples.Animations.PipelineExtension;

/// <summary>
///     TODO This class needs a refactor.
///     This class allows you to build content in runtime without the need of adding the resources in the mgcb.
///     Currently, it only builds fbx with animations.
/// </summary>
public class CustomPipelineManager : PipelineManager
{
    private readonly IConfigurationRoot _configuration;

    public CustomPipelineManager(string projectDir, string outputDir, string intermediateDir,
        IConfigurationRoot configuration) : base(projectDir, outputDir, intermediateDir)
    {
        _configuration = configuration;
    }

    /// <summary>
    ///     Provides methods for writing compiled binary format.
    /// </summary>
    public ContentCompiler Compiler { get; private set; }

    public static CustomPipelineManager CreateCustomPipelineManager(IConfigurationRoot configuration)
    {
        var binFolder = configuration["BinFolder"];
        var objFolder = configuration["ObjFolder"];
        var contentFolder = configuration["ContentFolder"];

        // This code is from MonoGame.Content.Builder.BuildContent.Build(out int successCount, out int errorCount).
        // TODO the folder logic can be load in the game a save in the configuration, to avoid this folder logic.
        var projectDirectory = PathHelper.Normalize(AppDomain.CurrentDomain.BaseDirectory);
        var projectContentDirectory =
            PathHelper.Normalize(Path.GetFullPath(Path.Combine(projectDirectory, "../../../" + contentFolder)));
        var outputPath = PathHelper.Normalize(Path.Combine(projectDirectory, contentFolder));
        var projectDirectoryParts = projectDirectory.Split(new[] { binFolder }, StringSplitOptions.None);
        var intermediatePath = PathHelper.Normalize(Path.GetFullPath(Path.Combine(projectContentDirectory,
            "../" + objFolder + projectDirectoryParts.Last())));

        return new CustomPipelineManager(projectContentDirectory, outputPath, intermediatePath, configuration);
    }

    public void BuildAnimationContent(string modelFilename)
    {
        var parameters = new OpaqueDataDictionary
        {
            { "ColorKeyColor", "0,0,0,0" },
            { "ColorKeyEnabled", "True" },
            { "DefaultEffect", "BasicEffect" },
            { "GenerateMipmaps", "True" },
            { "GenerateTangentFrames", "False" },
            { "PremultiplyTextureAlpha", "True" },
            { "PremultiplyVertexColors", "True" },
            { "ResizeTexturesToPowerOfTwo", "False" },
            { "RotationX", "0" },
            { "RotationY", "0" },
            { "RotationZ", "0" },
            { "Scale", "1" },
            { "SwapWindingOrder", "False" },
            { "TextureFormat", "Compressed" }
        };

        // Record what we're building and how.
        var pipelineEvent = new PipelineBuildEvent
        {
            SourceFile = modelFilename,
            DestFile = OutputDirectory + modelFilename + _configuration["ContentExtension"],
            Importer = _configuration["FbxImporterName"],
            Processor = _configuration["ProcessorName"],
            Parameters = ValidateProcessorParameters(_configuration["ProcessorName"], parameters)
        };

        var importContext = new PipelineImporterContext(this);
        var importer = new FbxImporter();
        var nodeContent =
            importer.Import(ProjectDirectory + modelFilename + _configuration["FbxExtension"], importContext);
        var animationProcessor = new AnimationProcessor();

        var processContext = new PipelineProcessorContext(this, pipelineEvent);
        var modelContent = animationProcessor.Process(nodeContent, processContext);

        // Write the content to disk.
        WriteXnb(modelContent, pipelineEvent);
    }

    private void WriteXnb(object content, PipelineBuildEvent pipelineEvent)
    {
        // Make sure the output directory exists.
        var outputFileDir = Path.GetDirectoryName(pipelineEvent.DestFile);

        Directory.CreateDirectory(outputFileDir);

        Compiler ??= new ContentCompiler();

        // Write the XNB.
        using (var stream = new FileStream(pipelineEvent.DestFile, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            Compiler.Compile(stream, content, Platform, Profile, CompressContent, OutputDirectory, outputFileDir);
        }

        // Store the last write time of the output XNB here so we can verify it hasn't been tampered with.
        pipelineEvent.DestTime = File.GetLastWriteTime(pipelineEvent.DestFile);
    }
}