namespace Manager
{
    public interface IUIComponent
    {
        void ShowUIPanel(object data);

        void CloseUIPanel();
        bool IsUIComponentActive { get; }
    }
}