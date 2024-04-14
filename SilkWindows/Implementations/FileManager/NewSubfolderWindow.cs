using System.Numerics;
using ImGuiNET;

namespace SilkWindows.Implementations.FileManager;

internal sealed class NewSubfolderWindow(DirectoryInfo directoryInfo) : IImguiDrawer<DirectoryInfo>
{
    public void Init()
    {
    }
    
    public void OnRender(string windowName, double deltaSeconds, ImFonts fonts)
    {
        ImGui.BeginChild(directoryInfo.FullName + '/');
        ImGui.PushFont(fonts.Large);
        ImGui.Text("Enter name for new subfolder:");
        ImGui.PopFont();
        
        ImGui.InputText("##newSubfolderInput", ref _newSubfolderInput, 32);
        
        if (ImGui.Button("Create"))
        {
            try
            {
                directoryInfo.CreateSubdirectory(_newSubfolderInput);
                Result = new DirectoryInfo(_newSubfolderInput);
                _shouldClose = true;
                _errorText = "";
            }
            catch (Exception e)
            {
                _errorText = e.Message;
            }
        }
        
        if (!string.IsNullOrWhiteSpace(_errorText))
        {
            var errorColor = new Vector4(1f, 0.1f, 0.1f, 1f);
            ImGui.PushFont(fonts.Bold);
            ImGui.PushStyleColor(ImGuiCol.Text, errorColor);
            ImGui.Text(_errorText);
            ImGui.PopStyleColor();
            ImGui.PopFont();
        }
        
        ImGui.EndChild();
    }
    
    public void OnWindowUpdate(double deltaSeconds, out bool shouldClose)
    {
        shouldClose = _shouldClose;
    }
    
    public void OnClose()
    {
        
    }
    
    public void OnFileDrop(string[] filePaths)
    {
        
    }
    
    public void OnWindowFocusChanged(bool changedTo)
    {
        
    }
    
    public DirectoryInfo? Result { get; private set; }
    
    private string _errorText = "";
    private bool _shouldClose;
    private string _newSubfolderInput = "";
}