using System.Diagnostics;
using System.Text;
using Verse;

namespace CM_Beat_Prisoners;

public static class Logger
{
    public static readonly bool MessageEnabled = false;
    public static readonly bool WarningEnabled = true;
    public static readonly bool ErrorEnabled = true;

    public static bool MessageInProgress;

    public static StringBuilder messageBuilder = new StringBuilder();

    public static void MessageNoCaller(string message)
    {
        if (!MessageEnabled)
        {
            return;
        }

        message = $"{new StackTrace().GetFrame(1).GetMethod().Name} - {message}";
        Log.Message(message);
    }

    public static void MessageFormat(object caller, string message, params object[] stuff)
    {
        if (!MessageEnabled)
        {
            return;
        }

        message = $"{caller.GetType()}.{new StackTrace().GetFrame(1).GetMethod().Name} - {message}";
        Log.Message(string.Format(message, stuff));
    }

    public static void MessageFormat(string message, params object[] stuff)
    {
        if (!MessageEnabled)
        {
            return;
        }

        message = $"{new StackTrace().GetFrame(1).GetMethod().Name} - {message}";
        Log.Message(string.Format(message, stuff));
    }

    public static void WarningFormat(object caller, string message, params object[] stuff)
    {
        if (!WarningEnabled)
        {
            return;
        }

        message = $"{caller.GetType()}.{new StackTrace().GetFrame(1).GetMethod().Name} - {message}";
        Log.Warning(string.Format(message, stuff));
    }

    public static void ErrorFormat(object caller, string message, params object[] stuff)
    {
        if (!ErrorEnabled)
        {
            return;
        }

        message = $"{caller.GetType()}.{new StackTrace().GetFrame(1).GetMethod().Name} - {message}";
        Log.Error(string.Format(message, stuff));
    }

    // Building and displaying message assumes caller will be checking for MessageEnabled, WarningEnabled or ErrorEnabled
    public static void StartMessage(object caller, string message, params object[] stuff)
    {
        if (!MessageEnabled)
        {
            return;
        }

        MessageInProgress = true;
        messageBuilder =
            new StringBuilder($"{caller.GetType()}.{new StackTrace().GetFrame(1).GetMethod().Name}: ");
        if (!string.IsNullOrEmpty(message))
        {
            AddToMessage(message, stuff);
        }
    }

    public static void AddToMessage(string message, params object[] stuff)
    {
        if (MessageInProgress)
        {
            messageBuilder.AppendLine(string.Format(message, stuff));
        }
    }

    public static void DisplayMessage()
    {
        if (!MessageInProgress)
        {
            return;
        }

        MessageInProgress = false;
        Log.Message(messageBuilder.ToString());
    }

    public static void DisplayWarning()
    {
        if (!MessageInProgress)
        {
            return;
        }

        MessageInProgress = false;
        Log.Warning(messageBuilder.ToString());
    }

    public static void DisplayError()
    {
        if (!MessageInProgress)
        {
            return;
        }

        MessageInProgress = false;
        Log.Error(messageBuilder.ToString());
    }
}