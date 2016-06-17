#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils.ExtensionMethods;

namespace Mediaportal.TV.Server.SetupTV.Sections.Helpers
{
  /// <summary>
  /// Class that handles populating and filtering of the list view used in the
  /// channels section.
  /// </summary>
  internal class ChannelListViewHandler
  {
    #region filter class

    /// <summary>
    /// A simple channel filtering implementation based on an abstract syntax
    /// tree.
    /// </summary>
    /// <remarks>
    /// The most basic filter - any old string - does a contains check on
    /// channel name only.
    /// 
    /// On top of this is a syntax that enables people to:
    /// - specify which fields to search
    /// - use alternative relational operators
    /// - combine expressions with conditional operators
    /// 
    /// The formal description of the syntax is probably something like this:
    /// expression            ::= term (conditional-operator term)
    /// term                  ::= ("!") component relational-operator (string | integer | "true" | "false") | "[" expression "]"
    /// conditional-operator  ::= "&" | "|"
    /// component             ::= "name" | "number" | "group" | "provider" | "type" | "visible" | "encryption"
    /// relational-operator   ::= ":" | "<" | "<=" | "==" | "!=" | ">=" | ">"
    /// 
    /// In other words, you can use expressions like:
    /// provider : "bbc" & type : "DVB-T"                             (find all channels provided by BBC with at least one DVB-T or T2 tuning detail)
    /// provider : "bbc" & !group : "Freeview"                        (find all channels provided by BBC which are not in the Freeview group)
    /// provider : "bbc" & ![group : "Freeview" | group : "Freesat"]  (find all channels provided by BBC which are not in either the Freeview or Freesat groups)
    /// 
    /// etc. etc. etc.
    /// </remarks>
    private class Filter
    {
      #region enums

      private enum ConditionalOperator
      {
        Null,
        And,
        Or,
        Not
      }

      private enum RelationalOperator
      {
        Null,
        Contains,
        LessThan,
        LessThanOrEqual,
        Equal,
        NotEqual,
        GreaterThanOrEqual,
        GreaterThan
      }

      private sealed class Token
      {
        private readonly string _name;
        private readonly ConditionalOperator _conditionalOperator;
        private readonly RelationalOperator _relationalOperator;
        private static readonly IDictionary<string, Token> _values = new Dictionary<string, Token>();

        // syntax
        public static readonly Token BracketOpen = new Token("[");
        public static readonly Token BracketClose = new Token("]");
        public static readonly Token String = new Token("\"");
        public static readonly Token Integer = new Token("i");
        public static readonly Token BooleanTrue = new Token("true");
        public static readonly Token BooleanFalse = new Token("false");
        public static readonly Token End = new Token("x");          // end of string/expression
        public static readonly Token Component = new Token("c");    // one of "name", "type" etc.

        // conditional operators
        public static readonly Token And = new Token("&", ConditionalOperator.And);
        public static readonly Token Or = new Token("|", ConditionalOperator.Or);
        public static readonly Token Not = new Token("!", ConditionalOperator.Not);

        // relational operators
        public static readonly Token Contains = new Token(":", RelationalOperator.Contains);
        public static readonly Token LessThan = new Token("<", RelationalOperator.LessThan);
        public static readonly Token LessThanOrEqual = new Token("<=", RelationalOperator.LessThanOrEqual);
        public static readonly Token Equal = new Token("==", RelationalOperator.Equal);
        public static readonly Token NotEqual = new Token("!=", RelationalOperator.NotEqual);
        public static readonly Token GreaterThanOrEqual = new Token(">=", RelationalOperator.GreaterThanOrEqual);
        public static readonly Token GreaterThan = new Token(">", RelationalOperator.GreaterThan);

        // encryption enum
        public static readonly Token EncryptionFree = new Token("free");
        public static readonly Token EncryptionMixed = new Token("mixed");
        public static readonly Token EncryptionEncrypted = new Token("encrypted");

        private Token(string name, ConditionalOperator conditionalOperator)
          : this(name, conditionalOperator, RelationalOperator.Null)
        {
        }

        private Token(string name, RelationalOperator relationalOperator)
          : this(name, ConditionalOperator.Null, relationalOperator)
        {
        }

