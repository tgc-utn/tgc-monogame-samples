using System;
using System.Diagnostics;
using System.IO;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.Samples.Samples.Shaders.ShaderReloader;

public class ShaderReloader
{
    private static readonly string MgfxcTool = "mgfxc";

    private readonly GraphicsDevice _graphicsDevice;
    private readonly string _shaderCodeFileName;
    private readonly string _shaderCodePath;
    private readonly string _shaderCompiledPath;

    private bool _compileError;
    private FileSystemWatcher _fileWatcher;
    private ProcessStartInfo _processStartInfo;

    public ShaderReloader(string path, string compiledFileExtension, GraphicsDevice device)
    {
        _shaderCodePath = path;
        _shaderCompiledPath = Path.ChangeExtension(path, compiledFileExtension);
        _shaderCodeFileName = Path.GetFileName(_shaderCodePath);

        _graphicsDevice = device;

        ConfigureProcessStartInfo();
        ConfigureWatcher();
    }

    public event Action<Effect> OnCompile;

    private void ConfigureWatcher()
    {
        _fileWatcher = new FileSystemWatcher();

        _fileWatcher.Path = Path.GetDirectoryName(_shaderCodePath);
        _fileWatcher.Filter = _shaderCodeFileName;

        // Add event handlers.
        // Listen to any event, as VS renames files instead of changing them.
        _fileWatcher.Changed += ReplaceShader;
        _fileWatcher.Created += ReplaceShader;
        _fileWatcher.Deleted += ReplaceShader;
        _fileWatcher.Renamed += ReplaceShader;

        // Begin watching.
        _fileWatcher.EnableRaisingEvents = true;
    }

    private void ReplaceShader(object sender, FileSystemEventArgs eventArgs)
    {
        // Can be triggered by temp files with suffixes.
        if (eventArgs.Name.Equals(_shaderCodeFileName))
        {
            CompileShader();

            if (!_compileError && File.Exists(_shaderCompiledPath))
            {
                var byteCode = File.ReadAllBytes(_shaderCompiledPath);
                var effect = new Effect(_graphicsDevice, byteCode);

                // Delete the file as we don't need it anymore.
                File.Delete(_shaderCompiledPath);

                OnCompile?.Invoke(effect);
            }
        }
    }

    private void ConfigureProcessStartInfo()
    {
        _processStartInfo = new ProcessStartInfo
        {
            FileName = MgfxcTool,
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

        var pProcess = new Process();
        pProcess.StartInfo = _processStartInfo;
        pProcess.EnableRaisingEvents = true;

        // Get program output.
        var stdError = new StringBuilder();
        var stdOutput = new StringBuilder();

        // Callbacks.
        pProcess.OutputDataReceived += (_, args) => stdOutput.Append(args.Data);
        pProcess.ErrorDataReceived += (_, args) => stdError.Append(args.Data);

        pProcess.Start();
        pProcess.BeginOutputReadLine();
        pProcess.BeginErrorReadLine();
        pProcess.WaitForExit();

        if (pProcess.ExitCode != 0)
        {
            Debug.WriteLine(stdError.ToString(), "Shader-Error");
            _compileError = true;
        }
    }

    public void Dispose()
    {
        _fileWatcher.Dispose();
    }
}
