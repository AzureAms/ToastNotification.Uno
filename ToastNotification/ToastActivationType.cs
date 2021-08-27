namespace Uno.Extras
{
    public enum ToastActivationType
    {
        /// <summary>
        /// Your corresponding background task (assuming you set everything up) is triggered, and you can execute code in the background (like sending the user's quick reply message) without interrupting the user.
        /// </summary>
        Foreground = 0,
        /// <summary>
        /// Default value. Your foreground app is launched.
        /// </summary>
        Background = 1,
        /// <summary>
        /// Launch a different app using protocol activation.
        /// </summary>
        Protocol = 2
    }
}