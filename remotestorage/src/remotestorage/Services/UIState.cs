public class UIState
{
    public event Action? OnChange;
    private bool _showNavbar = true;

    public bool ShowNavbar
    {
        get => _showNavbar;
        set
        {
            if (_showNavbar == value) return;
            _showNavbar = value;
            OnChange?.Invoke();
        }
    }
}
