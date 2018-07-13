using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Classic.FormFlow.Advanced
{
    internal class MessageActivityHelper
    {
        internal static string GetSanitizedTextInput(MessageActivity activity)
        {
            var text = (activity != null ? activity.Text : null);

            var result = text == null ? "" : text.Trim();
            if (result.StartsWith("\""))
            {
                result = result.Substring(1);
            }
            if (result.EndsWith("\""))
            {
                result = result.Substring(0, result.Length - 1);
            }

            return result;
        }
    }
}
