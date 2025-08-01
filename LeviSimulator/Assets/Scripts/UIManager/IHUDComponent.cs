namespace UIManager
{
    public interface IHUDComponent
    {
        /// <summary>
        /// 显示HUD组件
        /// </summary>
        void ShowHUD();

        /// <summary>
        /// 隐藏HUD组件
        /// </summary>
        void HideHUD();

        /// <summary>
        /// 更新HUD数据
        /// </summary>
        /// <param name="data">要更新的数据</param>
        void UpdateHUDData(object data);

        /// <summary>
        /// HUD组件是否可见
        /// </summary>
        bool IsHUDVisible { get; }

        /// <summary>
        /// HUD组件是否启用
        /// </summary>
        bool IsHUDEnabled { get; set; }
        /// <summary>
        ///  HUD组件是否默认隐藏
        /// </summary>
        bool IsDefaultHide { get; }    
    }
}