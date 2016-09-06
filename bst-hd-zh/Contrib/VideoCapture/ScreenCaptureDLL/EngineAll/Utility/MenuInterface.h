#pragma  once
class Menu;

class MenuShortcut
{
public:
    MenuShortcut();
    MenuShortcut(const String &S);
    void Dispatch(InputManager &I, UINT Id);

private:
    UINT _Key;
    bool _Shift;
    bool _Ctrl;
    bool _Alt;
};

class MenuEntry
{
public:
    MenuEntry();
    MenuEntry(Menu *SubMenu, const String &Name, UINT Id);
    ~MenuEntry();
    void FreeMemory();

    void DispatchKeyboardShortcuts(InputManager &I);

    __forceinline Menu& SubMenu()
    {
        Assert(_SubMenu != NULL, "MenuEntry uninitialized");
        return *_SubMenu;
    }
    __forceinline String& Name()
    {
        return _Name;
    }

private:
    Menu *_SubMenu;
    String _Name;
    UINT _Id;
    MenuShortcut _Shortcut;
};

class Menu
{
public:
    Menu();
    Menu(HMENU Handle);
    ~Menu();
    void FreeMemory();

    Menu& AddSubMenu(WindowManager &WM, UINT Id, const String &Name);
    void AddString(WindowManager &WM, UINT Id, const String &Name);
    void AddSeperator(WindowManager &WM, UINT Id);
    void SetItemState(WindowManager &WM, UINT Id, bool Enabled, bool Checked);
    void SetItemName(WindowManager &WM, UINT Id, const String &Name);

    bool HasSubMenu(const String &Name);
    Menu& GetSubMenu(const String &Name);

    void DispatchKeyboardShortcuts(InputManager &I);

private:
    Vector<MenuEntry> _Entries;
    HMENU _Handle;
};

class MenuInterface
{
public:
    void Init(WindowManager &WM);
    void DispatchKeyboardShortcuts(InputManager &I);
    __forceinline Menu& MenuBar()
    {
        Assert(_MenuBar != NULL, "Menu bar uninitialized");
        return *_MenuBar;
    }

private:
    Menu *_MenuBar;
};