        private Token(string name, ConditionalOperator conditionalOperator = ConditionalOperator.Null, RelationalOperator relationalOperator = RelationalOperator.Null)
        {
          _name = name;
          _conditionalOperator = conditionalOperator;
          _relationalOperator = relationalOperator;
          _values.Add(name, this);
        }

        public ConditionalOperator ConditionalOperator
        {
          get
          {
            return _conditionalOperator;
          }
        }

        public RelationalOperator RelationalOperator
        {
          get
          {
            return _relationalOperator;
          }
        }

        public static void ReadNext(ref string s, out Token token, out string stringValue)
        {
          s = s.TrimStart();
          if (s.Length == 0)
          {
            token = Token.End;
            stringValue = null;
            return;
          }

          foreach (Token t in _values.Values)
          {
            if (t == Token.Component)
            {
              foreach (Component c in Filter.Component.Values)
              {
                if (s.StartsWith(c, StringComparison.InvariantCultureIgnoreCase))
                {
                  s = s.Substring(c.Length);
                  token = t;
                  stringValue = c;
                  return;
                }
              }
            }
            else if (t == Token.Integer)
            {
              int index = s.IndexOf(' ');
              string numericString = s;
              if (index > 0)
              {
                numericString = s.Substring(0, index);
              }
              int tempInt;
              if (int.TryParse(numericString, out tempInt))
              {
                if (index > 0)
                {
                  s = s.Substring(index);
                }
                else
                {
                  s = string.Empty;
                }
                token = Token.Integer;
                stringValue = numericString;
                return;
              }
            }
            else if (t != Token.End && s.StartsWith(t, StringComparison.InvariantCultureIgnoreCase))
            {
              if (t == Token.Not && s.StartsWith(Token.NotEqual, StringComparison.InvariantCultureIgnoreCase))
              {
                token = Token.NotEqual;
              }
              else if (t == Token.LessThan && s.StartsWith(Token.LessThanOrEqual, StringComparison.InvariantCultureIgnoreCase))
              {
                token = Token.LessThanOrEqual;
              }
              else if (t == Token.GreaterThan && s.StartsWith(Token.GreaterThanOrEqual, StringComparison.InvariantCultureIgnoreCase))
              {
                token = Token.GreaterThanOrEqual;
              }
              else
              {
                token = t;
              }
              s = s.Substring(token._name.Length);
              stringValue = token;
              if (token == Token.String)
              {
                int index = s.IndexOf(Token.String, StringComparison.InvariantCultureIgnoreCase);
                if (index < 0)
                {
                  throw new Exception("Incomplete string.");
                }
                stringValue = s.Substring(0, index);
                s = s.Substring(index + 1);
              }
              return;
            }
          }

          throw new Exception("Invalid syntax.");
        }

        public override string ToString()
        {
          return _name;
        }

        public override bool Equals(object obj)
        {
          Token token = obj as Token;
          if (token != null && this == token)
          {
            return true;
          }
          return false;
        }

        public override int GetHashCode()
        {
          return _name.GetHashCode();
        }

        public static explicit operator Token(string name)
        {
          Token value = null;
          if (!_values.TryGetValue(name, out value))
          {
            return null;
          }
          return value;
        }

        public static implicit operator string(Token token)
        {
          return token._name;
        }
      }

      internal sealed class Component
      {
        private readonly string _name;
        private static readonly IDictionary<string, Component> _values = new Dictionary<string, Component>();

        public static readonly Component Name = new Component("name");
        public static readonly Component Number = new Component("number");
        public static readonly Component Type = new Component("type");
        public static readonly Component Provider = new Component("provider");
        public static readonly Component Group = new Component("group");
        public static readonly Component Visible = new Component("visible");
        public static readonly Component Encryption = new Component("encryption");

        private Component(string name)
        {
          _name = name;
          _values.Add(name, this);
        }

        public int Length
        {
          get
          {
            return _name.Length;
          }
        }

        public static IEnumerable<Component> Values
        {
          get
          {
            return _values.Values;
          }
        }

        public override string ToString()
        {
          return _name;
        }

        public override bool Equals(object obj)
        {
          Component component = obj as Component;
          if (component != null && this == component)
          {
            return true;
          }
          return false;
        }

        public override int GetHashCode()
        {
          return _name.GetHashCode();
        }

