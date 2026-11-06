namespace JSH.UI
{
    /// <summary>
    /// Short tags for dialogue writers (CSV Text / ChoiceA / ChoiceB), expanded to Text Animator tags before display.
    ///   &lt;angry&gt;...&lt;/angry&gt; -> shaking text (Text Animator &lt;shake&gt;)
    /// Any Text Animator tag (&lt;shake&gt;, &lt;wave&gt;, &lt;bounce&gt;, &lt;wiggle&gt;, &lt;speed=0.5&gt;, ...) and TMP tags
    /// (&lt;b&gt;, &lt;color=red&gt;) can also be written directly. Called once per line (allocates; not per frame).
    /// </summary>
    public static class DialogueTextTags
    {
        public static string Expand(string text)
        {
            if (string.IsNullOrEmpty(text) || text.IndexOf('<') < 0) return text;
            return text
                .Replace("<angry>", "<shake>")
                .Replace("</angry>", "</shake>");
        }
    }
}
