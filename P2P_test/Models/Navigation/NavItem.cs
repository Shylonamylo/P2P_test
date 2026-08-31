using System;
using Avalonia.Media;

namespace P2P_test.Models.Navigation;

public class NavItem
{
    public int Id { get; set; }
    public string Title { get; set; }
    public Type ViewModelType { get; set; }

    public NavItem(int id, string title, Type viewModelType)
    {
        Id = id;
        Title = title;
        ViewModelType = viewModelType;
    }
}