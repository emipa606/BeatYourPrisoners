using System.Diagnostics;
using System.Text;
using Verse;

namespace CM_Beat_Prisoners;

public static class Logger
{
    private const bool MessageEnabled = false;
    private const bool WarningEnabled = true;
    private const bool ErrorEnabled = true;

    private static bool messageInProgress;

    private static StringBuilder messageBuilder = new();

    public static void MessageFormat(object caller, string message, params object[] stuff)
    {
        if (!MessageEnabled)
        {
            return;
        }

        message = $"{caller.GetType()}.{new StackTrace().GetFrame(1).GetMethod().Name} - {message}";
        Log.Message(string.Format(message, stuff));
    }

    // Building and displaying message assumes caller will be checking for MessageEnabled, WarningEnabled or ErrorEnabled
    public static void StartMessage(object caller, string message, params object[] stuff)
    {
        if (!MessageEnabled)
        {
            return;
        }

        messageInProgress = true;
        messageBuilder =
            new StringBuilder($"{caller.GetType()}.{new StackTrace().GetFrame(1).GetMethod().Name}: ");
        if (!string.IsNullOrEmpty(message))
        {
            AddToMessage(message, stuff);
        }
    }

    public static void AddToMessage(string message, params object[] stuff)
    {
        if (messageInProgress)
        {
            messageBuilder.AppendLine(string.Format(message, stuff));
        }
    }

    public static void DisplayMessage()
    {
        if (!messageInProgress)
        {
            return;
        }

        messageInProgress = false;
        Log.Message(messageBuilder.ToString());
    }
}