        public static explicit operator Component(string name)
        {
          Component value = null;
          if (!_values.TryGetValue(name, out value))
          {
            return null;
          }
          return value;
        }

        public static implicit operator string(Component component)
        {
          return component._name;
        }
      }

      #endregion

      #region node interface and implementations

      public interface Node
      {
        bool Evaluate(IDictionary<Component, string> componentValues);
      }

      private class ConditionalOperationNode : Node
      {
        ConditionalOperator _operator;
        Node _operand1;
        Node _operand2;

        public ConditionalOperationNode(ConditionalOperator @operator, Node operand1, Node operand2 = null)
        {
          _operator = @operator;
          _operand1 = operand1;
          _operand2 = operand2;
        }

        public bool Evaluate(IDictionary<Component, string> componentValues)
        {
          if (_operator == ConditionalOperator.Not)
          {
            return !_operand1.Evaluate(componentValues);
          }
          if (_operator == ConditionalOperator.And)
          {
            return _operand1.Evaluate(componentValues) && _operand2.Evaluate(componentValues);
          }
          if (_operator == ConditionalOperator.Or)
          {
            return _operand1.Evaluate(componentValues) || _operand2.Evaluate(componentValues);
          }
          throw new TvException("Unexpected conditional operator '{0}'.", _operator);
        }
      }

      private class RelationalExpressionNode : Node
      {
        private RelationalOperator _operator;
        private Component _lhsComponent;
        private string _lhsValue;
        private Component _rhsComponent;
        private string _rhsValue;

        public RelationalExpressionNode(RelationalOperator @operator, Component lhsComponent, string lhsValue, Component rhsComponent, string rhsValue)
        {
          _operator = @operator;
          _lhsComponent = lhsComponent;
          _lhsValue = lhsValue;
          _rhsComponent = rhsComponent;
          _rhsValue = rhsValue;
        }

        public bool Evaluate(IDictionary<Component, string> componentValues)
        {
          if (_lhsComponent != null)
          {
            _lhsValue = componentValues[_lhsComponent];
          }
          if (_rhsComponent != null)
          {
            _rhsValue = componentValues[_rhsComponent];
          }

          if (_operator == RelationalOperator.Contains)
          {
            return _lhsValue.Contains(_rhsValue);
          }
          if (_operator == RelationalOperator.Equal)
          {
            return _lhsValue.Equals(_rhsValue);
          }
          if (_operator == RelationalOperator.NotEqual)
          {
            return !_lhsValue.Equals(_rhsValue);
          }

          int comparisonResult;
          int lhsIntValue;
          int rhsIntValue;
          if (!int.TryParse(_lhsValue, out lhsIntValue) || !int.TryParse(_rhsValue, out rhsIntValue))
          {
            comparisonResult = string.Compare(_lhsValue, _rhsValue);
          }
          else
          {
            comparisonResult = lhsIntValue - rhsIntValue;
          }

          if (_operator == RelationalOperator.LessThan)
          {
            return comparisonResult < 0;
          }
          if (_operator == RelationalOperator.LessThanOrEqual)
          {
            return comparisonResult <= 0;
          }
          if (_operator == RelationalOperator.GreaterThan)
          {
            return comparisonResult > 0;
          }
          if (_operator == RelationalOperator.GreaterThanOrEqual)
          {
            return comparisonResult >= 0;
          }

          throw new TvException("Unexpected relational operator '{0}'.", _operator);
        }
      }

      #endregion

      public static Node CreateAsnTree(string expression)
      {
        string originalExpression = string.Copy(expression);
        Node root = null;
        try
        {
          root = CreateAsnTree(ref expression);
          if (!string.IsNullOrWhiteSpace(expression))
          {
            throw new TvException("Incomplete expression, remaining = '{0}'.", expression);
          }
        }
        catch
        {
          // Invalid syntax => fall back to simple name-based filter.
          return new RelationalExpressionNode(RelationalOperator.Contains, Component.Name, null, null, originalExpression);
        }
        return root;
      }

