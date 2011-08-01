using TShockAPI;
using System.Collections.Generic;

namespace SignControl
{
    public class SPlayer : TSPlayer
    {
        protected SettingState State = SettingState.None;
        public string PasswordForSign = "";
        protected List<int> UnlockedSigns = new List<int>();

        public SPlayer(int index)
            : base(index)
        {
        }

        public SettingState getState()
        {
            return State;
        }

        public void setState(SettingState state)
        {
            State = state;
        }

        public void addSignAccess(int id)
        {
            UnlockedSigns.Add(id);
            PasswordForSign = "";
        }

        public void removeSignAccess(int id)
        {
            if (UnlockedSigns.Contains(id))
            {
                UnlockedSigns.Remove(id);
            }

            PasswordForSign = "";
        }


        public bool canEditSign(int id)
        {
            if (UnlockedSigns.Contains(id))
            {
                return true;
            }
            return false;
        }
    }

    public enum SettingState
    {
        None,
        Setting,
        Deleting,
        UnLocking
    }
}

