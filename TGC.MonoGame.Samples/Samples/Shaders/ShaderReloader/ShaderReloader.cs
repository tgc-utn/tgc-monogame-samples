using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.Samples.Samples.Shaders.ShaderReloader
{
    class ShaderReloader
    {
        private readonly static string CompiledShaderFileExtension = ".xnb";

        private string _shaderCodePath;
        private string _shaderCompiledPath;
        private string _shaderCodeFileName;

        private FileSystemWatcher _fileWatcher;
        private ProcessStartInfo _processStartInfo;
        
        private GraphicsDevice _graphicsDevice;

        private bool _compileError;

        public event Action<Effect> OnCompile;

        public ShaderReloader(string path, GraphicsDevice device)
        {
            _shaderCodePath = path;
            _shaderCompiledPath = Path.ChangeExtension(path, CompiledShaderFileExtension);
            _shaderCodeFileName = Path.GetFileName(_shaderCodePath);

            _graphicsDevice = device;

            ConfigureProcessStartInfo();
            ConfigureWatcher();
        }

        private void ConfigureWatcher()
        {
            _fileWatcher = new FileSystemWatcher();

            _fileWatcher.Path = Path.GetDirectoryName(_shaderCodePath);
            _fileWatcher.Filter = _shaderCodeFileName;

            // Add event handlers
            // Listen to any event, as VS renames files instead of changing them
            _fileWatcher.Changed += ReplaceShader;
            _fileWatcher.Created += ReplaceShader;
            _fileWatcher.Deleted += ReplaceShader;
            _fileWatcher.Renamed += ReplaceShader;

            // Begin watching.
            _fileWatcher.EnableRaisingEvents = true;
        }

        private void ReplaceShader(object sender, FileSystemEventArgs eventArgs)
        {
            // Can be triggered by temp files with suffixes
            if (eventArgs.Name.Equals(_shaderCodeFileName))
            {
                CompileShader();
                if (!_compileError)
                {
                    var byteCode = File.ReadAllBytes(_shaderCompiledPath);
                    var effect = new Effect(_graphicsDevice, byteCode);

                    // Delete the file as we don't need it anymore
                    File.Delete(_shaderCompiledPath);

                    OnCompile.Invoke(effect);
                }
            }
        }

        private void ConfigureProcessStartInfo()
        {
            _processStartInfo = new ProcessStartInfo
            {
                FileName = "mgfxc",
                Arguments = _shaderCodePath + " " + _shaderCompiledPath,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };
        }

        private void CompileShader()
        {
            _compileError = false;

            Process pProcess = new Process();
            pProcess.StartInfo = _processStartInfo;
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

            if (!stdError.Equals(""))
                ProcessError(stdError);
        }

        private void ProcessError(string error)
        {
            Debug.WriteLine(error, "Shader-Error");
            _compileError = true;
        }

        public void Dispose()
        {
            _fileWatcher.Dispose();
        }
    }
}