      private static Node CreateAsnTree(ref string expression)
      {
        Token token;
        string tokenString;
        Token.ReadNext(ref expression, out token, out tokenString);

        if (token == Token.Not)
        {
          return new ConditionalOperationNode(ConditionalOperator.Not, CreateAsnTree(ref expression));
        }
        if (token == Token.BracketOpen)
        {
          Node n = CreateAsnTree(ref expression);
          Token.ReadNext(ref expression, out token, out tokenString);
          if (token != Token.BracketClose)
          {
            throw new TvException("Expected bracket closure, instead got {0}.", token);
          }
          return n;
        }
        if (token != Token.Component && token != Token.String && token != Token.Integer && token != Token.BooleanTrue && token != Token.BooleanFalse)
        {
          throw new TvException("Expected component, string, integer or boolean (LHS), instead got {0}.", token);
        }

        Token relationalToken;
        string relationalTokenString;
        Token.ReadNext(ref expression, out relationalToken, out relationalTokenString);
        if (relationalToken.RelationalOperator == RelationalOperator.Null)
        {
          throw new TvException("Expected relational operator, instead got {0}.", relationalToken);
        }

        Token rhsToken;
        string rhsTokenString;
        Token.ReadNext(ref expression, out rhsToken, out rhsTokenString);
        if (rhsToken != Token.Component && rhsToken != Token.String && rhsToken != Token.Integer && rhsToken != Token.BooleanTrue && rhsToken != Token.BooleanFalse)
        {
          throw new TvException("Expected component, string, integer or boolean (RHS), instead got {0}.", rhsToken);
        }

        Component lhsComponent = null;
        if (token == Token.Component)
        {
          lhsComponent = (Component)tokenString;
        }
        Component rhsComponent = null;
        if (rhsToken == Token.Component)
        {
          rhsComponent = (Component)rhsTokenString;
        }
        RelationalExpressionNode xn = new RelationalExpressionNode(relationalToken.RelationalOperator, lhsComponent, tokenString, rhsComponent, rhsTokenString);

        Token.ReadNext(ref expression, out token, out tokenString);
        if (token == Token.End || token == Token.BracketClose)
        {
          if (token == Token.BracketClose)
          {
            expression = Token.BracketClose + expression;
          }
          return xn;
        }
        if (token == Token.And || token == Token.Or)
        {
          return new ConditionalOperationNode(token.ConditionalOperator, xn, CreateAsnTree(ref expression));
        }

        throw new TvException("Unexpected filter format, remaining = '{0}'.", expression);
      }
    }

    #endregion

    public delegate void OnFilteringCompleted();

    #region constants

    public const int SUBITEM_INDEX_NAME = 0;
    public const int SUBITEM_INDEX_NUMBER = 1;
    public const int SUBITEM_INDEX_GROUPS = 2;

    #endregion

    #region variables

    private ListView _listView = null;
    private OnFilteringCompleted _delegateFilteringCompleted = null;
    private IDictionary<int, ListViewItem> _itemCache = new Dictionary<int, ListViewItem>(1000);
    private IDictionary<int, ChannelGroup> _channelGroups = new Dictionary<int, ChannelGroup>(50);
    private System.Windows.Forms.Timer _filterApplyTimer = null;
    private volatile string _filterExpression = string.Empty;
    private DateTime _filterExpressionChangeTime = DateTime.MinValue;
    private volatile bool _isFilling = false;

    #endregion

    public ChannelListViewHandler(ListView listView, OnFilteringCompleted delegateFilteringCompleted)
    {
      _listView = listView;
      _delegateFilteringCompleted = delegateFilteringCompleted;
      _filterApplyTimer = new System.Windows.Forms.Timer();
      _filterApplyTimer.Interval = 150;
      _filterApplyTimer.Tick += new EventHandler(FillListView);
    }

    ~ChannelListViewHandler()
    {
      if (_filterApplyTimer != null)
      {
        _filterApplyTimer.Dispose();
      }
    }

    /// <summary>
    /// Start filtering the list view.
    /// </summary>
    public void FilterListView(string filterExpression)
    {
      if (!_filterApplyTimer.Enabled)
      {
        _filterApplyTimer.Enabled = true;
        _filterApplyTimer.Start();
      }
      _filterExpression = filterExpression;
      _filterExpressionChangeTime = DateTime.Now;
    }

