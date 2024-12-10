using System;

namespace QuestPatcher.Core.Downgrading
{
    public class DowngradeException: Exception
    {
        public DowngradeException(string message) : base(message)
        {
        }
        
        public DowngradeException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
