namespace ArgumentClinic
{
    public class Switch
    {
        private string shortForm;
        private string longForm;

        public Switch(string shortArg)
        {
            this.shortForm = Global.CleanFlag(shortArg);
            if(this.shortForm.Length != 1)
            {
                throw new System.Exception("Short flag must be one alphanumeric character");
            }
        }

        public Switch(string shortArg, string longArg)
        {
            this.shortForm = Global.CleanFlag(shortArg);
            if (this.shortForm.Length != 1)
            {
                throw new System.Exception("Short flag must be one alphanumeric character");
            }
            this.longForm = longArg.Trim('-');
        }

        public bool ParseArgs(string[] args)
        {
            for (int idx = 0; idx < args.Length; idx++)
            {
                string cleanFlag = Global.CleanFlag(args[idx]);
                if(shortForm == cleanFlag || longForm == args[idx].Trim('-'))
                    return true;
            }
            return false;
        }
    }
}
