using System;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;

namespace Water3D
{
    class MyLogger : ContentBuildLogger
    {
        public override void LogMessage(string message, params object[] messageArgs) { }
        public override void LogImportantMessage(string message, params object[] messageArgs) { }
        public override void LogWarning(string helpLink, ContentIdentity contentIdentity, string message, params object[] messageArgs) { }
    }

    class EffectContentProcessorContext : ContentProcessorContext
    {
        public override TargetPlatform TargetPlatform { get { return TargetPlatform.Windows; } }
        public override GraphicsProfile TargetProfile { get { return GraphicsProfile.HiDef; } }
        public override string BuildConfiguration { get { return string.Empty; } }
        public override string IntermediateDirectory { get { return string.Empty; } }
        public override string OutputDirectory { get { return string.Empty; } }
        public override string OutputFilename { get { return string.Empty; } }
        public override ContentIdentity SourceIdentity { get { return new ContentIdentity(OutputFilename); } }
        //public ContentIdentity SourceIdentity { get { return new ContentIdentity(); } }

        public override OpaqueDataDictionary Parameters { get { return parameters; } }
        OpaqueDataDictionary parameters = new OpaqueDataDictionary();

        public override ContentBuildLogger Logger { get { return logger; } }
        ContentBuildLogger logger = new MyLogger();

        public override void AddDependency(string filename) { }
        public override void AddOutputFile(string filename) { }

        public override TOutput Convert<TInput, TOutput>(TInput input, string processorName, OpaqueDataDictionary processorParameters) { throw new NotImplementedException(); }
        public override TOutput BuildAndLoadAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName) { throw new NotImplementedException(); }
        public override ExternalReference<TOutput> BuildAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName, string assetName) { throw new NotImplementedException(); }
    }
}
