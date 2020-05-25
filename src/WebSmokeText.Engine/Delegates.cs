using SmokeTest.Shared.Engine;

namespace SmokeTest.Engine
{

    #region Delegates

    /// <summary>
    /// Event Handler when Error Occurs
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void ErrorEventHandler(object sender, ErrorData e);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void MailErrorEventHandler(object sender, WebMonitorEmailExceptionArgs e);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void PageParsedEventHandler(object sender, WebMonitorPageParsedArgs e);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void CookieFoundEventHandler(object sender, WebMonitorCookieArgs e);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void BeforePageParsed(object sender, WebMonitorBeforePageParseArgs e);

    #endregion Delegates
}
