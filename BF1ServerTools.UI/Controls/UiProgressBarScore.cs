namespace BF1ServerTools.UI.Controls;

public class UiProgressBarScore: ProgressBar
{
    /// <summary>
    /// 阵营图标
    /// </summary>
    public string Team
    {
        get { return (string)GetValue(TeamProperty); }
        set { SetValue(TeamProperty, value); }
    }
    public static readonly DependencyProperty TeamProperty =
        DependencyProperty.Register("Team", typeof(string), typeof(UiProgressBarScore), new PropertyMetadata(default));

    /// <summary>
    /// 阵营分数
    /// </summary>
    public string Score
    {
        get { return (string)GetValue(ScoreProperty); }
        set { SetValue(ScoreProperty, value); }
    }
    public static readonly DependencyProperty ScoreProperty =
        DependencyProperty.Register("Score", typeof(string), typeof(UiProgressBarScore), new PropertyMetadata(default));
}
