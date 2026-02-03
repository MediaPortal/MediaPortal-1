using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MediaPortal.GUI.Library;

namespace MediaPortal
{
  public partial class SkinProperties : Form
  {

    [DllImport("user32.dll")]
    static extern int ShowCursor(bool bShow);

    private BindingSource bindingSource = new BindingSource();
    private List<NameValueItem> allItems = new List<NameValueItem>();
    private SortableBindingList<NameValueItem> filtered = new SortableBindingList<NameValueItem>();
    private Dictionary<string, NameValueItem> mirror = new Dictionary<string, NameValueItem>();

    public SkinProperties()
    {
      InitializeComponent();
      DoubleBuffered = true;

      bindingSource.DataSource = filtered;
      dataGridView1.DataSource = bindingSource;
      dataGridView1.AutoGenerateColumns = true;
      dataGridView1.Columns[nameof(NameValueItem.UpdatedAt)].DefaultCellStyle.Format = "HH:mm:ss.fff";
    }

    private void SkinProperties_Shown(object sender, EventArgs e)
    {
      GUIPropertyManager.OnPropertyChanged += GUIPropertyManager_OnPropertyChanged;
      foreach (var kv in GUIPropertyManager.GetNonEmptyProperties())
      {
        AddItem(kv.Key, kv.Value);
      }
      fillFiltered();
    }

    private NameValueItem AddItem(string tag, string tagValue)
    {
      var item = new NameValueItem(tag, tagValue);
      allItems.Add(item);
      mirror[tag] = item;
      return item;
    }

    private void SkinProperties_FormClosed(object sender, FormClosedEventArgs e)
    {
      GUIPropertyManager.OnPropertyChanged -= GUIPropertyManager_OnPropertyChanged;
    }

    private void GUIPropertyManager_OnPropertyChanged(string tag, string tagValue)
    {
      if (mirror.TryGetValue(tag, out var item))
        item.Value = tagValue;
      else
      {
        var nwItem = AddItem(tag, tagValue);
        if (isInFilter(nwItem))
          filtered.Add(nwItem);
      }
    }

    private void SkinProperties_Activated(object sender, EventArgs e)
    {
      int status = 0;
      do
      {
        status = ShowCursor(true);
      } while (status < 1);
    }

    private bool isInFilter(NameValueItem item)
    {
      if (comboBox1.Text.Length == 0)
        return true;

      return comboBox1.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
        .Any(w => item.Name.IndexOf(w, StringComparison.OrdinalIgnoreCase) >= 0);
    }

    private void fillFiltered()
    {
      filtered.RaiseListChangedEvents = false;
      filtered.Clear();
      foreach (var item in allItems.Where(i => isInFilter(i)))
      {
        filtered.Add(item);
      }
      filtered.RaiseListChangedEvents = true;
      filtered.ResetBindings();
    }

    private void comboBox1_TextChanged(object sender, EventArgs e)
    {
      fillFiltered();
    }

    private void comboBox1_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Enter)
      {
        string text = comboBox1.Text;
        if (string.IsNullOrWhiteSpace(text))
          return;

        comboBox1.Items.Remove(text);
        comboBox1.Items.Insert(0, text);
        e.SuppressKeyPress = true;
      }
    }
  }

  class NameValueItem : INotifyPropertyChanged
  {
    public string Name { get; }

    string _value;
    public string Value
    {
      get => _value;
      set
      {
        if (_value == value) return;
        _value = value;
        UpdatedAt = DateTime.Now;
        StackTrace = Environment.StackTrace;

        if (PropertyChanged != null)
        {
          PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
          PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(UpdatedAt)));
          PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(StackTrace)));
        }
      }
    }
    public DateTime UpdatedAt { get; private set; }
    public string StackTrace { get; private set; }

    public NameValueItem(string name, string value)
    {
      Name = name;
      _value = value;
      UpdatedAt = DateTime.Now;
    }

    public event PropertyChangedEventHandler PropertyChanged;
  }

  public class SortableBindingList<T> : BindingList<T>
  {
    private bool _isSorted;
    private ListSortDirection _sortDirection;
    private PropertyDescriptor _sortProperty;

    protected override bool SupportsSortingCore => true;
    protected override bool IsSortedCore => _isSorted;
    protected override ListSortDirection SortDirectionCore => _sortDirection;
    protected override PropertyDescriptor SortPropertyCore => _sortProperty;

    protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
    {
      var list = (List<T>)Items;

      list.Sort((x, y) =>
      {
        var xValue = prop.GetValue(x);
        var yValue = prop.GetValue(y);

        int result = Comparer<object>.Default.Compare(xValue, yValue);
        return direction == ListSortDirection.Ascending ? result : -result;
      });

      _sortProperty = prop;
      _sortDirection = direction;
      _isSorted = true;

      OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
    }

    protected override void RemoveSortCore()
    {
      _isSorted = false;
    }

    protected override void InsertItem(int index, T item)
    {
      base.InsertItem(index, item);

      if (item is INotifyPropertyChanged npc)
        npc.PropertyChanged += Item_PropertyChanged;
    }

    protected override void RemoveItem(int index)
    {
      if (Items[index] is INotifyPropertyChanged npc)
        npc.PropertyChanged -= Item_PropertyChanged;

      base.RemoveItem(index);
    }

    protected override void ClearItems()
    {
      foreach (var item in Items)
      {
        if (item is INotifyPropertyChanged npc)
          npc.PropertyChanged -= Item_PropertyChanged;
      }

      base.ClearItems();
    }

    private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!_isSorted || _sortProperty == null)
        return;

      if (e.PropertyName != _sortProperty.Name)
        return;

      ApplySortCore(_sortProperty, _sortDirection);
    }
  }

}
