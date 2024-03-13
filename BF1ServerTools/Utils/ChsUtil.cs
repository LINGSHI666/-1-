using Chinese;
using System;

namespace BF1ServerTools.Utils;

public static class ChsUtil
{
    /// <summary>
    /// 美式键盘
    /// </summary>
    private static readonly CultureInfo ENUS = new("en-US");
    /// <summary>
    /// 微软拼音
    /// </summary>
    private static readonly CultureInfo ZHCN = new("zh-CN");

    /// <summary>
    /// 字符串简体转繁体
    /// </summary>
    /// <param name="strSimple"></param>
    /// <returns></returns>
    public static string ToTraditional(string strSimple)
    {
        return ChineseConverter.ToTraditional(strSimple);
    }

    /// <summary>
    /// 字符串繁体转简体
    /// </summary>
    /// <param name="strTraditional"></param>
    /// <returns></returns>
    public static string ToSimplified(string strTraditional)
    {
        return ChineseConverter.ToSimplified(strTraditional);
    }

    /// <summary>
    /// 设置输入法为美式键盘
    /// </summary>
    public static void SetInputLanguageENUS()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            InputLanguageManager.Current.CurrentInputLanguage = ENUS;
        });
    }

    /// <summary>
    /// 设置输入法为微软拼音
    /// </summary>
    public static void SetInputLanguageZHCN()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            InputLanguageManager.Current.CurrentInputLanguage = ZHCN;
        });
    }
}
