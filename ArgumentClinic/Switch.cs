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
            this.shortForm = shortArg;
            if (this.shortForm.Length != 1)
            {
                throw new System.Exception("Short flag must be one alphanumeric character");
            }
            this.longForm = longArg;
        }

        public bool ParseArgs(string[] args)
        {
            for (int idx = 0; idx < args.Length - 1; idx++)
            {
                string cleanFlag = Global.CleanFlag(args[idx]);
                if(shortForm == cleanFlag || longForm == cleanFlag)
                    return true;
            }
            return false;
        }
    }
}