    /// <summary>
    /// Fill the list view with all items that match the filter.
    /// </summary>
    private void FillListView(object sender, EventArgs e)
    {
      DateTime previousFilterExpressionChange = _filterExpressionChangeTime;
      if ((DateTime.Now - previousFilterExpressionChange).TotalMilliseconds < 500)
      {
        return;
      }

      Application.UseWaitCursor = true;
      try
      {
        this.LogDebug("channels: start filtering, item count = {0}, filter = \"{1}\"", _itemCache.Count, _filterExpression);
        Filter.Node asnTreeRootNode = Filter.CreateAsnTree(_filterExpression);

        List<ListViewItem> items = new List<ListViewItem>();
        int i = 1;
        foreach (ListViewItem item in _itemCache.Values)
        {
          if (i++ % 50 == 0 && previousFilterExpressionChange != _filterExpressionChangeTime)
          {
            return;
          }

          IDictionary<Filter.Component, string> itemValues = GetFilterValuesForItem(item);
          if (asnTreeRootNode != null)
          {
            if (asnTreeRootNode.Evaluate(itemValues))
            {
              items.Add(item);
            }
          }
          else
          {
            items.Add(item);
          }
        }

        if (previousFilterExpressionChange == _filterExpressionChangeTime)
        {
          _listView.Invoke(new MethodInvoker(delegate()
          {
            _isFilling = true;
            _listView.BeginUpdate();
            try
            {
              _listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.None);
              _listView.Items.Clear();
              _listView.Items.AddRange(items.ToArray());
              _listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            }
            finally
            {
              _listView.EndUpdate();
              _isFilling = false;
            }
          }));
          _delegateFilteringCompleted();
          this.LogDebug("channels: finished filtering, visible item count = {0}", items.Count);
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "channels: failed to apply filter \"{0}\"", _filterExpression);
      }
      finally
      {
        _filterApplyTimer.Stop();
        Application.UseWaitCursor = false;
      }
    }

    private IDictionary<Filter.Component, string> GetFilterValuesForItem(ListViewItem item)
    {
      IDictionary<Filter.Component, string> itemValues = new Dictionary<Filter.Component, string>(20);
      itemValues[Filter.Component.Visible] = item.Checked.ToString().ToLowerInvariant();
      itemValues[Filter.Component.Name] = item.Text;
      itemValues[Filter.Component.Number] = item.SubItems[SUBITEM_INDEX_NUMBER].Text;
      itemValues[Filter.Component.Group] = item.SubItems[2].Text;
      itemValues[Filter.Component.Provider] = item.SubItems[3].Text;
      itemValues[Filter.Component.Type] = item.SubItems[4].Text;
      itemValues[Filter.Component.Encryption] = item.SubItems[3].Tag as string;
      return itemValues;
    }

    public bool IsFilling
    {
      get
      {
        return _isFilling;
      }
    }

    public Channel GetChannelForItem(ListViewItem item)
    {
      return item.Tag as Channel;
    }

    public ListViewItem GetItemForChannel(Channel channel)
    {
      return _itemCache[channel.IdChannel];
    }

    public ICollection<ListViewItem> AllItems
    {
      get
      {
        return _itemCache.Values;
      }
    }

    public bool IsChannelInGroup(ListViewItem item, ChannelGroup group)
    {
      HashSet<int> groupIds = item.SubItems[2].Tag as HashSet<int>;
      return groupIds != null && groupIds.Contains(group.IdGroup);
    }

