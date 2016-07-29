using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace VKClient.Common.UC
{
  [TemplatePart(Name = "PART_rectangleFill", Type = typeof (Rectangle))]
  [TemplatePart(Name = "PART_textBlockAnswer", Type = typeof (TextBlock))]
  [TemplatePart(Name = "PART_textBlockPercentage", Type = typeof (TextBlock))]
  public class PollAnswerControl : Control
  {
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof (double), typeof (PollAnswerControl), new PropertyMetadata(new PropertyChangedCallback(PollAnswerControl.Value_OnChanged)));
    public static readonly DependencyProperty RelativePercentageProperty = DependencyProperty.Register("RelativePercentage", typeof (double), typeof (PollAnswerControl), new PropertyMetadata(new PropertyChangedCallback(PollAnswerControl.RelativePercentage_OnChanged)));
    public static readonly DependencyProperty AbsolutePercentageProperty = DependencyProperty.Register("AbsolutePercentage", typeof (double), typeof (PollAnswerControl), new PropertyMetadata(new PropertyChangedCallback(PollAnswerControl.AbsolutePercentage_OnChanged)));
    public static readonly DependencyProperty AnswerProperty = DependencyProperty.Register("Answer", typeof (string), typeof (PollAnswerControl), new PropertyMetadata(new PropertyChangedCallback(PollAnswerControl.Answer_OnChanged)));
    private const string RECTANGLE_FILL_NAME = "PART_rectangleFill";
    private const string TEXTBLOCK_ANSWER_NAME = "PART_textBlockAnswer";
    private const string TEXTBLOCK_PERCENTAGE_NAME = "PART_textBlockPercentage";
    private Rectangle _rectangleFill;
    private TextBlock _textBlockAnswer;
    private TextBlock _textBlockPercentage;

    public double Value
    {
      get
      {
        return (double) this.GetValue(PollAnswerControl.ValueProperty);
      }
      set
      {
        this.SetValue(PollAnswerControl.ValueProperty, (object) value);
      }
    }

    public double RelativePercentage
    {
      get
      {
        return (double) this.GetValue(PollAnswerControl.RelativePercentageProperty);
      }
      set
      {
        this.SetValue(PollAnswerControl.RelativePercentageProperty, (object) value);
      }
    }

    public double AbsolutePercentage
    {
      get
      {
        return (double) this.GetValue(PollAnswerControl.AbsolutePercentageProperty);
      }
      set
      {
        this.SetValue(PollAnswerControl.AbsolutePercentageProperty, (object) value);
      }
    }

    public string Answer
    {
      get
      {
        return (string) this.GetValue(PollAnswerControl.AnswerProperty);
      }
      set
      {
        this.SetValue(PollAnswerControl.AnswerProperty, (object) value);
      }
    }

    public PollAnswerControl()
    {
      this.SizeChanged += new SizeChangedEventHandler(this.OnSizeChanged);
    }

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();
      this._rectangleFill = this.GetTemplateChild("PART_rectangleFill") as Rectangle;
      this._textBlockAnswer = this.GetTemplateChild("PART_textBlockAnswer") as TextBlock;
      this._textBlockPercentage = this.GetTemplateChild("PART_textBlockPercentage") as TextBlock;
    }

    private static void Value_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((PollAnswerControl) d).UpdateValues();
    }

    private void UpdateValues()
    {
      if (this._rectangleFill == null)
        return;
      this._rectangleFill.Width = this.ActualWidth * this.Value / 100.0;
    }

    private static void RelativePercentage_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }

    private static void AbsolutePercentage_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }

    private static void Answer_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((PollAnswerControl) d).UpdateAnswer();
    }

    private void UpdateAnswer()
    {
      if (this._textBlockAnswer == null)
        return;
      this._textBlockAnswer.Text = this.Answer;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      this.UpdateValues();
    }
  }
}
