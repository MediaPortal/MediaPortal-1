using System;
using System.Text;
using System.Xml.Serialization;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Setting
{
  [Serializable]
  public class TextProgressBar : Value
  {
    private char endChar;
    private char fillChar;
    private int length;
    private char startChar;
    private Property targetProperty;
    private char valueChar;
    private Property valueProperty;

    public TextProgressBar()
    {
      this.startChar = '[';
      this.endChar = ']';
      this.valueChar = '#';
      this.fillChar = '-';
      this.length = 8;
    }

    public TextProgressBar(string valueProperty, string targetProperty, int length)
    {
      this.startChar = '[';
      this.endChar = ']';
      this.valueChar = '#';
      this.fillChar = '-';
      this.length = 8;
      this.length = length;
      this.valueProperty = new Property(valueProperty);
      this.targetProperty = new Property(targetProperty);
    }

    private double ConvertToInt(string stringValue)
    {
      double num;
      if ((stringValue == null) || (stringValue.Length == 0))
      {
        num = 0.0;
      }
      if (stringValue.Contains(":"))
      {
        int num2;
        int num3;
        int num4;
        string[] strArray = stringValue.Split(new char[] {':'});
        if (strArray.Length == 3)
        {
          num2 = int.Parse(strArray[0]);
          num3 = int.Parse(strArray[1]);
          num4 = int.Parse(strArray[2]);
        }
        else if (strArray.Length == 2)
        {
          num2 = 0;
          num3 = int.Parse(strArray[0]);
          num4 = int.Parse(strArray[1]);
        }
        else
        {
          return 0.0;
        }
        return (double)(((num2 * 0xe10) + (num3 * 60)) + num4);
      }
      if (!double.TryParse(stringValue, out num))
      {
        num = Convert.ToInt32(stringValue);
      }
      return num;
    }

    protected override string DoEvaluate()
    {
      int num3;
      double num = this.ConvertToInt(this.valueProperty.Evaluate());
      double num2 = this.ConvertToInt(this.targetProperty.Evaluate());
      StringBuilder builder = new StringBuilder(this.length);
      if (num2 == 0.0)
      {
        num3 = 0;
        return "[TextProgressBar Error]";
      }
      num3 = (num <= 0.0) ? ((int)0.0) : ((int)((num / num2) * (this.length - 2)));
      builder.Append(this.startChar);
      builder.Append(this.valueChar, num3);
      builder.Append(this.fillChar, (this.length - 2) - num3);
      builder.Append(this.endChar);
      return builder.ToString();
    }

    [XmlAttribute]
    public string EndChar
    {
      get { return new string(this.endChar, 1); }
      set
      {
        if ((value != null) && (value.Length > 0))
        {
          this.endChar = value[0];
        }
      }
    }

    [XmlAttribute]
    public string FillChar
    {
      get { return new string(this.fillChar, 1); }
      set
      {
        if ((value != null) && (value.Length > 0))
        {
          this.fillChar = value[0];
        }
      }
    }

    [XmlAttribute]
    public int Length
    {
      get { return this.length; }
      set { this.length = value; }
    }

    [XmlAttribute]
    public string StartChar
    {
      get { return new string(this.startChar, 1); }
      set
      {
        if ((value != null) && (value.Length > 0))
        {
          this.startChar = value[0];
        }
      }
    }

    [XmlElement]
    public Property TargetProperty
    {
      get { return this.targetProperty; }
      set { this.targetProperty = value; }
    }

    [XmlAttribute]
    public string ValueChar
    {
      get { return new string(this.valueChar, 1); }
      set
      {
        if ((value != null) && (value.Length > 0))
        {
          this.valueChar = value[0];
        }
      }
    }

    [XmlElement]
    public Property ValueProperty
    {
      get { return this.valueProperty; }
      set { this.valueProperty = value; }
    }
  }
}