using System.Linq;

namespace ArgumentClinic
{
    /// <summary>
    /// Helper methods for the entire program
    /// </summary>
    public static class Global
    {
        /// <summary>
        /// Return the raw alphanumeric string to use as an argument flag
        /// </summary>
        /// <param name="rawArg"></param>
        /// <returns></returns>
        public static string CleanFlag(string rawArg)
        {
            string trimmedArg = rawArg.TrimStart('-');
            char[] alphaArr = rawArg.ToCharArray().Where(c => char.IsLetterOrDigit(c)).ToArray();
            string cleanedArg = "";
            foreach(char c in alphaArr)
            {
                cleanedArg += c;
            }
            if(cleanedArg.Length == 0)
            {
                throw new System.Exception("Supplied argument must be at least one alphanumeric character");
            }
            return cleanedArg;
        }
    }
}
