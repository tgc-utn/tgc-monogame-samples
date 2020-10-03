
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace TGC.MonoGame.Samples.Samples.Shaders
{
    class ShaderReloader
    {
        private readonly static string CompiledShaderFileExtension = ".xnb";

        private string ShaderCodePath;
        private string ShaderCompiledPath;
        private string FileNameWithExtension;

        private FileSystemWatcher FileWatcher;
        private ProcessStartInfo ProcessStartInfo;
        
        private GraphicsDevice GraphicsDevice;

        private bool CompileError;

        public event Action<Effect> OnCompile;



        public ShaderReloader(string path, GraphicsDevice device)
        {
            ShaderCodePath = path;
            FileNameWithExtension = Path.GetFileName(path);
            var fileName = Path.GetFileNameWithoutExtension(path);
            var ShaderCodeFolder = path.Split(fileName)[0];
            ShaderCompiledPath = ShaderCodeFolder + fileName + CompiledShaderFileExtension;

            GraphicsDevice = device;

            ConfigureProcessStartInfo();
            ConfigureWatcher(ShaderCodeFolder, FileNameWithExtension);
        }



        private void ConfigureWatcher(string folder, string fileName)
        {
            FileWatcher = new FileSystemWatcher();

            FileWatcher.Path = folder;

            FileWatcher.Filter = fileName;

            // Add event handlers
            // Listen to any event, as VS renames files instead of changing them
            FileWatcher.Changed += ReplaceShader;
            FileWatcher.Created += ReplaceShader;
            FileWatcher.Deleted += ReplaceShader;
            FileWatcher.Renamed += ReplaceShader;

            // Begin watching.
            FileWatcher.EnableRaisingEvents = true;
        }

        private void ReplaceShader(object sender, FileSystemEventArgs eventArgs)
        {
            // Can be triggered by temp files with suffixes
            if (!eventArgs.Name.Equals(FileNameWithExtension))
                return;
            
            CompileError = false;

            CompileShader();

            if (!CompileError)
            {
                var byteCode = File.ReadAllBytes(ShaderCompiledPath);
                var effect = new Effect(GraphicsDevice, byteCode);

                // Delete the file as we don't need it anymore
                File.Delete(ShaderCompiledPath);

                OnCompile.Invoke(effect);
            }
        }



        private void ConfigureProcessStartInfo()
        {
            ProcessStartInfo = new ProcessStartInfo
            {
                FileName = "mgfxc",
                Arguments = ShaderCodePath + " " + ShaderCompiledPath,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };
        }

        private void CompileShader()
        {
            Process pProcess = new Process();
            pProcess.StartInfo = ProcessStartInfo;
            pProcess.EnableRaisingEvents = true;
            //Get program output
            string stdError = null;
            StringBuilder stdOutput = new StringBuilder();
            
            // Callbacks
            pProcess.OutputDataReceived += (sender, args) => stdOutput.Append(args.Data);

            pProcess.Start();
            pProcess.BeginOutputReadLine();
            stdError = pProcess.StandardError.ReadToEnd();
            pProcess.WaitForExit();
            if (!(stdError.Equals(null) || stdError.Equals("")))
                ProcessError(stdError);
        }

        private void ProcessError(string error)
        {
            Debug.WriteLine(error, "Shader-Error");
            CompileError = true;
        }

        public void Dispose()
        {
            FileWatcher.Dispose();
        }

    }
}