    public void AddOrUpdateChannels(IEnumerable<Channel> channels)
    {
      foreach (Channel channel in channels)
      {
        ListViewItem item = new ListViewItem(channel.Name);
        item.Checked = channel.VisibleInGuide;
        item.Tag = channel;
        item.SubItems.Add(channel.ChannelNumber.ToString());

        HashSet<int> groupIds = new HashSet<int>();
        SortedSet<string> groupNames = new SortedSet<string>(StringComparer.InvariantCultureIgnoreCase);
        foreach (GroupMap gm in channel.GroupMaps)
        {
          groupIds.Add(gm.IdGroup);
          groupNames.Add(_channelGroups[gm.IdGroup].GroupName);
        }
        ListViewItem.ListViewSubItem subItem = item.SubItems.Add(string.Join(", ", groupNames));
        subItem.Tag = groupIds;

        SortedSet<string> providers = new SortedSet<string>(StringComparer.InvariantCultureIgnoreCase);
        SortedDictionary<string, BroadcastStandard> tuningDetailBroadcastStandardNames = new SortedDictionary<string, BroadcastStandard>(StringComparer.InvariantCultureIgnoreCase);
        IDictionary<BroadcastStandard, int> tuningDetailBroadcastStandardCounts = new Dictionary<BroadcastStandard, int>(10);
        bool hasFta = false;
        bool hasScrambled = false;
        foreach (TuningDetail tuningDetail in channel.TuningDetails)
        {
          if (!string.IsNullOrEmpty(tuningDetail.Provider))
          {
            providers.Add(tuningDetail.Provider);
          }

          int count = 0;
          BroadcastStandard tuningDetailBroadcastStandard = (BroadcastStandard)tuningDetail.BroadcastStandard;
          if (!tuningDetailBroadcastStandardCounts.TryGetValue(tuningDetailBroadcastStandard, out count))
          {
            tuningDetailBroadcastStandardCounts.Add(tuningDetailBroadcastStandard, 1);
            tuningDetailBroadcastStandardNames.Add(tuningDetailBroadcastStandard.GetDescription(), tuningDetailBroadcastStandard);
          }
          else
          {
            tuningDetailBroadcastStandardCounts[tuningDetailBroadcastStandard] = ++count;
          }

          if (tuningDetail.IsEncrypted)
          {
            hasScrambled = true;
          }
          else
          {
            hasFta = true;
          }
        }

        subItem = item.SubItems.Add(string.Join(", ", providers));

        if (tuningDetailBroadcastStandardCounts.Count == 0)
        {
          item.SubItems.Add("(no tuning details)");
        }
        else
        {
          List<string> sections = new List<string>(tuningDetailBroadcastStandardNames.Count);
          foreach (KeyValuePair<string, BroadcastStandard> broadcastStandard in tuningDetailBroadcastStandardNames)
          {
            sections.Add(string.Format("{0} ({1})", broadcastStandard.Key, tuningDetailBroadcastStandardCounts[broadcastStandard.Value]));
          }
          item.SubItems.Add(string.Join(", ", sections));
        }

        int imageIndex = 0;
        if (channel.MediaType == (int)MediaType.Television)
        {
          if (hasFta && hasScrambled)
          {
            imageIndex = 5;
            subItem.Tag = "mixed";
          }
          else if (hasScrambled)
          {
            imageIndex = 4;
            subItem.Tag = "encrypted";
          }
          else
          {
            imageIndex = 3;
            subItem.Tag = "free";
          }
        }
        else if (channel.MediaType == (int)MediaType.Radio)
        {
          if (hasFta && hasScrambled)
          {
            imageIndex = 2;
            subItem.Tag = "mixed";
          }
          else if (hasScrambled)
          {
            imageIndex = 1;
            subItem.Tag = "encrypted";
          }
          else
          {
            imageIndex = 0;
            subItem.Tag = "free";
          }
        }
        item.ImageIndex = imageIndex;

        _itemCache[channel.IdChannel] = item;
      }

      FillListView(null, null);
    }

    public void DeleteChannels(IEnumerable<ListViewItem> items, out ICollection<Channel> channels)
    {
      channels = new List<Channel>(_listView.Items.Count);
      foreach (ListViewItem item in items)
      {
        Channel channel = item.Tag as Channel;
        if (channel != null)
        {
          channels.Add(channel);
          _itemCache.Remove(channel.IdChannel);
        }
        item.Remove();
      }
    }

    public void AddChannelsToGroup(IEnumerable<GroupMap> mappings)
    {
      bool haveChannelInGroup = false;
      foreach (GroupMap mapping in mappings)
      {
        ListViewItem item;
        if (_itemCache.TryGetValue(mapping.IdChannel, out item))
        {
          haveChannelInGroup = true;
          ListViewItem.ListViewSubItem subItem = item.SubItems[SUBITEM_INDEX_GROUPS];
          HashSet<int> groupIds = subItem.Tag as HashSet<int>;
          groupIds.Add(mapping.IdGroup);
          SortedSet<string> groupNames = new SortedSet<string>(StringComparer.InvariantCultureIgnoreCase);
          foreach (int groupId in groupIds)
          {
            groupNames.Add(_channelGroups[groupId].GroupName);
          }
          subItem.Text = string.Join(", ", groupNames);
          subItem.Tag = groupIds;

          Channel channel = item.Tag as Channel;
          channel.GroupMaps.Add(mapping);
          channel.AcceptChanges();
          item.Tag = channel;
        }
      }

      if (haveChannelInGroup)
      {
        FillListView(null, null);
      }
    }

