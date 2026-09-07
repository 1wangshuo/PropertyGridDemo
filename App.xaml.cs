using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using PropertyGridLib.Controls;
using PropertyGridDemo.Models;

namespace PropertyGridDemo
{
    public partial class App : Application
    {
        public static readonly ConditionalWeakTable<PropertyItem, string> UnitStateTable =
            new ConditionalWeakTable<PropertyItem, string>();

        private void UnitCombo_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not ComboBox combo || combo.DataContext is not PropertyItem pi) return;
            if (pi.CustomEditorAttribute is not DemoTimeSpanEditorAttribute attr) return;

            var units = (attr.Units ?? "").Split(',');
            for (int i = 0; i < units.Length; i++) units[i] = units[i].Trim();
            units = units.Where(u => !string.IsNullOrEmpty(u)).ToArray();
            combo.ItemsSource = units;

            string selected;
            if (UnitStateTable.TryGetValue(pi, out var cachedSelected))
                selected = cachedSelected;
            else if (!string.IsNullOrEmpty(attr.DefaultUnit) && Array.IndexOf(units, attr.DefaultUnit) >= 0)
                selected = attr.DefaultUnit;
            else if (units.Length > 0)
                selected = units[0];
            else
                return;

            combo.SelectedItem = selected;
        }

        private void UnitCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ComboBox combo || combo.DataContext is not PropertyItem pi) return;
            if (combo.SelectedItem is string selectedUnit && !string.IsNullOrEmpty(selectedUnit))
            {
                UnitStateTable.Remove(pi);
                UnitStateTable.Add(pi, selectedUnit);
            }
        }
    }
}
