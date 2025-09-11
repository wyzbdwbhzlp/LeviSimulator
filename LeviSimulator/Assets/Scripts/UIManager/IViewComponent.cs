namespace Manager
{
    public interface IViewComponent
    {
        void ShowUIPanel(object data);

        void CloseUIPanel();
        bool IsUIComponentActive { get; }
    }
}