    public void RemoveChannelsFromGroup(IEnumerable<GroupMap> mappings)
    {
      bool haveChannelInGroup = false;
      foreach (GroupMap mapping in mappings)
      {
        ListViewItem item;
        if (!_itemCache.TryGetValue(mapping.IdChannel, out item))
        {
          continue;
        }

        ListViewItem.ListViewSubItem subItem = item.SubItems[SUBITEM_INDEX_GROUPS];
        HashSet<int> groupIds = subItem.Tag as HashSet<int>;
        if (groupIds.Remove(mapping.IdGroup))
        {
          haveChannelInGroup = true;

          SortedSet<string> groupNames = new SortedSet<string>(StringComparer.InvariantCultureIgnoreCase);
          foreach (int groupId in groupIds)
          {
            groupNames.Add(_channelGroups[groupId].GroupName);
          }
          subItem.Text = string.Join(", ", groupNames);
          subItem.Tag = groupIds;

          Channel channel = item.Tag as Channel;
          for (int i = channel.GroupMaps.Count - 1; i >= 0; i--)
          {
            if (channel.GroupMaps[i].IdGroup == mapping.IdGroup)
            {
              channel.GroupMaps.RemoveAt(i);
              break;
            }
          }
          channel.AcceptChanges();
          item.Tag = channel;
        }
      }

      if (haveChannelInGroup)
      {
        FillListView(null, null);
      }
    }

    public void AddGroup(ChannelGroup group)
    {
      _channelGroups[group.IdGroup] = group;
    }

    public void AddOrUpdateGroup(ChannelGroup group)
    {
      try
      {
        if (!_channelGroups.ContainsKey(group.IdGroup))
        {
          return;
        }
      }
      finally
      {
        _channelGroups[group.IdGroup] = group;
      }

      bool haveChannelInGroup = false;
      foreach (ListViewItem item in _itemCache.Values)
      {
        ListViewItem.ListViewSubItem subItem = item.SubItems[SUBITEM_INDEX_GROUPS];
        HashSet<int> groupIds = subItem.Tag as HashSet<int>;
        if (groupIds.Contains(group.IdGroup))
        {
          haveChannelInGroup = true;
          SortedSet<string> groupNames = new SortedSet<string>(StringComparer.InvariantCultureIgnoreCase);
          foreach (int groupId in groupIds)
          {
            groupNames.Add(_channelGroups[groupId].GroupName);
          }
          subItem.Text = string.Join(", ", groupNames);
          subItem.Tag = groupIds;
        }
      }

      if (haveChannelInGroup)
      {
        FillListView(null, null);
      }
    }

    public void DeleteGroup(ChannelGroup group)
    {
      _channelGroups.Remove(group.IdGroup);

      bool haveChannelInGroup = false;
      foreach (ListViewItem item in _itemCache.Values)
      {
        ListViewItem.ListViewSubItem subItem = item.SubItems[SUBITEM_INDEX_GROUPS];
        HashSet<int> groupIds = subItem.Tag as HashSet<int>;
        if (groupIds.Remove(group.IdGroup))
        {
          haveChannelInGroup = true;

          SortedSet<string> groupNames = new SortedSet<string>(StringComparer.InvariantCultureIgnoreCase);
          foreach (int groupId in groupIds)
          {
            groupNames.Add(_channelGroups[groupId].GroupName);
          }
          subItem.Text = string.Join(", ", groupNames);
          subItem.Tag = groupIds;

          Channel channel = item.Tag as Channel;
          for (int i = channel.GroupMaps.Count - 1; i >= 0; i--)
          {
            if (channel.GroupMaps[i].IdGroup == group.IdGroup)
            {
              channel.GroupMaps.RemoveAt(i);
              break;
            }
          }
          channel.AcceptChanges();
          item.Tag = channel;
        }
      }

      if (haveChannelInGroup)
      {
        FillListView(null, null);
      }
    }
  }
}